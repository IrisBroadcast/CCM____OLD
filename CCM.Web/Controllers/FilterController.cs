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
using CCM.Core.Interfaces.Managers;
using CCM.Web.Authentication;
using CCM.Web.Infrastructure;
using CCM.Web.Models.Filter;

namespace CCM.Web.Controllers
{
    [CcmAuthorize(Roles = ApplicationConstants.Admin)]
    public class FilterController : BaseController
    {
        private readonly IFilterManager _filterManager;

        public FilterController(IFilterManager filterManager)
        {
            _filterManager = filterManager;
        }

        public ActionResult Index(string search = "")
        {
            var filters = string.IsNullOrWhiteSpace(search) ? _filterManager.GetAllFilters() : _filterManager.FindFilters(search);
            var model = filters.Select(filter => new FilterViewModel() {Id = filter.Id, Name = filter.Name}).ToList();

            ViewBag.SearchString = search;
            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var model = new FilterFormViewModel
            {
                Filters = _filterManager.GetFilterProperties()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FilterFormViewModel model)
        {
            var availableFilters = _filterManager.GetFilterProperties();

            if (ModelState.IsValid)
            {
                if (_filterManager.CheckFilterNameAvailability(model.FilterName, model.Id))
                {
                    var availableFilter = availableFilters.FirstOrDefault(f => f.FilteringName == model.SelectedFilter);
                    if (availableFilter != null)
                    {
                        var filter = new Core.Entities.Filter()
                        {
                            FilteringName = availableFilter.FilteringName,
                            Name = model.FilterName,
                            TableName = availableFilter.TableName,
                            ColumnName = availableFilter.ColumnName,
                            CreatedBy = User.Identity.Name,
                            UpdatedBy = User.Identity.Name
                        };

                        _filterManager.Save(filter);

                        return RedirectToAction("Index");
                    }
                }
            }

            model.Filters = availableFilters;
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            var filter = _filterManager.GetFilter(id);
            if (filter == null)
            {
                return RedirectToAction("Index");
            }

            var model = new FilterFormViewModel();
            model.Filters = _filterManager.GetFilterProperties();
            model.FilterName = filter.Name;
            model.Id = filter.Id;
            model.SelectedFilter = filter.FilteringName;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FilterFormViewModel model)
        {
            var availableFilters = _filterManager.GetFilterProperties();
            if (ModelState.IsValid)
            {
                if (_filterManager.CheckFilterNameAvailability(model.FilterName, model.Id))
                {
                    var availableFilter = availableFilters.FirstOrDefault(f => f.FilteringName == model.SelectedFilter);
                    if (availableFilter != null)
                    {
                        var filter = new Core.Entities.Filter()
                        {
                            Id = model.Id,
                            FilteringName = availableFilter.FilteringName,
                            TableName = availableFilter.TableName,
                            Name = model.FilterName,
                            ColumnName = availableFilter.ColumnName,
                            UpdatedBy = User.Identity.Name
                        };

                        _filterManager.Save(filter);

                        return RedirectToAction("Index");
                    }
                }
                ModelState.AddModelError(string.Empty, Resources.Filter_Name_In_Use);
            }

            ModelState.AddModelError("NameMustBeUnique", "Filtret kunde inte sparas. Möjligen används filter-namnet redan.");
            
            model.Filters = availableFilters;
            return View(model);
        }

        [HttpGet]
        public ActionResult Delete(Guid id)
        {
            var filter = _filterManager.GetFilter(id);

            if (filter == null)
            {
                return RedirectToAction("Index");
            }

            return View(filter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(CCM.Core.Entities.Filter filter)
        {
            _filterManager.Delete(filter.Id);

            return RedirectToAction("Index");
        }
    }
}