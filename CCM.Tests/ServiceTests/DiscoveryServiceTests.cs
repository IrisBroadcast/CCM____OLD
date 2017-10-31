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
using System.Diagnostics;
using System.IO;
using System.Linq;
using CCM.Core.Discovery;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Managers;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Managers;
using CCM.Core.Service;
using CCM.Data.Radius;
using CCM.Data.Repositories;
using LazyCache;
using Newtonsoft.Json;
using Ninject;
using NUnit.Framework;

namespace CCM.Tests.ServiceTests
{
    [TestFixture, Ignore("")]
    public class DiscoveryServiceTests
    {
        #region Plumbing
        private StandardKernel _kernel;
        private DiscoveryService _discoveryService;

        private static StandardKernel GetKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<IAppCache>().To<CachingService>();
            kernel.Bind<IRegisteredSipRepository>().To<RegisteredSipRepository>();
            kernel.Bind<IRadiusUserRepository>().To<RadiusUserRepository>();
            kernel.Bind<ICcmUserManager>().To<CcmUserManager>();
            kernel.Bind<ICcmUserRepository>().To<UserRepository>();
            kernel.Bind<ISettingsRepository>().To<SettingsRepository>();
            kernel.Bind<ISettingsManager>().To<SettingsManager>();
            kernel.Bind<IProfileRepository>().To<ProfileRepository>();
            kernel.Bind<IFilterManager>().To<FilterManager>();
            kernel.Bind<IFilterRepository>().To<FilterRepository>();
            kernel.Bind<IMetaRepository>().To<MetaRepository>();
            kernel.Bind<IDiscoveryService>().To<DiscoveryService>();
            kernel.Bind<ILocationManager>().To<LocationManager>();
            kernel.Bind<ILocationRepository>().To<LocationRepository>();
            return kernel;
        }

        [OneTimeSetUp]
        public void SetupFixture()
        {
            _kernel = GetKernel();

        }

        [SetUp]
        public void Setup()
        {
            _discoveryService = _kernel.Get<DiscoveryService>();
        } 
        #endregion

        [Test]
        public void should_get_filters()
        {
            var filters = _discoveryService.GetFilters();

            foreach (var filter in filters)
            {
                Debug.WriteLine("Name:{0}, filter options:{1}", filter.Name, string.Join(", ", filter.Options));
            }

            // Kodgenerering
            //var fi = filters[0];
            //for (int i = 0; i < fi.FilterOptions.Count; i++)
            //{
            //    var o = fi.FilterOptions[i];
            //    Debug.WriteLine("Assert.AreEqual(\"{0}\", filters[0].FilterOptions[{1}].Name);", o.Name, i);    
            //}

            //fi = filters[1];
            //for (int i = 0; i < fi.FilterOptions.Count; i++)
            //{
            //    var o = fi.FilterOptions[i];
            //    Debug.WriteLine("Assert.AreEqual(\"{0}\", filters[0].FilterOptions[{1}].Name);", o.Name, i);
            //}

            Assert.AreEqual(2, filters.Count);

            Assert.AreEqual("1 Region", filters[0].Name);
            Assert.AreEqual(28, filters[0].Options.Count);
            
            Assert.AreEqual("Blekinge", filters[0].Options[0]);
            Assert.AreEqual("Dalarna", filters[0].Options[1]);
            Assert.AreEqual("Gotland", filters[0].Options[2]);
            Assert.AreEqual("Gävleborg", filters[0].Options[3]);
            Assert.AreEqual("Göteborg", filters[0].Options[4]);
            Assert.AreEqual("Halland", filters[0].Options[5]);
            Assert.AreEqual("Internationellt", filters[0].Options[6]);
            Assert.AreEqual("Jämtland", filters[0].Options[7]);
            Assert.AreEqual("Jönköping", filters[0].Options[8]);
            Assert.AreEqual("Kalmar", filters[0].Options[9]);
            Assert.AreEqual("Kristianstad", filters[0].Options[10]);
            Assert.AreEqual("Kronoberg", filters[0].Options[11]);
            Assert.AreEqual("Malmö", filters[0].Options[12]);
            Assert.AreEqual("Norrbotten", filters[0].Options[13]);
            Assert.AreEqual("Ospecificerad", filters[0].Options[14]);
            Assert.AreEqual("Sjuhärad", filters[0].Options[15]);
            Assert.AreEqual("Skandinavien", filters[0].Options[16]);
            Assert.AreEqual("Skaraborg", filters[0].Options[17]);
            Assert.AreEqual("Stockholm", filters[0].Options[18]);
            Assert.AreEqual("Sörmland", filters[0].Options[19]);
            Assert.AreEqual("Uppland", filters[0].Options[20]);
            Assert.AreEqual("Värmland", filters[0].Options[21]);
            Assert.AreEqual("Väst", filters[0].Options[22]);
            Assert.AreEqual("Västerbotten", filters[0].Options[23]);
            Assert.AreEqual("Västernorrland", filters[0].Options[24]);
            Assert.AreEqual("Västmanland", filters[0].Options[25]);
            Assert.AreEqual("Örebro", filters[0].Options[26]);
            Assert.AreEqual("Östergötland", filters[0].Options[27]);

            Assert.AreEqual("2 Kodartyp", filters[1].Name);
            Assert.AreEqual(7, filters[1].Options.Count);

            Assert.AreEqual("Externa", filters[1].Options[0]);
            Assert.AreEqual("Fordon", filters[1].Options[1]);
            Assert.AreEqual("OB / Portabla", filters[1].Options[2]);
            Assert.AreEqual("OBV / OB väskan", filters[1].Options[3]);
            Assert.AreEqual("Personliga", filters[1].Options[4]);
            Assert.AreEqual("Studio", filters[1].Options[5]);
            Assert.AreEqual("Test", filters[1].Options[6]);
        }


