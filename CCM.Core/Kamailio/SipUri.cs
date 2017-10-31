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

using System;
using System.Text.RegularExpressions;

namespace CCM.Core.Kamailio
{
    public class SipUri
    {
        private readonly string pattern = @"^((?<scheme>[a-zA-Z]*):)?" // scheme
                          + @"((?<user>[a-zA-Z0-9\-\.\!\~\*\'\(\)&=\+\$,;\?\/\%_]+)@)?" // user
                          + @"(?<host>[^;\?:]*)(:(?<port>[\d]+))?" // host, port
                          + @"(;(?<params>[\S]*))?$"; // parameters

        public string DisplayName { get; set; }
        public string Schema { get; set; }
        public string User { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string Parameters { get; set; }
        public string UserAtHost { get { return string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Host) ? string.Empty : string.Format("{0}@{1}", User, Host); } }

        private readonly string _sipString; // Orginalsträngen

        public SipUri(string sipAddress)
        {
            try
            {
                // Handle display name. TODO: Hur gör man detta med RegExp?
                string displayName = "";
                if (sipAddress.Contains("<"))
                {
                    var indexOfLessThan = sipAddress.IndexOf('<');
                    var indexOfMoreThan = sipAddress.IndexOf('>');
                    displayName = sipAddress.Substring(0, indexOfLessThan).Trim();
                    sipAddress = sipAddress.Substring(indexOfLessThan + 1, indexOfMoreThan - indexOfLessThan - 1);
                }

                _sipString = sipAddress;
                var regExp = new Regex(pattern, RegexOptions.IgnoreCase);

                Match match = regExp.Match(sipAddress);
                if (match.Success)
                {
                    DisplayName = displayName;
                    Schema = match.Groups["scheme"].Value;
                    User = match.Groups["user"].Value;
                    Host = match.Groups["host"].Value;
                    Port = match.Groups["port"].Value;
                    Parameters = match.Groups["params"].Value;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Invalid SIP address", ex);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", UserAtHost, _sipString);
        }
    }
}