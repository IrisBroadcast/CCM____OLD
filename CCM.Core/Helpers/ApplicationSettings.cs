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
using System.Configuration;

namespace CCM.Core.Helpers
{
    public static class ApplicationSettings
    {
        public static string RadiusHost => ConfigurationManager.AppSettings["RadiusHost"];
        public static string RadiusSecret => ConfigurationManager.AppSettings["RadiusSecret"];

        public static Uri DiscoveryHost => new Uri(ConfigurationManager.AppSettings["DiscoveryHost"]);
        public static string DiscoveryLogLevelUrl => new Uri(DiscoveryHost, "api/loglevel").ToString();

        // URL to CCM web, used in Discovery service
        public static Uri CcmHost => new Uri(ConfigurationManager.AppSettings["CCMHost"]);

        public static int CacheTimeLiveData => Int32.Parse(ConfigurationManager.AppSettings["CacheTimeLiveData"]);
        public static int CacheTimeConfigData => Int32.Parse(ConfigurationManager.AppSettings["CacheTimeConfigData"]);

        public static string DiscoveryUsername => ConfigurationManager.AppSettings["DiscoveryUsername"];
        public static string DiscoveryPassword => ConfigurationManager.AppSettings["DiscoveryPassword"];
    }
}