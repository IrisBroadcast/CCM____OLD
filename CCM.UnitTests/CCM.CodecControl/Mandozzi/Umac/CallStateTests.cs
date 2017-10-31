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

using CCM.CodecControl.Mandozzi.Umac;
using CCM.Core.CodecControl.Enums;
using NUnit.Framework;

namespace CCM.UnitTests.CCM.CodecControl.Mandozzi.Umac
{
    [TestFixture]
    public class CallStateTests
    {
        [Test]
        public void should_parse_call_state_when_idle()
        {
            string s = " CALL_STATE disconnected [tx: idle, rx: ödla]\r\n";

            var callInfo = new UmacCallState(s);

            Assert.AreEqual(LineStatusCode.Disconnected, callInfo.State);
            Assert.AreEqual("idle", callInfo.TxProtocol);
            Assert.AreEqual("ödla", callInfo.RxProtocol);
        }

        [Test]
        public void should_parse_call_state_when_connected()
        {
            string s = " CALL_STATE connected [tx: sip, rx: sup]";

            var callInfo = new UmacCallState(s);

            Assert.AreEqual(LineStatusCode.ConnectedCalled, callInfo.State);
            Assert.AreEqual("sip", callInfo.TxProtocol);
            Assert.AreEqual("sup", callInfo.RxProtocol);
        }
    }
}

