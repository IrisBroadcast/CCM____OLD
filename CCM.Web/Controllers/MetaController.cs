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
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Repositories;
using CCM.Web.Authentication;
using CCM.Web.Infrastructure;
using CCM.Web.Models.Meta;

namespace CCM.Web.Controllers
{
    [CcmAuthorize(Roles = ApplicationConstants.Admin)]
    public class MetaController : BaseController
    {
        private readonly IMetaRepository _metaRepository;

        public MetaController(IMetaRepository metaRepository)
        {
            _metaRepository = metaRepository;
        }

        public ActionResult Index(string search = "")
        {
            var metaTypes = string.IsNullOrWhiteSpace(search) ? _metaRepository.GetAll() : _metaRepository.FindMetaTypes(search);
            var model = metaTypes.Select(metaType => new MetaViewModel() {Id = metaType.Id, Name = metaType.Name}).ToList();

            ViewBag.SearchString = search;
            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var model = new MetaFormViewModel {MetaTypeValues = _metaRepository.GetMetaTypeProperties()};
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MetaFormViewModel model)
        {
            var availableMetaTypes = _metaRepository.GetMetaTypeProperties();
            if (ModelState.IsValid)
            {
                if (_metaRepository.CheckMetaTypeNameAvailability(model.MetaTypeName, model.Id))
                {
                    var availableMetaType = availableMetaTypes.FirstOrDefault(m => m.FullPropertyName == model.SelectedMetaTypeValue);
                    if (availableMetaType != null)
                    {
                        var metaType = new CCM.Core.Entities.MetaType()
                        {
                            FullPropertyName = availableMetaType.FullPropertyName,
                            Name = model.MetaTypeName,
                            PropertyName = availableMetaType.PropertyName,
                            Type = availableMetaType.Type,
                            CreatedBy = User.Identity.Name,
                            UpdatedBy = User.Identity.Name
                        };

                        _metaRepository.Save(metaType);

                        return RedirectToAction("Index");
                    }
                }
            }

            model.MetaTypeValues = availableMetaTypes;
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            var metaType = _metaRepository.GetById(id);
            if (metaType == null)
            {
                return RedirectToAction("Index");
            }

            var model = new MetaFormViewModel
            {
                MetaTypeValues = _metaRepository.GetMetaTypeProperties(),
                MetaTypeName = metaType.Name,
                Id = metaType.Id,
                SelectedMetaTypeValue = metaType.FullPropertyName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MetaFormViewModel model)
        {
            var availableMetaTypes = _metaRepository.GetMetaTypeProperties();
            if (ModelState.IsValid)
            {
                if (_metaRepository.CheckMetaTypeNameAvailability(model.MetaTypeName, model.Id))
                {
                    var availableMetaType = availableMetaTypes.FirstOrDefault(m => m.FullPropertyName == model.SelectedMetaTypeValue);
                    if (availableMetaType != null)
                    {
                        var metaType = new CCM.Core.Entities.MetaType()
                        {
                            Id = model.Id,
                            FullPropertyName = availableMetaType.FullPropertyName,
                            Name = model.MetaTypeName,
                            PropertyName = availableMetaType.PropertyName,
                            Type = availableMetaType.Type,
                            UpdatedBy = User.Identity.Name
                        };

                        _metaRepository.Save(metaType);

                        return RedirectToAction("Index");
                    }
                }
            }

            model.MetaTypeValues = availableMetaTypes;
            return View(model);
        }

        [HttpGet]
        public ActionResult Delete(Guid id)
        {
            var metaType = _metaRepository.GetById(id);

            if (metaType == null)
            {
                return RedirectToAction("Index");
            }

            return View(metaType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(CCM.Core.Entities.MetaType metaType)
        {
            _metaRepository.Delete(metaType.Id);

            return RedirectToAction("Index");
        }
    }
}