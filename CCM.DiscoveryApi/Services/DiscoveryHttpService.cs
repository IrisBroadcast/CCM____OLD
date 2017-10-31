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
using System.Net.Http;
using System.Threading.Tasks;
using CCM.Core.Discovery;
using CCM.Core.Helpers;
using NLog;

namespace CCM.DiscoveryApi.Services
{
    // Retreives discovery data via CCM's REST service.

    public class DiscoveryHttpService : IDiscoveryHttpService
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public async Task<List<FilterDto>> GetFiltersAsync()
        {
            var url = new Uri(ApplicationSettings.CcmHost, "api/discovery/filters");
            log.Debug("Getting filters from {0}", url);
            return await GetData<FilterDto>(url);
        }
        
        public async Task<List<ProfileDto>> GetProfilesAsync()
        {
            var url = new Uri(ApplicationSettings.CcmHost, "api/discovery/profiles");
            log.Debug("Getting profiles from {0}", url);
            return await GetData<ProfileDto>(url);
        }

        public async Task<UserAgentsResultDto> GetUserAgentsAsync(UserAgentSearchParamsDto searchParams)
        {
            using (var client = new HttpClient())
            {
                var url = new Uri(ApplicationSettings.CcmHost, "api/discovery/useragents");
                log.Debug("Getting useragents from {0}", url);
                var response = await client.PostAsJsonAsync(url, searchParams).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    log.Warn("Unable to get discovery data. Response: {0} {1}", response.StatusCode, response.ReasonPhrase);
                    return null;
                }
                return await response.Content.ReadAsAsync<UserAgentsResultDto>();
            }
        }

        private async Task<List<T>> GetData<T>(Uri url)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    log.Error("Unable to get discovery data. Response: {0} {1}", response.StatusCode, response.ReasonPhrase);
                    return null;
                }
                return await response.Content.ReadAsAsync<List<T>>();
            }
        }

    }
}