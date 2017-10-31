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
using System.Collections.Generic;
using CCM.Core.Attributes;

namespace CCM.Core.Entities.Specific
{
    public class RegisteredSipDto
    {
        public Guid Id { get; set; }
        public DateTime Updated { get; set; }
        public string Sip { get; set; }
        public bool InCall { get; set; }
        public string DisplayName { get; set; }
        public string IpAddress { get; set; }
        public string UserAgentHeader { get; set; } // User agent-sträng kodaren skickar
        public IList<string> Profiles { get; set; }

        public List<KeyValuePair<string, string>> MetaData { get; set; }

        public string UserDisplayName { get; set; }
        public string UserName { get; set; }
        public string CodecTypeColor { get; set; }
        public string Api { get; set; }

        // Aktiva filteregenskaper
        [FilterProperty(TableName = "Regions", ColumnName = "Name")]
        public string RegionName { get; set; }

        [FilterProperty(TableName = "CodecTypes", ColumnName = "Name")]
        public string CodecTypeName { get; set; }

        [FilterProperty(TableName = "Locations", ColumnName = "Name")]
        public string LocationName { get; set; }

        // Ej aktiva Filteregenskaper. Rensa?
        [FilterProperty(TableName = "Cities", ColumnName = "Name")]
        public string CityName { get; set; }

        [FilterProperty(TableName = "Locations", ColumnName = "ShortName")]
        public string LocationShortName { get; set; }

        [FilterProperty(TableName = "UserAgents", ColumnName = "Name")]
        public string UserAgentName { get; set; }

        public string Comment { get; set; }
        public string Image { get; set; }
        public bool HasGpo { get; set; }
        public string InCallWithId { get; set; }
        public string InCallWithSip { get; set; }
        public string InCallWithName { get; set; }
        public string InCallWithLocation { get; set; }
        public bool InCallWithHasGpo { get; set; }
        public bool IsCallingPart { get; set; } // True om i samtal och denna enhet ringde upp
        public bool IsPhoneCall { get; set; } // True om i samtal och det är ett telefonsaamtal
        public DateTime CallStartedAt { get; set; }

    }
}