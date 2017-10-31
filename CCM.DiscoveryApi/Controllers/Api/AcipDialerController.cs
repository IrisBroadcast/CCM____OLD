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
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using CCM.Core.Discovery;
using CCM.DiscoveryApi.Areas.DiscoveryV2.Model.Filters;
using CCM.DiscoveryApi.Areas.DiscoveryV2.Model.Profiles;
using CCM.DiscoveryApi.Areas.DiscoveryV2.Model.UserAgents;
using CCM.DiscoveryApi.Services;

namespace CCM.DiscoveryApi.Controllers.Api
{
    [RoutePrefix("api/acipdialer")]
    public class AcipDialerController : ApiController
    {
        private readonly IDiscoveryHttpService _discoveryService;

        public AcipDialerController(IDiscoveryHttpService discoveryService)
        {
            _discoveryService = discoveryService;
        }

        [HttpGet]
        [Route("filters")]
        public async Task<List<FilterV2>> Filters()
        {
            List<FilterDto> filterDtos = await _discoveryService.GetFiltersAsync();

            var filters = filterDtos.Select(f => new FilterV2()
            {
                Name = f.Name,
                Options = f.Options.Select(o => new FilterOptionV2() { Name = o }).ToList()
            }).ToList();

            return filters;
        }

        [HttpGet]
        [Route("profiles")]
        public async Task<List<ProfileDtoV2>> Profiles()
        {
            var profileDtos = await _discoveryService.GetProfilesAsync();

            var profiles = profileDtos
                .Select(p => new ProfileDtoV2() { Name = p.Name, Sdp = p.Sdp })
                .ToList();

            return profiles;
        }

        [HttpPost]
        [Route("useragents")]
        public async Task<UserAgentsResultV2> UserAgents(UserAgentSearchParamsV2 searchParams)
        {
            if (searchParams == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var searchParamsDto = new UserAgentSearchParamsDto()
            {
                Caller = searchParams.Caller,
                Callee = searchParams.Callee,
                IncludeCodecsInCall = searchParams.IncludeCodecsInCall,
                Filters = searchParams.Filters
            };

            UserAgentsResultDto uaResult = await _discoveryService.GetUserAgentsAsync(searchParamsDto);

            var result = new UserAgentsResultV2
            {
                Profiles = uaResult.Profiles.Select(p => new ProfileDtoV2() { Name = p.Name, Sdp = p.Sdp }).ToList(),
                UserAgents = uaResult.UserAgents.Select(ua => new UserAgentDtoV2()
                {
                    SipId = ua.SipId,
                    ConnectedTo = ua.ConnectedTo,
                    Profiles = ua.Profiles,
                    MetaData = ua.MetaData.Select(m => new KeyValuePairDtoV2(m.Key, m.Value)).ToList()
                }).ToList()
            };

            return result;
        }
    }
}