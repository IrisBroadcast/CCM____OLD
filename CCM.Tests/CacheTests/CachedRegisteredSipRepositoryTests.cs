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

﻿using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using CCM.Core.Cache;
using CCM.Core.Entities;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Kamailio;
using CCM.Core.Managers;
using CCM.Data.Repositories;
using LazyCache;
using NUnit.Framework;

namespace CCM.Tests.CacheTests
{
    [TestFixture]
    public class CachedRegisteredSipRepositoryTests
    {
        [Test, Explicit]
        public void ReloadOnUpdate()
        {
            var repo = GetCachedRegisteredSipRepository();
            var sipsOnline = repo.GetCachedRegisteredSips();
            var sipOnline = sipsOnline.First();
            var updated = sipOnline.Updated;

            var repo2 = GetSimpleRegisteredSipRepository();
            RegisteredSip sip = repo2.GetRegisteredSipById(sipOnline.Id);
            sip.DisplayName += "¤";
            repo.UpdateRegisteredSip(sip);

            sipOnline = repo.GetCachedRegisteredSips().First(rs => rs.Id == sip.Id);
            Assert.AreNotEqual(updated, sipOnline.Updated);
        }

        [Test, Explicit]
        public void getRegisteredSipsOnline_should_be_updated_by_codec_registration()
        {
            var repo = GetCachedRegisteredSipRepository();
            var sipsOnline = repo.GetCachedRegisteredSips();
            var sipOnline = sipsOnline.First();
            var updated = sipOnline.Updated;

            var repo2 = GetSimpleRegisteredSipRepository();
            var sip = repo2.GetRegisteredSipById(sipOnline.Id);

            Thread.Sleep(1200);

            var newSip = new RegisteredSip
            {
                IP = sip.IP,
                Port = sip.Port,
                ServerTimeStamp = sip.ServerTimeStamp,
                SIP = sip.SIP,
                UserAgentHead = sip.UserAgentHead,
                Username = sip.Username,
                DisplayName = sip.DisplayName,
                Expires = sip.Expires
            };

            var changeStatus = repo.UpdateRegisteredSip(newSip);
            Assert.AreEqual(KamailioMessageChangeStatus.NothingChanged, changeStatus);

            sipOnline = repo.GetCachedRegisteredSips().First();
            var updated2 = sipOnline.Updated;

            Assert.AreNotEqual(updated, updated2);
        }

        [Test, Explicit]
        public void GetCachedRegisteredSips()
        {
            var repo = GetCachedRegisteredSipRepository();
            var sipsOnline = repo.GetCachedRegisteredSips();

            Assert.IsNotNull(sipsOnline);
            Assert.IsTrue(sipsOnline.Any());
        }

        [Test, Explicit]
        public void GetRegisteredSipsOnline()
        {
            var repo = GetCachedRegisteredSipRepository();
            var sipsOnline = repo.GetCachedRegisteredSips();

            Assert.IsNotNull(sipsOnline);
            Assert.IsTrue(sipsOnline.Any());
        }

        private IRegisteredSipRepository GetCachedRegisteredSipRepository()
        {
            return new CachedRegisteredSipRepository(
                new CachingService(), 
                new RegisteredSipRepository(
                    new SettingsManager(new SettingsRepository(new CachingService())),
                    new LocationManager(new LocationRepository(new CachingService())),
                    new MetaRepository(new CachingService()),
                    new CachingService())
                );
        }

        private ISimpleRegisteredSipRepository GetSimpleRegisteredSipRepository()
        {
            return new SimpleRegisteredSipRepository(new SettingsManager(new SettingsRepository(new CachingService())), new CachingService());
        }
    }
}