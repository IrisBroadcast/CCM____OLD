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
using System.Threading.Tasks;
using System.Web.Http;
using CCM.Core.Helpers;
using CCM.DiscoveryApi.Areas.Discovery.Models;
using CCM.DiscoveryApi.Authentication;
using CCM.DiscoveryApi.Infrastructure;
using CCM.DiscoveryApi.Services;
using NLog;

namespace CCM.DiscoveryApi.Areas.Discovery.Controllers
{
    [DiscoveryControllerConfig]
    public class FiltersController : ApiController
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IDiscoveryHttpService _discoveryService;

        public FiltersController(IDiscoveryHttpService discoveryService)
        {
            _discoveryService = discoveryService;
        }

        /// <summary>
        /// Returnerar en lista med tillgängliga filter
        /// </summary>
        [DiscoveryAuthentication]
        [Authorize]
        [Route("~/filters")]
        public async Task<SrDiscovery> Post()
        {
            log.Debug("Received request for filters");
            using (new TimeMeasurer("Discovery Get filters"))
            {
                var filterDtos = await _discoveryService.GetFiltersAsync();

                List<Filter> filters = filterDtos.Select(f => new Filter()
                {
                    Name = f.Name,
                    FilterOptions = f.Options.Select(fo => new FilterOption() { Name = fo}).ToList()
                }).ToList();

                return new SrDiscovery { Filters = filters };
            }
        }


    }
}