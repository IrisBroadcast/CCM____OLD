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

﻿using CCM.Core.Extensions;
using NUnit.Framework;

namespace CCM.UnitTests.CCM.Core.Extensions
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        public void LeftOf_At()
        {
            var s = "apanFlyger@acip.example.com";
            var r = s.LeftOf("@");
            Assert.AreEqual("apanFlyger", r);
        }

        [Test]
        public void LeftOf_String()
        {
            var s = "apanFlyger@acip.example.com";
            var r = s.LeftOf(".com");
            Assert.AreEqual("apanFlyger@acip.example", r);
        }

        [Test]
        public void LeftOf_String2()
        {
            var s = "grodlår i låda";
            var r = s.LeftOf("r i");
            Assert.AreEqual("grodlå", r);
        }

        [Test]
        public void LeftOf_EmptyUntilString()
        {
            var s = "grodlår i låda";
            var r = s.LeftOf("");
            Assert.AreEqual("grodlår i låda", r);
        }

        [Test]
        public void LeftOf_EmptyString()
        {
            var s = "";
            var r = s.LeftOf("@");
            Assert.AreEqual("", r);
        }

        [Test]
        public void LeftOf_NoMatch()
        {
            var s = "Gummiboll";
            var r = s.LeftOf("@");
            Assert.AreEqual("Gummiboll", r);
        }


        [Test]
        public void IsNumeric_123()
        {
            var s = "123";
            var r = s.IsNumeric();
            Assert.IsTrue(r);
        }

        [Test]
        public void IsNumeric_StringWithLetters()
        {
            var s = "12Mössa3";
            var r = s.IsNumeric();
            Assert.IsFalse(r);
        }

        [Test]
        public void IsNumeric_Numeric_With_Comma()
        {
            var s = "123,4";
            var r = s.IsNumeric();
            Assert.IsFalse(r);
        }

        [Test]
        public void IsNumeric_VeryLongNumericString()
        {
            var s = "12323427389237497654678684623784623786427836423423423423423423";
            var r = s.IsNumeric();
            Assert.IsTrue(r);
        }
    }
}