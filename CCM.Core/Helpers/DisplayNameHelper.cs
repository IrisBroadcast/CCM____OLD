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

﻿using System;
using System.Linq;
using CCM.Core.Entities;
using CCM.Core.Entities.Specific;
using CCM.Core.Extensions;

namespace CCM.Core.Helpers
{
    public class DisplayNameHelper
    {
        public static string GetDisplayName(RegisteredSip registeredSip, string sipDomain)
        {
            return GetDisplayName(registeredSip.DisplayName, registeredSip.User != null ? registeredSip.User.DisplayName : string.Empty, string.Empty, registeredSip.Username, registeredSip.SIP, "", sipDomain);
        }

        public static string GetDisplayName(RegisteredSipDto registeredSip, string sipDomain)
        {
            return GetDisplayName(registeredSip.DisplayName, registeredSip.UserDisplayName, string.Empty, registeredSip.UserName, registeredSip.Sip, "", sipDomain);
        }

        public static string GetDisplayName(string primaryDisplayName, string secondaryDisplayName, string tertiaryDisplayName, string primaryUserName, string secondaryUserName, string tertiaryUserName,
            string homeDomain, string defaultDisplayName = "")
        {
            if (!string.IsNullOrWhiteSpace(primaryDisplayName)) { return primaryDisplayName; }
            if (!string.IsNullOrWhiteSpace(secondaryDisplayName)) { return secondaryDisplayName; }
            if (!string.IsNullOrEmpty(tertiaryDisplayName)) { return tertiaryDisplayName; }

            if (!string.IsNullOrEmpty(primaryUserName)) { return GetUserNameWithoutHomeDomain(primaryUserName, homeDomain); }
            if (!string.IsNullOrEmpty(secondaryUserName)) { return GetUserNameWithoutHomeDomain(secondaryUserName, homeDomain); }
            if (!string.IsNullOrEmpty(tertiaryUserName)) { return GetUserNameWithoutHomeDomain(tertiaryUserName, homeDomain); }

            return defaultDisplayName;
        }

        private static string GetUserNameWithoutHomeDomain(string userName, string homeDomain)
        {
            var domainIndex = userName.IndexOf(string.Format("@{0}", homeDomain), StringComparison.CurrentCulture);
            if (domainIndex > 0)
            {
                return userName.Remove(domainIndex);
            }
            return userName;
        }

        public static string AnonymizePhonenumber(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            s = s.Trim();

            var username = s.LeftOf("@");

            if (username.IsNumeric())
            {
                // Telefonnummer
                if (username.Length <= 6)
                {
                    // Intern anknytning
                    return username;
                }

                return "Externt nummer";
            }
            
            return s;
        }


        public static string AnonymizeDisplayName(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            s = s.Trim();

            var username = s.LeftOf("@");

            if (username.IsNumeric())
            {
                // Telefonnummer
                if (username.Length <= 6)
                {
                    // Intern anknytning
                    return string.Format("Ank {0}", username);
                }

                return "Externt nummer";
            }

            return s;
        }

        
    }
}