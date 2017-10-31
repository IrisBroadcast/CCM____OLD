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

using CCM.Core.Kamailio;
using CCM.Data.Repositories;
using Ninject;
using NUnit.Framework;

namespace CCM.Tests.ServiceTests.SipMessageHandlerTests
{
    [TestFixture, Ignore("")]
    public class HandleMessageTests : SipMessageHandlerTestsBase
    {
        [Test]
        public void should_register_new_codec()
        {
            var sipMessageManager = kernel.Get<KamailioMessageManager>();
            var sipRep = kernel.Get<RegisteredSipRepository>();
            
            // ASSIGN
            var userName = "patpet2@acip.example.com";

            // Delete any already registered codec
            var existingSip = sipRep.Single(rs => rs.SIP == userName);
            if (existingSip != null)
            {
                sipRep.DeleteRegisteredSip(existingSip.Id);
            }
            
            var ipAddress = GetRandomLocationIpAddress();
            var displayName = "Test";

            var sipMessage = CreateSipMessage(ipAddress, "ME-UMAC2-M/0.255", userName, displayName);

            // ACT
            KamailioMessageHandlerResult result = sipMessageManager.DoHandleMessage(sipMessage);

            // ASSERT
            Assert.AreEqual(KamailioMessageChangeStatus.CodecAdded, result.ChangeStatus);

            var sip = sipRep.Single(rs => rs.SIP == userName);
            Assert.AreEqual(ipAddress, sip.IP);
            Assert.AreEqual(userName, sip.User.UserName);

            // Clean up
            sipRep.DeleteRegisteredSip(sip.Id);
        }

      


     
    }
}