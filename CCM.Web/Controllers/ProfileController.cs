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
using System.Data;
using System.Linq;
using System.Web.Mvc;
using CCM.Core.Entities;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Security;
using CCM.Web.Authentication;
using CCM.Web.Infrastructure;
using CCM.Web.Models.Profile;

namespace CCM.Web.Controllers
{
    [CcmAuthorize(Roles = "Admin, Remote")]
    public class ProfileController : BaseController
    {
        private readonly IProfileRepository _profileRepository;

        public ProfileController(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public ActionResult Index(string search = "")
        {
            ViewBag.SearchString = search;

            var profiles = string.IsNullOrWhiteSpace(search) ?
                _profileRepository.GetAll() :
                _profileRepository.FindProfiles(search);

            return View(profiles);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Create()
        {
            ViewBag.Title = Resources.New_Profile;
            return View("CreateEdit", new ProfileViewModel());
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Edit(Guid id)
        {
            Profile profile = _profileRepository.GetById(id);
            if (profile == null)
            {
                return RedirectToAction("Index");
            }

            var model = new ProfileViewModel
            {
                Description = profile.Description,
                Id = profile.Id,
                Name = profile.Name,
                Sdp = profile.Sdp,
            };

            ViewBag.Title = Resources.Edit_Profile;
            return View("CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult CreateEdit(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var profile = new Profile()
                {
                    Description = model.Description,
                    Id = model.Id,
                    Name = model.Name,
                    Sdp = model.Sdp,
                    UpdatedBy = User.Identity.Name,
                };

                if (profile.Id == Guid.Empty)
                {
                    profile.CreatedBy = User.Identity.Name;
                }

                try
                {
                    _profileRepository.Save(profile);
                }
                catch (DuplicateNameException ex)
                {
                    ModelState.AddModelError("NameMustBeUnique", "Profilen kunde inte sparas. Namnet används redan.");
                    return View("CreateEdit", model);
                }
                return RedirectToAction("Index");
            }

            return View("CreateEdit", model);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Delete(Guid id)
        {
            Profile profile = _profileRepository.GetById(id);
            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Delete(Profile profile)
        {
            _profileRepository.Delete(profile.Id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public JsonResult SetProfileSortIndex(List<GuidSortIndexTuple> profileSortIndex)
        {
            var model = new SetSortIndexResultViewModel();

            if (profileSortIndex == null || profileSortIndex.Any(d => d.Id == Guid.Empty))
            {
                model.IndexSet = false;
            }
            else
            {
                var paramdata = profileSortIndex.Select(i => new Tuple<Guid, int>(i.Id, i.SortIndex)).ToList();
                _profileRepository.SetProfileSortIndex(paramdata);
            }
            return Json(model);
        }
    }
}