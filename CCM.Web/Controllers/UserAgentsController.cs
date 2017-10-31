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
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CCM.CodecControl;
using CCM.Core.Entities;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Repositories;
using CCM.Web.Authentication;
using CCM.Web.Infrastructure;
using CCM.Web.Models.UserAgents;

namespace CCM.Web.Controllers
{
    [CcmAuthorize(Roles = "Admin, Remote")]
    public class UserAgentsController : BaseController
    {
        private readonly ICodecPresetRepository _codecPresetRepository;
        private readonly IUserAgentRepository _userAgentRepository;
        private readonly IProfileRepository _profileRepository;

        public UserAgentsController(IUserAgentRepository userAgentRepository, IProfileRepository profileRepository, ICodecPresetRepository codecPresetRepository)
        {
            _userAgentRepository = userAgentRepository;
            _profileRepository = profileRepository;
            _codecPresetRepository = codecPresetRepository;
        }

        public ActionResult Index(string search = "")
        {
            var userAgents = string.IsNullOrWhiteSpace(search) ? _userAgentRepository.GetAll() : _userAgentRepository.Find(search);

            ViewBag.SearchString = search;
            return View(userAgents);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Create()
        {
            var model = new UserAgentViewModel() { InputGainStep = 3 };
            GetCodecApiValues(model);
            GetProfileViewModels(model);
            GetCodecPresetViewModels(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Create(UserAgentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userAgent = GetUserAgentFromViewModel(model);
                userAgent.CreatedBy = User.Identity.Name;

                _userAgentRepository.Save(userAgent);
                return RedirectToAction("Index");
            }
            GetCodecApiValues(model);
            return View(model);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Edit(Guid id)
        {
            var userAgent = _userAgentRepository.GetById(id);

            if (userAgent == null)
            {
                return RedirectToAction("Index");
            }

            var model = new UserAgentViewModel()
            {
                ActiveX = userAgent.Ax,
                UserInterfaceLink = userAgent.UserInterfaceLink,
                Height = userAgent.Height,
                Id = userAgent.Id,
                Identifier = userAgent.Identifier,
                Image = userAgent.Image,
                Name = userAgent.Name,
                Width = userAgent.Width,
                MatchType = userAgent.MatchType,
                Api = userAgent.Api,
                Lines = userAgent.Lines,
                Inputs = userAgent.Inputs,
                MaxInputDb = userAgent.InputMaxDb,
                MinInputDb = userAgent.InputMinDb,
                Comment = userAgent.Comment,
                InputGainStep = userAgent.InputGainStep,
                GpoNames = userAgent.GpoNames,
                UserInterfaceIsOpen = userAgent.UserInterfaceIsOpen,
                UseScrollbars = userAgent.UseScrollbars
            };

            GetCodecApiValues(model);
            GetProfileViewModels(model, userAgent.Profiles);
            GetCodecPresetViewModels(model, userAgent.CodecPresets);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Edit(UserAgentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userAgent = GetUserAgentFromViewModel(model);
                userAgent.Id = model.Id;

                _userAgentRepository.Save(userAgent);
                return RedirectToAction("Index");
            }
            GetCodecApiValues(model);
            return View(model);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Delete(Guid id)
        {
            var userAgent = _userAgentRepository.GetById(id);

            return View(userAgent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Delete(UserAgent agent)
        {
            _userAgentRepository.Delete(agent.Id);

            return RedirectToAction("Index");
        }

        private void GetCodecApiValues(UserAgentViewModel model)
        {
            model.CodecApis = new Dictionary<string, string> {{string.Empty, string.Empty}};
            foreach (var availableApi in CodecManager.AvailableApis.Values)
            {
                model.CodecApis.Add(availableApi.DisplayName, availableApi.Name);
            }
            
            model.LinesInList = 10;
            model.InputsInList = 10;
            model.InputDbInListMin = -100;
            model.InputDbInListMax = 100;
        }

        private UserAgent GetUserAgentFromViewModel(UserAgentViewModel model)
        {
            var userAgent = new UserAgent
            {
                Ax = model.ActiveX,
                UserInterfaceLink = model.UserInterfaceLink,
                Height = model.Height,
                Identifier = model.Identifier,
                Name = model.Name,
                Width = model.Width,
                Profiles = GetSelectedProfiles(model.Profiles),
                Image = model.Image,
                MatchType = model.MatchType,
                UpdatedBy = User.Identity.Name,
                Api = model.Api,
                Lines = model.Lines,
                Inputs = model.Inputs,
                InputMinDb = model.MinInputDb,
                InputMaxDb = model.MaxInputDb,
                Comment = model.Comment,
                InputGainStep = model.InputGainStep,
                GpoNames = model.GpoNames,
                UserInterfaceIsOpen = model.UserInterfaceIsOpen,
                UseScrollbars = model.UseScrollbars,
                CodecPresets = GetSelectedCodecPresets(model.CodecPresets)
            };

            // Hantera uppladdad bild
            var imageFile = Request.Files != null && Request.Files.Count > 0 ? Request.Files[0] : null;

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var imagesFolder = Server.MapPath("~/Images/Agents");

                // Ta bort ev gammal bild
                if (!string.IsNullOrWhiteSpace(model.Image))
                {
                    var oldFile = Path.Combine(imagesFolder, model.Image);
                    if (System.IO.File.Exists(oldFile))
                    {
                        System.IO.File.Delete(oldFile);
                    }
                    // TODO: Ta bort från andra servern!
                }

                // Spara
                var newFile = Path.GetFileName(imageFile.FileName) ?? string.Empty; // Empty string to quiet the compiler.
                var filename = Path.Combine(imagesFolder, newFile);
                imageFile.SaveAs(filename);
                userAgent.Image = newFile;

                // TODO: Replikera bilden till andra servern!
                // TODO: Eller ännu hellre lagra på gemensam filyta
            }
            return userAgent;
        }

        private static List<Profile> GetSelectedProfiles(IEnumerable<ProfileListItemViewModel> profiles)
        {
            return profiles == null ? new List<Profile>()
                : profiles
                    .Where(p => p.Selected)
                    .OrderBy(p => p.SortIndex)
                    .Select(p => new Profile {Id = p.Id, Name = p.Name})
                    .ToList();
        }

        private static List<CodecPreset> GetSelectedCodecPresets(IEnumerable<CodecPresetListItemViewModel> codecPresets)
        {
            return codecPresets == null ? new List<CodecPreset>()
                : codecPresets
                    .Where(p => p.Selected)
                    .Select(codecPreset => new CodecPreset() {Id = codecPreset.Id, Name = codecPreset.Name})
                    .ToList();
        }

        private void GetProfileViewModels(UserAgentViewModel model, List<Profile> profiles = null)
        {
            model.Profiles = new List<ProfileListItemViewModel>();

            // Add existing profiles
            if (profiles != null)
            {
                foreach (var profile in profiles)
                {
                    model.Profiles.Add(new ProfileListItemViewModel
                    {
                        Id = profile.Id,
                        Name = profile.Name,
                        Selected = true
                    });
                }
            }

            List<Profile> allProfiles = _profileRepository.GetAll();

            // Add other profiles
            foreach (Profile profile in allProfiles)
            {
                if (!model.Profiles.Any(p => p.Id == profile.Id))
                {
                    model.Profiles.Add(new ProfileListItemViewModel
                        {
                            Id = profile.Id,
                            Name = profile.Name,
                            Selected = false
                        });
                }
            }
        }

        private void GetCodecPresetViewModels(UserAgentViewModel model, List<CodecPreset> connectedCodecPresets = null)
        {
            var codecPresets = _codecPresetRepository.GetAll();
            model.CodecPresets = new List<CodecPresetListItemViewModel>();
            foreach (var codecPreset in codecPresets)
            {
                model.CodecPresets.Add(new CodecPresetListItemViewModel()
                {
                    Id = codecPreset.Id,
                    Name = codecPreset.Name,
                    Selected = connectedCodecPresets != null ? connectedCodecPresets.Any(c => c.Id == codecPreset.Id) : false
                });
            }
        }
    }
}