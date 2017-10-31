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

namespace CCM.Core.Kamailio.Messages
{
    public class KamailioDialogMessage : KamailioMessageBase
    {
        public DialogStatus Status { get; set; }
        public string CallId { get; set; }
        public string HashId { get; set; }
        public string HashEntry { get; set; }
        public SipUri FromSipUri { get; set; }
        public SipUri ToSipUri { get; set; }
        public string FromDisplayName { get; set; }
        public string ToDisplayName { get; set; }
        public string FromTag { get; set; }
        public string ToTag { get; set; }
        public string Sdp { get; set; }
        public string HangupReason { get; set; }

        public override string ToDebugString()
        {
            if (Status == DialogStatus.SingleBye)
            {
                return string.Format("CallId:{0}, FromSip:{1}, ToSip:{2}, FromTag:{3}, ToTag:{4}",
                    CallId, 
                    FromSipUri != null ? FromSipUri.UserAtHost : string.Empty,
                    ToSipUri != null ? ToSipUri.UserAtHost : string.Empty,
                    FromTag, 
                    ToTag);
            }

            if (Status == DialogStatus.End)
            {
                return string.Format("CallId:{0}, HashId:{1}, HashEntry:{2}, Hangup reason:{3}, FromDisplayName:{4} FromSip:{5}, ToDisplayName:{6} ToSip:{7}, FromTag:{8}, ToTag:{9}",
                    CallId, HashId, HashEntry, HangupReason,
                    FromDisplayName, FromSipUri != null ? FromSipUri.UserAtHost : string.Empty,
                    ToDisplayName, ToSipUri != null ? ToSipUri.UserAtHost : string.Empty,
                    FromTag, ToTag);

            }

            return string.Format("CallId:{0}, HashId:{1}, HashEntry:{2}, FromDisplayName:{3} FromSip:{4}, ToDisplayName:{5} ToSip:{6}, FromTag:{7}, ToTag:{8}", 
                CallId, HashId, HashEntry,
                FromDisplayName, FromSipUri != null ? FromSipUri.UserAtHost : string.Empty, 
                ToDisplayName, ToSipUri != null ? ToSipUri.UserAtHost : string.Empty, 
                FromTag, ToTag);
        }
    }
}