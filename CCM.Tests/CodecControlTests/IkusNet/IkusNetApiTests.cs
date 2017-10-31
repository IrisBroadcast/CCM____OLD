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

﻿using CCM.CodecControl.Prodys.IkusNet;
using CCM.Core.CodecControl.Entities;
using CCM.Core.CodecControl.Enums;
using NUnit.Framework;

namespace CCM.Tests.CodecControlTests.IkusNet
{
    [TestFixture]
    public class IkusNetApiTests
    {
        private string _hostAddress;

        [SetUp]
        public void SetUp()
        {
            _hostAddress = "192.0.2.237";
        }

        [Test]
        public void GetDeviceName()
        {
            var sut = new IkusNetApi();
            var deviceName = sut.GetDeviceName(_hostAddress);
            Assert.AreEqual("MTU 25", deviceName);
        }

        [Test]
        public void GetInputLevel()
        {
            var sut = new IkusNetApi();

            sut.SetInputGainLevel(_hostAddress, 0, 6);

            var level = sut.GetInputGainLevel(_hostAddress, 0);
            Assert.AreEqual(6, level);

            sut.SetInputGainLevel(_hostAddress, 0, 4);

            level = sut.GetInputGainLevel(_hostAddress, 0);
            Assert.AreEqual(4, level);

        }

        [Test]
        public void GetLineStatus()
        {
            var sut = new IkusNetApi();
            
            LineStatus lineStatus = sut.GetLineStatus(_hostAddress, 0);
            Assert.AreEqual("", lineStatus.RemoteAddress);
            Assert.AreEqual(LineStatusCode.NoPhysicalLine, lineStatus.StatusCode);
            Assert.AreEqual(DisconnectReason.None, lineStatus.DisconnectReason);
            Assert.AreEqual(IpCallType.Invalid, lineStatus.IpCallType);
        }

        [Test, Ignore("To avoid unintentional calling")]
        public void Call()
        {
            var sut = new IkusNetApi();

            var call = new Call()
            {
                Address = "sto-s17-01@acip.example.com",
                CallType = IpCallType.UnicastBidirectional,
                Codec = Codec.Program,
                Content = CallContent.Audio,
                Profile = "Studio"
            };

            bool result = sut.Call(_hostAddress, call);
            Assert.IsTrue(result);
        }

        [Test, Ignore("To avoid unintentional hangup")]
        public void Hangup()
        {
            var sut = new IkusNetApi();
            bool result = sut.HangUp(_hostAddress, Codec.Program);
            Assert.IsTrue(result);
        }
    }
}