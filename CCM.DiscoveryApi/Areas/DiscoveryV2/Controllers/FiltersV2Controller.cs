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

﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using CCM.DiscoveryApi.Areas.DiscoveryV2.Model;
using CCM.DiscoveryApi.Interfaces;
using CCM.DiscoveryApi.Models;
using CCM.WebCommon.Authentication;
using log4net;
using LogManager = log4net.LogManager;

namespace CCM.DiscoveryApi.Areas.DiscoveryV2.Controllers
{
    [BasicAuthentication] // Enable Basic authentication for this controller.
    public class FiltersV2Controller : ApiController
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDiscoveryService _discoveryService;

        public FiltersV2Controller(IDiscoveryService discoveryService)
        {
            _discoveryService = discoveryService;
        }

        [Authorize]
        [HttpGet]
        [Route("~/v2/filters")]
        public List<FilterV2> Get()
        {
            var filterDtos = _discoveryService.GetFilters();

            var filters = filterDtos.Select(f => new FilterV2()
            {
                Name = f.Name,
                Options = f.Options.Select(o => new FilterOptionV2() {Name = o}).ToList()
            }).ToList();

            return filters;
        }
    }
}