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

﻿using CCM.Core.Kamailio;
using CCM.Data.Repositories;
using Ninject;
using NUnit.Framework;

namespace CCM.Tests.ServiceTests.SipMessageHandlerTests
{
    [TestFixture, Ignore("")]
    public class KamailioEventTests : SipMessageHandlerTestsBase
    {

        [Test]
        public void should_register_växjö_10()
        {
            var sipMessageManager = kernel.Get<KamailioMessageManager>();

            var sipMessage = CreateSipMessage("192.0.2.82", "ProntoNet LC v6.8.1", "vaxjo-10@acip.example.com", "Växjö 10");
            sipMessageManager.RegisterSip(sipMessage);

            var sipRep = kernel.Get<RegisteredSipRepository>();
            var sip = sipRep.Single(rs => rs.SIP == "vaxjo-10@acip.example.com");

            Assert.IsNotNull(sip);
            Assert.AreEqual("192.0.2.82", sip.IP);
            Assert.AreEqual("RH Växjö", sip.Location.Name);
            Assert.AreEqual("ProntoNet", sip.UserAgent.Name);
            Assert.AreEqual("vaxjo-10@acip.example.com", sip.User.UserName);
        }

       

        [Test]
        public void should_only_update_timestamp_when_same_registered_sip_and_ccm_is_unchanged()
        {
            // TODO: Implement this
        }


    }
}
