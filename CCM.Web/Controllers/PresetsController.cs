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
using System.Web.Mvc;
using CCM.Core.Entities;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Security;
using CCM.Web.Authentication;
using CCM.Web.Infrastructure;

namespace CCM.Web.Controllers
{
    public class PresetsController : Controller
    {
        private readonly ICodecPresetRepository _codecPresetRepository;

        public PresetsController(ICodecPresetRepository codecPresetRepository)
        {
            _codecPresetRepository = codecPresetRepository;
        }
        
        public ActionResult Index(string search = "")
        {
            var codecPresets = string.IsNullOrWhiteSpace(search) ? _codecPresetRepository.GetAll() : _codecPresetRepository.Find(search);

            ViewBag.SearchString = search;
            return View(codecPresets);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Create(CodecPreset model)
        {
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                model.CreatedBy = User.Identity.Name;
                model.UpdatedBy = User.Identity.Name;

                _codecPresetRepository.Save(model);
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("Name", Resources.Name_Required);

            return View(model);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Edit(Guid id)
        {
            var codecPreset = _codecPresetRepository.GetById(id);

            if (codecPreset == null)
            {
                return RedirectToAction("Index");
            }

            return View(codecPreset);
        }

        [HttpPost]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Edit(CodecPreset model)
        {
            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                model.UpdatedBy = User.Identity.Name;

                _codecPresetRepository.Save(model);
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("Name", Resources.Name_Required);

            return View(model);
        }

        [HttpGet]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Delete(Guid id)
        {
            var codecPreset = _codecPresetRepository.GetById(id);

            if (codecPreset == null)
            {
                return RedirectToAction("Index");
            }

            return View(codecPreset);
        }

        [HttpPost]
        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        public ActionResult Delete(CodecPreset model)
        {
            _codecPresetRepository.Delete(model.Id);
            return RedirectToAction("Index");
        }
    }
}