        [Test]
        public void should_get_profiles()
        {
            var profiles = _discoveryService.GetProfiles();

            foreach (var p in profiles)
            {
                Debug.WriteLine("Name:{0} Sdp:{1}", p.Name, p.Sdp);
            }

            Assert.AreEqual(9, profiles.Count);
            Assert.AreEqual("Studio", profiles[0].Name);
            Assert.AreEqual("EWAN", profiles[1].Name);
            Assert.AreEqual("Fordon Mono", profiles[2].Name);
            Assert.AreEqual("Fordon Stereo", profiles[3].Name);
        }

        [Test]
        public void should_get_useragents_vaxjo13_nofilters()
        {
            var filters = new List<KeyValuePair<string, string>>();
            
            var result = _discoveryService.GetUserAgents("vaxjo-16@acip.example.com", null, filters);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.UserAgents.Count > 0);
        }

        [Test]
        public void should_get_useragents_vaxjo13_stockholm_studio()
        {
            // Ca 250 ms lokalt

            var filters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("1 Region", "Stockholm"),
                new KeyValuePair<string, string>("2 Kodartyp", "Studio")
            };
            
            UserAgentsResultDto result = _discoveryService.GetUserAgents("vaxjo-13@acip.example.com", null, filters);
            AssertContentAreEqual(new UserAgentsResultDto() { UserAgents = result.UserAgents, Profiles = result.Profiles }, @"vaxjo13_stockholm_studio.json");

            List<string> recList = result.UserAgents.SelectMany(ua => ua.Profiles ?? Enumerable.Empty<string>()).ToList();
            var d = recList.Distinct().ToList();

            Assert.AreEqual(31, result.UserAgents.Count);
            Assert.AreEqual(8, result.Profiles.Count);
        }

        [Test]
        public void should_get_useragents_vaxjo13_kronoberg_ob()
        {
            var filters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("1 Region", "Kronoberg"),
                new KeyValuePair<string, string>("2 Kodartyp", "OB / Portabla")
            };

            var sr = _discoveryService.GetUserAgents("vaxjo-13@acip.example.com", null, filters);

            AssertContentAreEqual(new UserAgentsResultDto { UserAgents = sr.UserAgents, Profiles = sr.Profiles }, @"vaxjo13_kronoberg_ob.json");

            List<string> recList = sr.UserAgents.SelectMany(ua => ua.Profiles ?? Enumerable.Empty<string>()).ToList();
            var d = recList.Distinct().ToList();
            
            Assert.AreEqual(3, sr.UserAgents.Count);
            Assert.AreEqual(1, sr.Profiles.Count);
        }

        private void AssertContentAreEqual(UserAgentsResultDto srDiscovery, string fileName)
        {
            var s = JsonConvert.SerializeObject(srDiscovery);
            var expected = File.ReadAllText(Path.Combine(@"..\..\Testdata", fileName));
            Assert.AreEqual(expected, s);
        }

    }
}
