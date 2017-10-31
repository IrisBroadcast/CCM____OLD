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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using CCM.Core.Cache;
using CCM.Core.Discovery;
using CCM.Core.Entities;
using CCM.Core.Entities.Specific;
using CCM.Core.Helpers;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Managers;
using CCM.Core.Interfaces.Repositories;
using LazyCache;
using NLog;

namespace CCM.Core.Service
{
    public class DiscoveryService : IDiscoveryService
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly ISettingsManager _settingsManager;
        private readonly IFilterManager _filterManager;
        private readonly IProfileRepository _profileRepository;
        private readonly IRegisteredSipRepository _registeredSipRepository;
        private readonly IAppCache _cache;

        public DiscoveryService(ISettingsManager settingsManager, IFilterManager filterManager, IProfileRepository profileRepository,
            IRegisteredSipRepository registeredSipRepository, IAppCache cache)
        {
            _settingsManager = settingsManager;
            _filterManager = filterManager;
            _profileRepository = profileRepository;
            _registeredSipRepository = registeredSipRepository;
            _cache = cache;
        }

        public List<ProfileDto> GetProfiles()
        {
            var profiles = _cache.GetProfiles(_profileRepository.GetAllProfileNamesAndSdp);
            var result = profiles.Select(p => new ProfileDto { Name = p.Name, Sdp = p.Sdp }).ToList();
            return result;
        }

        public List<FilterDto> GetFilters()
        {
            IList<AvailableFilter> filters = _cache.GetAvailableFilters(_filterManager.GetAvailableFiltersIncludingOptions);

            var result = filters.Select(filter => new FilterDto
            {
                Name = filter.Name,
                Options = filter.Options.OrderBy(o => o).ToList()
            })
            .OrderBy(f => f.Name)
            .ToList();

            return result;
        }

        public UserAgentsResultDto GetUserAgents(string caller, string callee, IList<KeyValuePair<string, string>> filterParams, bool includeCodecsInCall = false)
        {
            if (filterParams == null)
            {
                filterParams = new List<KeyValuePair<string,string>>();
            }

            IList<ProfileNameAndSdp> callerProfiles = !string.IsNullOrEmpty(caller) ? GetProfilesForRegisteredSip(caller) : _cache.GetProfiles(_profileRepository.GetAllProfileNamesAndSdp);
            log.Debug("Found {0} profiles for caller {1}", callerProfiles.Count, caller);

            IList<RegisteredSipDto> sipsOnline;

            if (string.IsNullOrWhiteSpace(callee))
            {
                var filterSelections = GetFilteringValues(filterParams);
                sipsOnline = GetFilteredSipsOnline(filterSelections);

                if (sipsOnline == null) { return new UserAgentsResultDto() { Profiles = new List<ProfileDto>(), UserAgents = new List<UserAgentDto>() }; }

                // Exkludera egna kodaren
                sipsOnline = sipsOnline.Where(sip => sip.Sip != caller).ToList();

                // Eventuellt exkludera kodare i samtal
                sipsOnline = includeCodecsInCall ? sipsOnline : sipsOnline.Where(sip => !sip.InCall).ToList();

            }
            else
            {
                var calleeSip = _registeredSipRepository.GetCachedRegisteredSips().FirstOrDefault(s => s.Sip == callee);

                if (calleeSip == null) { return new UserAgentsResultDto() { Profiles = new List<ProfileDto>(), UserAgents = new List<UserAgentDto>() }; }

                sipsOnline = new List<RegisteredSipDto> { calleeSip };
            }

            var result = ProfilesAndUserAgents(sipsOnline, callerProfiles.Select(p => new ProfileDto() { Name = p.Name, Sdp = p.Sdp }).ToList());
            return result;

        }

        private IList<ProfileNameAndSdp> GetProfilesForRegisteredSip(string sipId)
        {
            var regSip = _registeredSipRepository.GetCachedRegisteredSips().FirstOrDefault(s => s.Sip == sipId);
            var profileNames = regSip != null ? regSip.Profiles : new List<string>();
            return _cache.GetProfiles(_profileRepository.GetAllProfileNamesAndSdp).Where(p => profileNames.Contains(p.Name)).ToList();
        }

        /// <summary>
        /// Returnerar lista med kodare online filtrerat på filterparametrar
        /// </summary>
        private IList<RegisteredSipDto> GetFilteredSipsOnline(IList<FilterSelection> filterSelections)
        {
            var registeredSips = _registeredSipRepository.GetCachedRegisteredSips();
            if (registeredSips == null)
            {
                return new List<RegisteredSipDto>();
            }

            registeredSips = registeredSips.ToList(); // To be sure we don't mess with original list
            foreach (var filterSelection in filterSelections)
            {
                registeredSips = registeredSips.Where(rs => MetadataHelper.GetPropertyValue(rs, filterSelection.Property) == filterSelection.Value).ToList();
            }

            log.Debug("Found {0} registered sips.", registeredSips.Count);
            return registeredSips;

        }

        /// <summary>
        /// Returnerar en lista med de filter som efterfrågats samt deras filter-värdet
        /// </summary>
        private List<FilterSelection> GetFilteringValues(IList<KeyValuePair<string, string>> selectedFilters)
        {
            var availableFilters = _cache.GetAvailableFilters(_filterManager.GetAvailableFiltersIncludingOptions);

            var filterSelections = (from selectedFilter in selectedFilters.Where(f => !string.IsNullOrEmpty(f.Value))
                                    let matchingFilter = availableFilters.FirstOrDefault(f => f.Name == selectedFilter.Key)
                                    where matchingFilter != null
                                    select new FilterSelection() { Property = matchingFilter.FilteringName, Value = selectedFilter.Value })
                                    .ToList();

            return filterSelections;
        }


        private UserAgentsResultDto ProfilesAndUserAgents(IEnumerable<RegisteredSipDto> callees, IList<ProfileDto> callerProfiles)
        {
            var userAgents = new List<UserAgentDto>();

            try
            {
                var callerProfileNames = callerProfiles.Select(p => p.Name).ToList();

                foreach (var callee in callees)
                {
                    // INFO: Viktigt att ordningen på gemensamma profiler baseras på callee's profilordning.
                    var matchingProfiles = callee.Profiles.Intersect(callerProfileNames).ToList();

                    if (matchingProfiles.Any())
                    {
                        var displayName = DisplayNameHelper.GetDisplayName(callee, _settingsManager.SipDomain);
                        var userAgent = new UserAgentDto
                        {
                            SipId = string.Format("{0} <{1}>", displayName, callee.Sip),
                            PresentationName = displayName,
                            ConnectedTo = callee.InCallWithName ?? string.Empty,
                            InCall = callee.InCall,
                            MetaData = callee.MetaData.Select(meta => new KeyValuePair<string,string>(meta.Key, meta.Value)).ToList(),
                            Profiles = matchingProfiles,
                        };
                        userAgents.Add(userAgent);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error while getting user agents.");
                return new UserAgentsResultDto();
            }

            var allMatchingProfileNames = userAgents.SelectMany(ua => ua.Profiles.Select(p => p)).Distinct().ToList();

            var result = new UserAgentsResultDto
            {
                UserAgents = userAgents.OrderBy(ua => ua.SipId).ToList(),
                Profiles = callerProfiles.Where(p => allMatchingProfileNames.Contains(p.Name)).ToList()
            };

            return result;
        }
    }
}