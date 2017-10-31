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
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using CCM.Core.Discovery;
using CCM.Core.Helpers;
using CCM.DiscoveryApi.Areas.Discovery.Models;
using CCM.DiscoveryApi.Authentication;
using CCM.DiscoveryApi.Infrastructure;
using CCM.DiscoveryApi.Models;
using CCM.DiscoveryApi.Services;
using NLog;

namespace CCM.DiscoveryApi.Areas.Discovery.Controllers
{
    [DiscoveryControllerConfig]
    public class UserAgentsController : ApiController
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IDiscoveryHttpService _discoveryService;

        public UserAgentsController(IDiscoveryHttpService discoveryService)
        {
            _discoveryService = discoveryService;
        }

        public string Get()
        {
            log.Debug("User agent method accessed {0}", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            return "CCM Discovery Api at your service!";
        }

        /// <summary>
        /// Gets user agents based on supplied filters.
        /// </summary>
        [DiscoveryAuthentication]
        [Authorize]
        [Route("~/useragents")]
        public async Task<SrDiscovery> Post()
        {
            log.Debug("Received request for user agents");
            var parameters = (SrDiscoveryParameters)Request.Properties["SRDiscoveryParameters"];

            var searchParams = new UserAgentSearchParamsDto()
            {
                Caller = parameters.Caller,
                Callee = parameters.Callee,
                IncludeCodecsInCall = parameters.IncludeCodecsInCall,
                Filters = parameters.Filters
            };

            UserAgentsResultDto uaResult;

            using (new TimeMeasurer("Discovery Get user agents"))
            {
                uaResult = await _discoveryService.GetUserAgentsAsync(searchParams);
            }

            if (uaResult == null)
            {
                log.Info("No user agents found returned");
                return new SrDiscovery();
            }

            log.Debug("Returning {0} useragents and {1} profiles.",
                uaResult.UserAgents != null ? uaResult.UserAgents.Count : 0,
                uaResult.Profiles != null ? uaResult.Profiles.Count : 0);

            var profiles = uaResult.Profiles == null ? new List<Profile>()
                : uaResult.Profiles.Select(p => new Profile() { Name = p.Name, Sdp = p.Sdp }).ToList();

            var userAgents = uaResult.UserAgents == null ? new List<UserAgent>() : uaResult.UserAgents.Select(ua => new UserAgent()
            {
                SipId = ua.SipId,
                ConnectedTo = ua.ConnectedTo,
                ProfileRec = ua.Profiles.Select(p => new UserAgentProfileRef { Name = p }).ToList(),
                MetaData = ua.MetaData.Select(m => new UserAgentMetaData() { Key = m.Key, Value = m.Value }).ToList()
            }).ToList();

            return new SrDiscovery()
            {
                UserAgents = userAgents,
                Profiles = profiles
            };
        }

    }
}