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

using CCM.Core.Helpers;
using NUnit.Framework;

namespace CCM.UnitTests.CCM.Core
{
    [TestFixture]
    public class DisplayNameHelperTests
    {
        [Test]
        public void should_hide_external_numbers()
        { 
            var s = "0090510@acip.example.com";
            var result = DisplayNameHelper.AnonymizePhonenumber(s);
            Assert.AreEqual("Externt nummer", result);
        }

        [Test]
        public void should_show_internal_number_without_host()
        {
            var s = "840200@acip.example.com";
            var result = DisplayNameHelper.AnonymizePhonenumber(s);
            Assert.AreEqual("840200", result);
        }

        [Test]
        public void should_not_anonymize_standard_sipaddresses()
        {
            var s = "sto-s17-01@acip.example.com";
            var result = DisplayNameHelper.AnonymizePhonenumber(s);
            Assert.AreEqual(s, result);
        }

        [Test]
        public void should_handle_empty_string()
        {
            var s = string.Empty;
            var result = DisplayNameHelper.AnonymizePhonenumber(s);
            Assert.AreEqual(s, result);
        }

    }
}