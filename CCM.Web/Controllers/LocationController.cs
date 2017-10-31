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
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using CCM.Core.Entities;
using CCM.Core.Entities.Specific;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Repositories;
using CCM.Web.Authentication;
using CCM.Web.Infrastructure;
using CCM.Web.Models.Location;

namespace CCM.Web.Controllers
{
    [CcmAuthorize(Roles = "Admin, Remote")]
    public class LocationController : BaseController
    {
        private readonly ICityRepository _cityRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IRegionRepository _regionRepository;
        private readonly IProfileGroupRepository _profileGroupRepository;

        public LocationController(ILocationRepository locationManager,
            IRegionRepository regionRepository,
            ICityRepository cityRepository,
            IProfileGroupRepository profileGroupRepository)
        {
            _locationRepository = locationManager;
            _regionRepository = regionRepository;
            _cityRepository = cityRepository;
            _profileGroupRepository = profileGroupRepository;
        }

        public ActionResult Index(LocationIndexViewModel model = null)
        {
            if (model == null)
            {
                model = new LocationIndexViewModel();
            }

            model.Locations = string.IsNullOrWhiteSpace(model.Search) ? _locationRepository.GetAll() : _locationRepository.FindLocations(model.Search);
            model.Locations = model.Locations.OrderBy(m => m.Name).ToList();

            if (model.SortBy == 0)
            {
                model.Locations = model.Direction == 0 ?
                    model.Locations.OrderBy(l => l.Name).ToList() :
                    model.Locations.OrderByDescending(l => l.Name).ToList();
            }
            else if (model.SortBy == 1)
            {
                model.Locations = model.Direction == 0 ?
                    model.Locations.OrderBy(l => l.Net, new IpAddressComparer()).ThenBy(l => l.Cidr).ToList() :
                    model.Locations.OrderByDescending(l => l.Net_v6, new IpAddressComparer()).ThenByDescending(l => l.Cidr).ToList();
            }
            else if (model.SortBy == 2)
            {
                model.Locations = model.Direction == 0 ?
                    model.Locations.OrderBy(l => l.Net_v6, new IpAddressComparer()).ThenBy(l => l.Cidr_v6).ToList() :
                    model.Locations.OrderByDescending(l => l.Net_v6, new IpAddressComparer()).ThenByDescending(l => l.Cidr_v6).ToList();
            }

            return View(model);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Create()
        {
            var model = new LocationViewModel();
            PopulateViewModel(model, null);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Create(LocationViewModel model)
        {
            if (ModelState.IsValid)
            {
                Location location = GetLocation(model);
                location.CreatedBy = User.Identity.Name;

                _locationRepository.Save(location);
                return RedirectToAction("Index");
            }

            // TODO: Populate view model

            return View(model);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Edit(Guid id)
        {
            Location location = _locationRepository.GetById(id);
            if (location == null)
            {
                return RedirectToAction("Index");
            }

            var model = new LocationViewModel
            {
                Id = location.Id,
                Name = location.Name,
                ShortName = location.ShortName,
                Net = location.Net,
                Cidr = location.Cidr,
                NetV6 = location.Net_v6,
                CidrV6 = location.Cidr_v6,
                CarrierConnectionId = location.CarrierConnectionId,
                Comment = location.Comment,
                Region = location.Region != null ? location.Region.Id : Guid.Empty,
                City = location.City != null ? location.City.Id : Guid.Empty,
                ProfileGroup = location.ProfileGroup != null ? location.ProfileGroup.Id : Guid.Empty
            };

            PopulateViewModel(model, location);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Edit(LocationViewModel model)
        {
            Location location = GetLocation(model);

            if (ModelState.IsValid)
            {
                _locationRepository.Save(location);
                return RedirectToAction("Index");
            }

            PopulateViewModel(model, location);

            return View(model);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Delete(Guid id)
        {
            Location location = _locationRepository.GetById(id);
            if (location == null)
            {
                return RedirectToAction("Index");
            }

            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Delete(Location location)
        {
            _locationRepository.Delete(location.Id);

            return RedirectToAction("Index");
        }

        private Location GetLocation(LocationViewModel model)
        {
            var location = new Location
            {
                Id = model.Id,
                Comment = model.Comment,
                Name = model.Name,
                ShortName = model.ShortName,
                Net = model.Net,
                Cidr = model.Cidr,
                Net_v6 = model.NetV6,
                Cidr_v6 = model.CidrV6,
                CarrierConnectionId = model.CarrierConnectionId,
                UpdatedBy = User.Identity.Name
            };

            if (model.Region != Guid.Empty)
            {
                var region = _regionRepository.GetById(model.Region);

                if (region != null)
                {
                    location.Region = region;
                }
            }

            if (model.City != Guid.Empty)
            {
                var city = _cityRepository.GetById(model.City);
                if (city != null)
                {
                    location.City = city;
                }
            }

            var profileGroup = _profileGroupRepository.GetById(model.ProfileGroup ?? Guid.Empty);
            location.ProfileGroup = profileGroup;

            return location;
        }

        private void PopulateViewModel(LocationViewModel model, Location location)
        {
            PopulateRegions(model, location);
            PopulateCities(model, location);
            PopulateProfileGroups(model, location);
        }

        private void PopulateCities(LocationViewModel model, Location location)
        {
            var cities = _cityRepository.GetAll();
            model.Cities = cities.Select(city => new ListItemViewModel
            {
                Id = city.Id,
                Name = city.Name,
                Selected = location != null && location.City != null && location.City.Id == city.Id
            }).ToList();
            model.Cities.Insert(0, new ListItemViewModel() { Id = Guid.Empty, Name = string.Empty });
        }

        private void PopulateProfileGroups(LocationViewModel model, Location location)
        {
            var groups = _profileGroupRepository.GetAll();
            model.ProfileGroups = groups.Select(g => new ListItemViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Selected = location?.ProfileGroup != null && location.ProfileGroup.Id == g.Id
            }).ToList();
            model.ProfileGroups.Insert(0, new ListItemViewModel() { Id = null, Name = string.Empty });
        }

        private void PopulateRegions(LocationViewModel model, Location location)
        {
            var regions = _regionRepository.GetAll();
            model.Regions = regions.Select(region => new ListItemViewModel
            {
                Id = region.Id,
                Name = region.Name,
                Selected = location?.Region != null && location.Region.Id == region.Id
            }).ToList();
            model.Regions.Insert(0, new ListItemViewModel() { Id = Guid.Empty, Name = string.Empty });
        }
    }
}