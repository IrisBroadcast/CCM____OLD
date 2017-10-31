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
using CCM.Core.CodecControl.Entities;
using NUnit.Framework;

namespace CCM.Tests.CodecControlTests.Umac
{
    [TestFixture]
    public class UmacTests
    {
        private string ip = "192.0.2.107";
        private UmacApi _sut; // System Under Test

        [SetUp]
        public void SetUp()
        {
            _sut = new UmacApi();
        }
        
        [Test]
        public void UMAC_GetLineStatus()
        {
            var lineStatus = _sut.GetLineStatus(ip, 0);
            Assert.IsNotNull(lineStatus);

        }

        [Test]
        public void UMAC_Call_Telefon()
        {
            var call = new Call()
            {
                Address = "sto-s17-01@acip.example.com",
                Profile = "Telefon"
            };

            var result = _sut.Call(ip, call);
            Assert.IsTrue(result);
        }

        [Test]
        public void UMAC_HangUp()
        {
            var result = _sut.HangUp(ip, 0);
            Assert.IsTrue(result);
        }
    }
}