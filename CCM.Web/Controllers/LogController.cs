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
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Managers;
using CCM.Web.Models.Log;
using NLog;

namespace CCM.Web.Controllers
{
    public class LogController : Controller
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly ILogRepository _logRepository;

        public LogController(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task<ActionResult> Index(LogViewModel model)
        {
            model.Search = model.Search ?? string.Empty;
            model.Application = !string.IsNullOrEmpty(model.Application) ? model.Application : CcmApplications.Web;
            model.SelectedLastOption = !string.IsNullOrEmpty(model.SelectedLastOption) ? model.SelectedLastOption : GetLastOptions().First().Value;
            model.StartDateTime = model.StartDateTime > DateTime.MinValue ? model.StartDateTime : DateTime.Now.AddHours(-6);
            model.EndDateTime = model.EndDateTime > DateTime.MinValue ? model.EndDateTime : DateTime.Now; 
            model.Rows = model.Rows > 0 ? model.Rows : 25;

            DateTime? startTime;
            DateTime? endTime;

            if (model.SelectedLastOption == "interval")
            {
                startTime = model.StartDateTime;
                endTime = model.EndDateTime;
            }
            else
            {
                var ts = TimeSpan.Parse(model.SelectedLastOption);
                startTime = DateTime.Now.Subtract(ts);
                endTime = null;
            }

            model.LogRows = await _logRepository.GetLastAsync(model.Rows, model.Application, startTime, endTime, model.SelectedLevel, model.Search, model.ActivityId);
            model.LastOptions = GetLastOptions();
            model.Levels = LogLevel.AllLoggingLevels.ToList().Select(l => new SelectListItem() { Value = l.Ordinal.ToString(), Text = l.Name });

            return View(model);
        }

        private IEnumerable<SelectListItem> GetLastOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem() {Value = "00:01:00", Text = @"minuten"},
                new SelectListItem() {Value = "00:10:00", Text = @"10 minuter"},
                new SelectListItem() {Value = "00:30:00", Text = @"30 minuter"},
                new SelectListItem() {Value = "01:00:00", Text = @"1 timme"},
                new SelectListItem() {Value = "03:00:00", Text = @"3 timmar"},
                new SelectListItem() {Value = "06:00:00", Text = @"6 timmar"},
                new SelectListItem() {Value = "12:00:00", Text = @"12 timmar"},
                new SelectListItem() {Value = "1.00:00:00", Text = @"dygnet"},
                new SelectListItem() {Value = "3.00:00:00", Text = @"3 dygnen"},
                new SelectListItem() {Value = "7.00:00:00", Text = @"veckan"},
                new SelectListItem() {Value = "interval", Text = @"Tidsintervall"},
            };
        }

        [HttpGet]
        public async Task<ActionResult> Level(string application)
        {
            application = application ?? string.Empty;
            string logLevel = "";

            if (application == CcmApplications.Web)
            {
                logLevel = LogLevelManager.GetCurrentLevel().Name;
            }
            else if (application == CcmApplications.Discovery)
            {
                logLevel = await GetDiscoveryLogLevelAsync();
            }

            ViewBag.CurrentLevel = logLevel;
            ViewBag.Application = application;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Level(string logLevel, string application)
        {
            application = application ?? string.Empty;

            if (application == CcmApplications.Web)
            {
                var wasSet = LogLevelManager.SetLogLevel(logLevel);
                if (wasSet)
                {
                    log.Info("Log level changed to {0}", logLevel);
                }
                else
                {
                    log.Info("Log level was NOT changed. ({0})", logLevel);
                }
            }
            else if (application == CcmApplications.Discovery)
            {
                var result = await SetDiscoveryLogLevelAsync(logLevel);

                if (result != logLevel)
                {
                    ViewBag.CurrentLevel = logLevel;
                    ViewBag.Application = application;
                    return View();
                }
            }

            return RedirectToAction("Index", new { application });
        }

        [HttpGet]
        public async Task<ActionResult> Delete(string application, int nrOfRowsToDelete = 100)
        {
            ViewBag.nrOfRowsToDelete = nrOfRowsToDelete;
            ViewBag.LogCount = await _logRepository.GetLogInfoAsync();
            
            return View();
        }

        [HttpPost]
        public ActionResult Delete(int nrOfRowsToDelete = 100)
        {
            _logRepository.DeleteOldest(nrOfRowsToDelete);
            ViewBag.nrOfRowsToDelete = nrOfRowsToDelete;
            return RedirectToAction("Delete", new { nrOfRowsToDelete });
        }

        private async Task<string> GetDiscoveryLogLevelAsync()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ApplicationSettings.DiscoveryLogLevelUrl);
                if (response.IsSuccessStatusCode)
                {
                    var levelModel = await response.Content.ReadAsAsync<LevelModel>();
                    return levelModel != null ? levelModel.LogLevel : string.Empty;
                }
                return "";
            }
        }

        private async Task<string> SetDiscoveryLogLevelAsync(string level)
        {
            using (var client = new HttpClient())
            {
                var responseMessage = await client.PostAsJsonAsync(ApplicationSettings.DiscoveryLogLevelUrl, new LevelModel() { LogLevel = level });
                if (responseMessage.IsSuccessStatusCode)
                {
                    var levelModel = await responseMessage.Content.ReadAsAsync<LevelModel>();
                    return levelModel != null ? levelModel.LogLevel : string.Empty;
                }
                return "";
            }
        }
    }
}