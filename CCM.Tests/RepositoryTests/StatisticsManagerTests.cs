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
using System.Linq;
using CCM.Core.Cache;
using CCM.Core.Managers;
using CCM.Data.Repositories;
using LazyCache;
using NUnit.Framework;

namespace CCM.Tests.RepositoryTests
{
    [TestFixture]
    public class StatisticsManagerTests
    {
        [Test, Explicit]
        public void GetLocationStatistics()
        {
            var manager = new StatisticsManager(new CallHistoryRepository(new CachingService()), 
                new CodecTypeRepository(new CachingService()), 
                new OwnersRepository(new CachingService()), 
                new RegionRepository(new CachingService()), 
                new LocationRepository(new CachingService()), 
                new UserRepository(new CachingService()), 
                new SimpleRegisteredSipRepository(new SettingsManager(new SettingsRepository(new CachingService())), new CachingService()));

            var result = manager.GetLocationStatistics(DateTime.Parse("2016-06-10 00:00:00"),
                DateTime.Parse("2016-06-17 00:00:00"), Guid.Empty, Guid.Empty, Guid.Empty);

            foreach (var item in result.Where(i => i.NumberOfCalls > 0))
            {
                Console.WriteLine("{1} {2} {3:0.00} [{0}]", item.LocationName, item.NumberOfCalls, item.MaxSimultaneousCalls, item.TotaltTimeForCalls);
            }
        }
    }
}