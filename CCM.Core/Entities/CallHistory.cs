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

﻿namespace CCM.Core.Entities
{
    using System;

    public class CallHistory
    {
        public Guid CallHistoryId { get; set; }
        public Guid CallId { get; set; }
        public string SipCallId { get; set; }
        public DateTime Started { get; set; }
        public DateTime Ended { get; set; }
        public string DlgHashId { get; set; }
        public string DlgHashEnt { get; set; }
        public string ToTag { get; set; }
        public string FromTag { get; set; }
        public Guid FromId { get; set; }
        public string FromSip { get; set; }
        public string FromUsername { get; set; }
        public string FromDisplayName { get; set; }
        public string FromComment { get; set; }
        public Guid FromLocationId { get; set; }
        public string FromLocationName { get; set; }
        public string FromLocationComment { get; set; }
        public string FromLocationShortName { get; set; }
        public Guid FromCodecTypeId { get; set; }
        public string FromCodecTypeName { get; set; }
        public string FromCodecTypeColor { get; set; }
        public Guid FromOwnerId { get; set; }
        public string FromOwnerName { get; set; }
        public Guid FromRegionId { get; set; }
        public string FromRegionName { get; set; }
        public string FromUserAgentHead { get; set; }
        public Guid ToId { get; set; }
        public string ToSip { get; set; }
        public string ToUsername { get; set; }
        public string ToDisplayName { get; set; }
        public string ToComment { get; set; }
        public Guid ToLocationId { get; set; }
        public string ToLocationName { get; set; }
        public string ToLocationComment { get; set; }
        public string ToLocationShortName { get; set; }
        public Guid ToCodecTypeId { get; set; }
        public string ToCodecTypeName { get; set; }
        public string ToCodecTypeColor { get; set; }
        public Guid ToOwnerId { get; set; }
        public string ToOwnerName { get; set; }
        public Guid ToRegionId { get; set; }
        public string ToRegionName { get; set; }
        public string ToUserAgentHead { get; set; }
        public bool IsPhoneCall { get; set; }
    }
}