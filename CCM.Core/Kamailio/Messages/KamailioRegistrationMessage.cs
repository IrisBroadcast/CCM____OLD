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
    public class KamailioRegistrationMessage : KamailioMessageBase
    {
        public SipUri Sip { get; set; }
        public string FromDisplayName { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public SipUri RequestedSip { get; set; }
        public string ReceivedIp { get; set; } // Används inte
        public int ReceivedPort { get; set; } // Används inte
        public string UserAgent { get; set; }
        public string ToUsername { get; set; }
        public string Username { get; set; }
        public string CallId { get; set; }
        public string ToDisplayName { get; set; }
        public long UnixTimeStamp { get; set; }
        public int Expires { get; set; }

        public override string ToDebugString()
        {
            return string.Format(
                    "SIP:{0}, IP:{1}, Port:{2}, RequestedSip:{3}, ReceivedIp:{4}, ReceivedPort:{5}, UserAgent:{6}, ToUsername:{7}" +
                    ", Username:{8}, CallId:{9}, ToDisplayName:{10}, UnixTimeStamp:{11}, Expires:{12}, FromDisplayName:{13}",
                    Sip, Ip, Port, RequestedSip, ReceivedIp, ReceivedPort, UserAgent, ToUsername, Username, CallId, ToDisplayName, UnixTimeStamp, Expires, FromDisplayName);
        }
    }
}