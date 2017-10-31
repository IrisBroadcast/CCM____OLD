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

﻿using CCM.Core.Entities;
using CCM.Core.Helpers;
using NUnit.Framework;

namespace CCM.Tests
{
    [TestFixture]
    public class MetadataHelperTests
    {
        [Test]
        public void should_get_property_value()
        {
            var obj = new Location() { ShortName = "kort namn" };
            var s = MetadataHelper.GetPropertyValue(obj, "ShortName");
            Assert.AreEqual("kort namn", s);
        }

        [Test]
        public void should_handle_properties_with_null_value()
        {
            var obj = new Location() { ShortName = null };
            var s = MetadataHelper.GetPropertyValue(obj, "ShortName");
            Assert.AreEqual(string.Empty, s);
        }

        [Test]
        public void should_handle_properties_with_null_value_in_object_hierarcy()
        {
            var obj = new RegisteredSip() {Location = null };
            var s = MetadataHelper.GetPropertyValue(obj, "Location.ShortName");
            Assert.AreEqual(string.Empty, s);
        }

        [Test]
        public void should_handle_object_hierarchy()
        {
            var obj = new RegisteredSip() { Location = new Location() { ShortName = "kort namn" } };
            var s = MetadataHelper.GetPropertyValue(obj, "Location.ShortName");
            Assert.AreEqual("kort namn", s);
        }

        [Test]
        public void should_handle_invalid_path()
        {
            var obj = new RegisteredSip() { Location = new Location() { ShortName = "kort namn" } };
            var s = MetadataHelper.GetPropertyValue(obj, "Location.SkjortName");
            Assert.AreEqual("", s);
        }

        [Test]
        public void should_handle_null_object()
        {
            var s = MetadataHelper.GetPropertyValue(null, "Location.SkjortName");
            Assert.AreEqual("", s);
        }

        [Test]
        public void should_handle_null_path()
        {
            var obj = new RegisteredSip() { Location = new Location() { ShortName = "kort namn" } };
            var s = MetadataHelper.GetPropertyValue(obj, null);
            Assert.AreEqual("", s);
        }

        [Test]
        public void should_handle_empty_path()
        {
            var obj = new RegisteredSip() { Location = new Location() { ShortName = "kort namn" } };
            var s = MetadataHelper.GetPropertyValue(obj, string.Empty);
            Assert.AreEqual("", s);
        }
    }
}
