/*
 * Copyright (c) 2017 Sveriges Radio AB, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CCM.Core.Entities;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Interfaces.Repositories.Specialized;
using CCM.Web.Authentication;
using CCM.Web.Infrastructure;
using CCM.Web.Models.Regions;

namespace CCM.Web.Controllers
{
    [CcmAuthorize(Roles = "Admin, Remote")]
    public class RegionsController : BaseController
    {
        private readonly IRegionRepository _regionRepository;
        private readonly ILocationInfoRepository _locationInfoRepository;

        public RegionsController(IRegionRepository regionRepository, ILocationInfoRepository locationInfoRepository)
        {
            _regionRepository = regionRepository;
            _locationInfoRepository = locationInfoRepository;
        }

        public ActionResult Index(string search = "")
        {
            var model = new List<RegionViewModel>();
            List<Region> regions = string.IsNullOrWhiteSpace(search) ? _regionRepository.GetAll() : _regionRepository.FindRegions(search);

            foreach (Region region in regions)
            {
                var regionViewModel = new RegionViewModel
                {
                    Id = region.Id,
                    Name = region.Name,
                    Locations = new List<LocationViewModel>()
                };

                foreach (var location in region.Locations) // <-- alltid tom lista? ta bort.
                {
                    regionViewModel.Locations.Add(new LocationViewModel()
                    {
                        Id = location.Id,
                        Name = location.Name
                    });
                }

                model.Add(regionViewModel);
            }

            ViewBag.SearchString = search;
            return View(model);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Create()
        {
            var model = new RegionViewModel
            {
                Locations = _locationInfoRepository.GetAll().Select(location => new LocationViewModel { Id = location.Id, Name = location.Name }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Create(RegionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var region = ViewModelToRegion(model);
                region.CreatedBy = User.Identity.Name;

                _regionRepository.Save(region);

                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Edit(Guid id)
        {
            var region = _regionRepository.GetById(id);
            if (region == null)
            {
                return RedirectToAction("Index");
            }

            var model = RegionToViewModel(region);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Edit(RegionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var region = ViewModelToRegion(model);
                _regionRepository.Save(region);

                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Delete(Guid id)
        {
            var region = _regionRepository.GetById(id);
            if (region == null)
            {
                return RedirectToAction("Index");
            }

            return View(region);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Delete(Region region)
        {
            _regionRepository.Delete(region.Id);
            return RedirectToAction("Index");
        }

        private RegionViewModel RegionToViewModel(Region region)
        {
            var model = new RegionViewModel
            {
                Id = region.Id,
                Name = region.Name,
                Locations = _locationInfoRepository.GetAll().Select(location => new LocationViewModel { Id = location.Id,Name = location.Name }).ToList()
            };

            foreach (var location in region.Locations)
            {
                var modelLocation = model.Locations.SingleOrDefault(l => l.Id == location.Id);
                if (modelLocation != null)
                {
                    modelLocation.Selected = true;
                }
            }

            return model;
        }

        private Region ViewModelToRegion(RegionViewModel model)
        {
            return new Region
            {
                Id = model.Id,
                Name = model.Name,
                UpdatedBy = User.Identity.Name,
                Locations = model.Locations
                    .Where(vm => vm.Selected)
                    .Select(vm => new Location {Id = vm.Id, Name = vm.Name})
                    .ToList()
            };
        }
    }
}