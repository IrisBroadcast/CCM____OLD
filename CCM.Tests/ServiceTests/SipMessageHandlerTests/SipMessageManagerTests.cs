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
using CCM.Core.Kamailio;
using CCM.Core.Kamailio.Messages;
using CCM.Data.Repositories;
using Ninject;
using NUnit.Framework;

namespace CCM.Tests.ServiceTests.SipMessageHandlerTests
{

    [TestFixture, Ignore("")]
    public class SipMessageManagerTests : SipMessageHandlerTestsBase
    {
        [SetUp]
        public void Setup()
        {
            _sipMessageManager = kernel.Get<KamailioMessageManager>();
            _sipRep = kernel.Get<RegisteredSipRepository>();
        }

        [Test]
        public void test_codec_registration()
        {
            var userName = "patpet2@acip.example.com";

            DeleteExisting(userName);

            // Add new
            var sipMessage = new KamailioRegistrationMessage()
            {
                Ip = GetRandomLocationIpAddress(),
                Port = 5060,
                UnixTimeStamp = GetUnixTimeStamp(DateTime.Now),
                Sip = new SipUri(userName),
                UserAgent = "ProntoNet LC v6.8.1",
                Username = userName,
                ToDisplayName = "Test",
                Expires = 60
            };

            // Assert
            var result = _sipMessageManager.RegisterSip(sipMessage);
            Assert.AreEqual(KamailioMessageChangeStatus.CodecAdded, result.ChangeStatus);

            // Update only timestamp. Should return nothing changed.
            // Act
            sipMessage.UnixTimeStamp = GetUnixTimeStamp(DateTime.Now.AddSeconds(1));
            // Assert
            result = _sipMessageManager.RegisterSip(sipMessage);
            Assert.AreEqual(KamailioMessageChangeStatus.NothingChanged, result.ChangeStatus);

            // Update location. Should return status updated.
            // Act
            sipMessage.Ip = GetRandomLocationIpAddress();
            // Assert
            result = _sipMessageManager.RegisterSip(sipMessage);
            Assert.AreEqual(KamailioMessageChangeStatus.CodecUpdated, result.ChangeStatus);

            // Only timestamp
            sipMessage.UnixTimeStamp = GetUnixTimeStamp(DateTime.Now.AddSeconds(2));
            result = _sipMessageManager.RegisterSip(sipMessage);
            Assert.AreEqual(KamailioMessageChangeStatus.NothingChanged, result.ChangeStatus);

            // Update display name. Should return status updated.
            // Act
            sipMessage.ToDisplayName = "New display name";
            // Assert
            result = _sipMessageManager.RegisterSip(sipMessage);
            Assert.AreEqual(KamailioMessageChangeStatus.CodecUpdated, result.ChangeStatus);

            // Update user agent
            sipMessage.UserAgent = "Quantum/3.4.3";
            result = _sipMessageManager.RegisterSip(sipMessage);
            Assert.AreEqual(KamailioMessageChangeStatus.CodecUpdated, result.ChangeStatus);

            // Only timestamp
            sipMessage.UnixTimeStamp = GetUnixTimeStamp(DateTime.Now.AddSeconds(3));
            result = _sipMessageManager.RegisterSip(sipMessage);
            Assert.AreEqual(KamailioMessageChangeStatus.NothingChanged, result.ChangeStatus);

        }

      
    }
}
