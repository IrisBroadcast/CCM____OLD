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
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using CCM.Core.Entities;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Repositories;
using CCM.Web.Authentication;
using CCM.Web.Models.Common;
using CCM.Web.Models.Studio;

namespace CCM.Web.Controllers
{
    // Administration av studior
    [CcmAuthorize(Roles = "Admin, Remote")]
    public class StudioController : Controller
    {

        private readonly IStudioRepository _studioRepository;

        public StudioController(IStudioRepository studioRepository)
        {
            _studioRepository = studioRepository;
        }

        public ActionResult Index(string search = "")
        {
            var studios = string.IsNullOrWhiteSpace(search) ? _studioRepository.GetAll() : _studioRepository.FindStudios(search);
            var model = studios.Select(StudioToViewModel).OrderBy(s => s.Name).ToList();
            ViewBag.SearchString = search;
            return View(model);
        }
        
        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Create()
        {
            return View(new StudioViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Create(StudioViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var studio = ViewModelToStudio(model);
                    studio.CreatedBy = studio.UpdatedBy = User.Identity.Name;
                    studio.CreatedOn = studio.UpdatedOn = DateTime.UtcNow;
                    _studioRepository.Save(studio);
                    return RedirectToAction("Index");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View(model);
            }
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Edit(Guid id)
        {
            var studio = _studioRepository.GetById(id);
            if (studio == null)
            {
                return RedirectToAction("Index");
            }

            var model = StudioToViewModel(studio);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Edit(StudioViewModel model)
        {
            if (ModelState.IsValid)
            {
                var studio = ViewModelToStudio(model);
                studio.UpdatedBy = User.Identity.Name;
                _studioRepository.Save(studio);
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Delete(Guid id)
        {
            var studio = _studioRepository.GetById(id);
            if (studio == null)
            {
                return RedirectToAction("Index");
            }

            var vm = new DeleteViewModel()
            {
                Id = studio.Id,
                Name = studio.Name,
                Title = "Ta bort studio",
                WarningText = "Är du säker på att du vill ta bort denna studio"
            };

            return View("../Shared/Delete", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Delete(DeleteViewModel model)
        {
            _studioRepository.Delete(model.Id);
            return RedirectToAction("Index");
        }

        #region Private methods
        private StudioViewModel StudioToViewModel(Studio studio)
        {
            return Mapper.Map<StudioViewModel>(studio);
        }

        private Studio ViewModelToStudio(StudioViewModel model)
        {
            return Mapper.Map<Studio>(model);
        }
        #endregion
    }
}
