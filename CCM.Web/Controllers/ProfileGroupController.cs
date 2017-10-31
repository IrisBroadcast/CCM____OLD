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
using CCM.Web.Authentication;
using CCM.Web.Infrastructure;
using CCM.Web.Models.Profile;
using CCM.Core.Entities.Specific;
using AutoMapper;
using CCM.Web.Models.UserAgents;

namespace CCM.Web.Controllers
{
    [CcmAuthorize(Roles = "Admin, Remote")]
    public class ProfileGroupController : BaseController
    {
        private readonly IProfileGroupRepository _profileGroupRepository;
        private readonly IProfileRepository _profileRepository;

        public ProfileGroupController(IProfileGroupRepository profileGroupRepository, IProfileRepository profileRepository)
        {
            _profileGroupRepository = profileGroupRepository;
            _profileRepository = profileRepository;
        }

        public ActionResult Index(string search = "")
        {
            ViewBag.SearchString = search;

            //var profiles = string.IsNullOrWhiteSpace(search) ?
            //    _profileRepository.GetAllProfilesIncludingRelations() :
            //    _profileRepository.FindProfiles(search);
            var profileGroups = _profileGroupRepository.GetAll();

            return View(profileGroups);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Create()
        {
            ViewBag.Title = Resources.New_ProfileGroup;

            var model = new ProfileGroupViewModel();
            PopulateViewModel(model);
            return View("CreateEdit", model);
        }

        private void PopulateViewModel(ProfileGroupViewModel model)
        {
            var profiles = _profileRepository.GetAllProfileInfos() ?? new List<ProfileInfo>();
            var notSelectedViewModels = profiles.Where(p => !model.Profiles.Select(pr => pr.Id).Contains(p.Id)).OrderBy(p => p.Name).Select(p => new ProfileListItemViewModel() { Id = p.Id, Name = p.Name, Selected = false });
            model.Profiles.AddRange(notSelectedViewModels);
        }


        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Edit(Guid id)
        {
            ViewBag.Title = Resources.Edit_ProfileGroup;

            var group = _profileGroupRepository.GetById(id);
            if (group == null)
            {
                return RedirectToAction("Index");
            }
            var model = Mapper.Map<ProfileGroupViewModel>(group);
            PopulateViewModel(model);
            return View("CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Edit(ProfileGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                var group = Mapper.Map<ProfileGroup>(model);
                group.UpdatedBy = User.Identity.Name;
                _profileGroupRepository.Save(group);
                return RedirectToAction("Index");
            }

            return View("CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult CreateEdit(ProfileGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                var group = Mapper.Map<ProfileGroup>(model);

                if (group.Id == Guid.Empty)
                {
                    group.CreatedBy = User.Identity.Name;
                    group.CreatedOn = DateTime.UtcNow;
                }

                group.UpdatedBy = User.Identity.Name;
                group.UpdatedOn = DateTime.UtcNow;

                try
                {
                    _profileGroupRepository.Save(group);
                }
                catch (DuplicateNameException ex)
                {
                    ModelState.AddModelError("NameMustBeUnique", "Gruppen kunde inte sparas. Namnet används redan.");
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

            var group = _profileGroupRepository.GetById(id);
            if (group == null)
            {
                return RedirectToAction("Index");
            }

            return View(group);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Delete(ProfileGroup profile)
        {
            _profileGroupRepository.Delete(profile.Id);
            return RedirectToAction("Index");
        }

    }
}