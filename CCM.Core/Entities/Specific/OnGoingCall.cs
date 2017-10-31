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

namespace CCM.Core.Entities.Specific
{
    public class OnGoingCall
    {
        public string CallId { get; set; }
        public DateTime Started { get; set; }
        public bool IsPhoneCall { get; set; }
        public string FromDisplayName { get; set; }
        public string FromSip { get; set; }
        public string FromId { get; set; }
        public string FromLocationName { get; set; }
        public string FromLocationShortName { get; set; }
        public string FromComment { get; set; }
        public string FromRegionName { get; set; }
        public string FromCodecTypeName { get; set; }
        public string FromCodecTypeColor { get; set; }
        public string ToDisplayName { get; set; }
        public string ToSip { get; set; }
        public string ToId { get; set; }
        public string ToLocationName { get; set; }
        public string ToLocationShortName { get; set; }
        public string ToComment { get; set; }
        public string ToRegionName { get; set; }
        public string ToCodecTypeName { get; set; }
        public string ToCodecTypeColor { get; set; }

        public int DurationSeconds => Convert.ToInt32(DateTime.UtcNow.Subtract(Started).TotalSeconds);

    }
}