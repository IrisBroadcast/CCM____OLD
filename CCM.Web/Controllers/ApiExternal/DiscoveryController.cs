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
using System.Net;
using System.Reflection;
using System.Web.Http;
using CCM.Core.Discovery;
using CCM.Core.Interfaces;
using CCM.Web.Infrastructure;
using NLog;

namespace CCM.Web.Controllers.ApiExternal
{
    [CamelCaseControllerConfig]
    [RoutePrefix("api/discovery")]
    public class DiscoveryController : ApiController
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IDiscoveryService _discoveryService;

        public DiscoveryController(IDiscoveryService discoveryService)
        {
            _discoveryService = discoveryService;
        }

        [HttpGet]
        [Route("filters")]
        public List<FilterDto> Filters()
        {
            return _discoveryService.GetFilters();
        }

        [HttpGet]
        [Route("profiles")]
        public List<ProfileDto> Profiles()
        {
            return _discoveryService.GetProfiles();
        }

        [Route("useragents")]
        [HttpPost]
        public UserAgentsResultDto UserAgents(UserAgentSearchParamsDto searchParams)
        {
            if (searchParams == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            UserAgentsResultDto uaResult = _discoveryService.GetUserAgents(searchParams.Caller, searchParams.Callee, searchParams.Filters, searchParams.IncludeCodecsInCall);
            return uaResult;
        }

    }
}
