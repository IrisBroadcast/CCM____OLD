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

using CCM.Core.Kamailio.Messages;
using CCM.Core.Kamailio.Parser;
using NUnit.Framework;

namespace CCM.UnitTests.CCM.Core.Kamailio
{
    [TestFixture]
    public class KamailioDataParserTests
    {
        [Test]
        public void should_handle_empty_data()
        {
            var sut = new KamailioDataParser();
            var data = sut.ParseToKamailioData("");

            Assert.IsNull(data);
        }

        [Test]
        public void should_handle_string_without_divider()
        {
            var sut = new KamailioDataParser();
            var data = sut.ParseToKamailioData("en sträng utan pipetecken");

            Assert.IsNull(data);
        }

        [Test]
        public void should_handle_string_with_only_header()
        {
            var sut = new KamailioDataParser();
            var data = sut.ParseToKamailioData("dialog|");

            Assert.IsNotNull(data);
            Assert.AreEqual(KamailioMessageType.Dialog, data.MessageType);
        }

        [Test]
        public void should_handle_correct_message()
        {
            var sut = new KamailioDataParser();
            var data = sut.ParseToKamailioData("dialog|dstat::start|hashid::abc|hashent::def|ci::ghi");

            Assert.AreEqual(KamailioMessageType.Dialog, data.MessageType);
            Assert.AreEqual("start", data.Fields["dstat"]);
            Assert.AreEqual("abc", data.Fields["hashid"]);
            Assert.AreEqual("def", data.Fields["hashent"]);
            Assert.AreEqual("ghi", data.Fields["ci"]);
        }

        [Test]
        public void should_exclude_field_with_invalid_form()
        {
            var sut = new KamailioDataParser();
            var data = sut.ParseToKamailioData("dialog|dstat::start|hashid::abc|hashent::apa::banan");
            Assert.IsFalse(data.Fields.ContainsKey("hashent"));
        }

        [Test]
        public void should_exclude_empty_field_key()
        {
            var sut = new KamailioDataParser();
            var data = sut.ParseToKamailioData("dialog|dstat::start|::123|hashent::banan");
            Assert.IsTrue(data.Fields.Count == 2);
        }

        [Test]
        public void should_accept_empty_field_value()
        {
            var sut = new KamailioDataParser();
            var data = sut.ParseToKamailioData("dialog|dstat::start|hashid::|hashent::apa::banan");
            Assert.IsTrue(data.Fields.ContainsKey("hashid"));
        }

        [Test]
        public void should_replace_null_string_with_empty_string()
        {
            var sut = new KamailioDataParser();
            var data = sut.ParseToKamailioData("dialog|dstat::start|hashid::<null>|hashent::");
            Assert.IsTrue(data.Fields.ContainsKey("hashid"));
            Assert.AreEqual(string.Empty, data.Fields["hashid"]);
        }

    }
}