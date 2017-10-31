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
using CCM.Core.Entities;
using CCM.Core.Entities.Specific;
using CCM.Core.Helpers;
using LazyCache;
using NLog;

namespace CCM.Core.Cache
{
    public class CcmCache : ICcmCache
    {
        private readonly IAppCache _cache;

        private const string CachedRegisteredSipsKey = "CachedRegisteredSip_List";
        private const string SettingsKey = "Settings";

        // Cache time in seconds
        public static int CacheTimeCachedRegisteredSips = ApplicationSettings.CacheTimeLiveData;
        public static int CacheTimeOngoingCalls = ApplicationSettings.CacheTimeLiveData;
        public static int CacheTimeOldCalls = ApplicationSettings.CacheTimeLiveData;

        public static int CacheTimeFilter = ApplicationSettings.CacheTimeConfigData;
        public static int CacheTimeProfiles = ApplicationSettings.CacheTimeConfigData;
        public static int CacheTimeSettings = ApplicationSettings.CacheTimeConfigData;
        
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public CcmCache(IAppCache cache)
        {
            _cache = cache;
        }

        public IList<RegisteredSipDto> GetRegisteredSips()
        {
            throw new NotImplementedException();
        }

        public void ClearRegisteredSips()
        {
            throw new NotImplementedException();
        }

        public IList<Call> GetCalls()
        {
            throw new NotImplementedException();
        }

        public void ClearCalls()
        {
            throw new NotImplementedException();
        }

        public IList<Setting> GetSettings()
        {
            throw new NotImplementedException();
        }

        public void ClearSettings()
        {
            throw new NotImplementedException();
        }
    }
}