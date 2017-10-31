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
using System.Linq;
using CCM.Core.Interfaces.Managers;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Kamailio;
using CCM.Core.Kamailio.Messages;
using CCM.Core.Managers;
using CCM.Core.Security;
using CCM.Core.Service;
using CCM.Data;
using CCM.Data.Repositories;
using Ninject;
using NUnit.Framework;

namespace CCM.Tests.ServiceTests.SipMessageHandlerTests
{
    public class SipMessageHandlerTestsBase
    {
        #region Plumbing
        protected StandardKernel kernel;
        protected DiscoveryService discoveryService;
        protected KamailioMessageManager _sipMessageManager;
        protected RegisteredSipRepository _sipRep;

        protected static StandardKernel GetKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<ISipMessageManager>().To<KamailioMessageManager>();
            kernel.Bind<IRegisteredSipRepository>().To<RegisteredSipRepository>();
            kernel.Bind<ICallRepository>().To<CallRepository>();
            kernel.Bind<ICallHistoryRepository>().To<CallHistoryRepository>();
            kernel.Bind<ISettingsRepository>().To<SettingsRepository>();
            kernel.Bind<ISettingsManager>().To<SettingsManager>();
            kernel.Bind<ILocationManager>().To<LocationManager>();
            kernel.Bind<ILocationRepository>().To<LocationRepository>();
            kernel.Bind<IMetaRepository>().To<MetaRepository>();
            return kernel;
        }

        [OneTimeSetUp]
        public void SetupFixture()
        {
            kernel = GetKernel();

        }
        #endregion

        public void DeleteExisting(string userName)
        {
            var existingSip = _sipRep.Single(rs => rs.SIP == userName);
            if (existingSip != null)
            {
                _sipRep.DeleteRegisteredSip(existingSip.Id);
            }
        }

        public KamailioRegistrationMessage CreateSipMessage(string ip, string userAgent, string sip, string displayName)
        {
            return new KamailioRegistrationMessage
            {
                Ip = ip,
                Port = 5060,
                UnixTimeStamp = GetUnixTimeStamp(DateTime.Now),
                Sip = new SipUri(sip),
                UserAgent = userAgent,
                Username = sip,
                ToDisplayName = displayName,
                Expires = 60
            };
        }

        public static long GetUnixTimeStamp(DateTime dateTime)
        {
            return (long)dateTime.Subtract(DateTime.Parse("1970-01-01")).TotalSeconds;
        }

        public static string GetRandomUserName()
        {
            var users = new CcmDbContext(null).Users.Where(u => u.UserType == UserType.SIP).Select(u => u.UserName).ToList();
            int randomIndex = new Random().Next(0, users.Count);
            var userName = users[randomIndex];
            return userName;
        }

        public static string GetRandomLocationIpAddress()
        {
            var locations = new CcmDbContext(null).Locations.Select(l => l.Net_Address_v4).ToList();
            int randomIndex = new Random().Next(0, locations.Count);
            var locationAddress = locations[randomIndex];
            return locationAddress;
        }

    }
}