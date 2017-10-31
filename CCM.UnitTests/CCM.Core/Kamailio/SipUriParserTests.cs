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
using NUnit.Framework;

namespace CCM.UnitTests.CCM.Core.Kamailio
{
    [TestFixture]
    public class SipUriParserTests
    {
        [Test]
        public void should_parse_sip_address()
        {
            var sipUri = "sip:840202@gw.acip.example.com:5060;user=phone;transport=udp";
            var parser = new SipUri(sipUri);
            Assert.AreEqual("sip", parser.Schema);
            Assert.AreEqual("840202", parser.User);
            Assert.AreEqual("gw.acip.example.com", parser.Host);
            Assert.AreEqual("840202@gw.acip.example.com", parser.UserAtHost);
            Assert.AreEqual("5060", parser.Port);
            Assert.AreEqual("user=phone;transport=udp", parser.Parameters);
        }

        [Test]
        public void should_handle_missing_schema()
        {
            var sipUri = "840202@gw.acip.example.com:5060;user=phone;transport=udp";
            var parser = new SipUri(sipUri);
            Assert.AreEqual("", parser.Schema);
            Assert.AreEqual("840202", parser.User);
            Assert.AreEqual("gw.acip.example.com", parser.Host);
            Assert.AreEqual("840202@gw.acip.example.com", parser.UserAtHost);
            Assert.AreEqual("5060", parser.Port);
            Assert.AreEqual("user=phone;transport=udp", parser.Parameters);
        }

        [Test]
        public void should_handle_missing_port()
        {
            var sipUri = "sip:840202@gw.acip.example.com;user=phone;transport=udp";
            var parser = new SipUri(sipUri);
            Assert.AreEqual("sip", parser.Schema);
            Assert.AreEqual("840202", parser.User);
            Assert.AreEqual("gw.acip.example.com", parser.Host);
            Assert.AreEqual("840202@gw.acip.example.com", parser.UserAtHost);
            Assert.AreEqual("", parser.Port);
            Assert.AreEqual("user=phone;transport=udp", parser.Parameters);
        }

        [Test]
        public void should_parse_sip_address_without_user()
        {
            var sipUri = "sip:gw.acip.example.com:5060;user=phone;transport=udp";
            var parser = new SipUri(sipUri);
            Assert.AreEqual("sip", parser.Schema);
            Assert.AreEqual("", parser.User);
            Assert.AreEqual("gw.acip.example.com", parser.Host);
            Assert.AreEqual("", parser.UserAtHost);
            Assert.AreEqual("5060", parser.Port);
            Assert.AreEqual("user=phone;transport=udp", parser.Parameters);
        }

        [Test]
        public void should_handle_underscore_address()
        {
            var sipUri = "sip:carl_magnus.von_der_pahlen@acip.example.com";
            var parser = new SipUri(sipUri);
            Assert.AreEqual("sip", parser.Schema);
            Assert.AreEqual("carl_magnus.von_der_pahlen", parser.User);
            Assert.AreEqual("acip.example.com", parser.Host);
            Assert.AreEqual("carl_magnus.von_der_pahlen@acip.example.com", parser.UserAtHost);
            Assert.AreEqual("", parser.Port);
            Assert.AreEqual("", parser.Parameters);
        }

        [Test]
        public void should_handle_missing_parameters()
        {
            var sipUri = "sip:840202@gw.acip.example.com:5060";
            var parser = new SipUri(sipUri);
            Assert.AreEqual("sip", parser.Schema);
            Assert.AreEqual("840202", parser.User);
            Assert.AreEqual("gw.acip.example.com", parser.Host);
            Assert.AreEqual("840202@gw.acip.example.com", parser.UserAtHost);
            Assert.AreEqual("5060", parser.Port);
            Assert.AreEqual("", parser.Parameters);
        }

        [Test]
        public void should_handle_displayname()
        {
            var sipUri = "Anders Stenberg <sip:andste@acip.example.com:5060;user=phone;transport=udp>";
            var parser = new SipUri(sipUri);
            Assert.AreEqual("Anders Stenberg", parser.DisplayName);
            Assert.AreEqual("sip", parser.Schema);
            Assert.AreEqual("andste", parser.User);
            Assert.AreEqual("acip.example.com", parser.Host);
            Assert.AreEqual("andste@acip.example.com", parser.UserAtHost);
            Assert.AreEqual("5060", parser.Port);
            Assert.AreEqual("user=phone;transport=udp", parser.Parameters);
        }

        [Test]
        public void should_handle_displayname_without_schema()
        {
            var sipUri = "Anders Stenberg <andste@acip.example.com:5060;user=phone;transport=udp>";
            var parser = new SipUri(sipUri);
            Assert.AreEqual("Anders Stenberg", parser.DisplayName);
            Assert.AreEqual("", parser.Schema);
            Assert.AreEqual("andste", parser.User);
            Assert.AreEqual("acip.example.com", parser.Host);
            Assert.AreEqual("andste@acip.example.com", parser.UserAtHost);
            Assert.AreEqual("5060", parser.Port);
            Assert.AreEqual("user=phone;transport=udp", parser.Parameters);
        }
    }
}