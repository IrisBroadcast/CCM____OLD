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

﻿using CCM.CodecControl.Mandozzi.Umac;
using NUnit.Framework;

namespace CCM.UnitTests.CCM.CodecControl.Mandozzi.Umac
{
    [TestFixture]
    public class CallInfoTests
    {
        [Test]
        public void should_parse_call_info_when_connected()
        {
            string s = " CONFIRMED [sip:sto-s17-01@acip.example.com]\r\nremote info : sip:sto-s17-01@acip.example.com\r\ncontact info: \"Studio 17 01\" <sip:sto-s17-01@192.0.2.15:5060;transport=UDP>\r\ncodec tx: opus 48k mono 64k [dsp], rx: opus 48k mono 32k [dsp]\r\n";

            var callInfo = new UmacCallInfo(s);

            Assert.AreEqual("sip:sto-s17-01@acip.example.com", callInfo.ConnectedTo);
            Assert.AreEqual("CONFIRMED", callInfo.State);
            Assert.AreEqual("opus 48k mono 64k [dsp]", callInfo.Tx);
            Assert.AreEqual("opus 48k mono 32k [dsp]", callInfo.Rx);
        }
    }
}