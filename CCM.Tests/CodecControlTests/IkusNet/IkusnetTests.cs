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

﻿using CCM.CodecControl;
using CCM.Core.CodecControl.Entities;
using CCM.Core.CodecControl.Interfaces;
using NUnit.Framework;

namespace CCM.Tests.CodecControlTests.IkusNet
{
    [TestFixture]
    public class IkusnetTests
    {
        private static readonly CodecInformation CodecInformation = new CodecInformation
        {
            SipAddress = "sto-s17-01@acip.example.com",
            Api = "IkusNet",
            Ip = "192.0.2.30" // sto-s17-01
        };

        private ICodecManager GetCodecManager()
        {
            var settingsManager = new DummySettingsManager();
            return new CodecManager(settingsManager);
        }

        [Test]
        public void Ikusnet_GetLineStatus()
        {
            var manager = GetCodecManager();
            var lineStatus = manager.GetLineStatus(CodecInformation, 0);
            Assert.IsNotNull(lineStatus);
        }

    }

}