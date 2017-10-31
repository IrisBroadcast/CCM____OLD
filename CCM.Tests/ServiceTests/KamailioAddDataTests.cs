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
using CCM.Core.Entities;
using CCM.Core.Kamailio;
using CCM.Core.Kamailio.Messages;
using CCM.Core.Managers;
using CCM.Tests.ServiceTests.SipMessageHandlerTests;
using Ninject;
using NUnit.Framework;

namespace CCM.Tests.ServiceTests
{
    [TestFixture, Ignore("")]
    public class KamailioAddDataTests : SipMessageHandlerTestsBase
    {

        [Test, Explicit]
        public void register_växjö_10()
        {
            var sipMessageManager = kernel.Get<KamailioMessageManager>();
            var sipMessage = CreateSipMessage("192.0.2.82", "ProntoNet LC v6.8.1", "vaxjo-10@acip.example.com", "Växjö 10");
            sipMessageManager.RegisterSip(sipMessage);
        }

        [Test, Explicit]
        public void register_växjö_11()
        {
            var sipMessageManager = kernel.Get<KamailioMessageManager>();
            var sipMessage = CreateSipMessage("192.0.2.83", "ProntoNet LC v6.8.1", "vaxjo-11@acip.example.com", "Växjö 11");
            sipMessageManager.RegisterSip(sipMessage);
        }

        [Test, Explicit]
        public void StartCall()
        {
            var sipMessageManager = kernel.Get<KamailioMessageManager>();
            var sipMessage = CreateCallStartMessage("ob142254@acip.example.com", "sto-04@acip.example.com");
            sipMessageManager.RegisterCall(sipMessage);
        }

        public KamailioDialogMessage CreateCallStartMessage(string sip, string requestedSip)
        {
            return new KamailioDialogMessage
            {
                FromSipUri = new SipUri(sip),
                ToSipUri = new SipUri(requestedSip),
                CallId = "mycallid",
                ToTag = "ToTagTest",
                FromTag = "FromTagTest"
            };
        }


    }
}
