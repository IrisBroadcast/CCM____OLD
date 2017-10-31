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
using CCM.Core.CodecControl.Enums;

namespace CCM.CodecControl.Mandozzi.Umac
{
    public class UmacCallState
    {
        public LineStatusCode State { get; set; }
        public string TxProtocol { get; set; }
        public string RxProtocol { get; set; }

        public UmacCallState(string s)
        {
            Match m = Regex.Match(s, @"CALL_STATE (?<state>[^[]+) \[tx: (?<tx>.*), rx: (?<rx>.*)\]");
            if (m.Success == false || m.Groups.Count != 4)
            {
                throw new Exception("Unexpected response to \"show call\".");
            }

            string state = m.Groups["state"].Value;
            switch (state)
            {
                case "unknown":
                    State = LineStatusCode.NotAvailable;
                    break;
                case "disconnected":
                    State = LineStatusCode.Disconnected;
                    break;
                case "temp_disabled":
                    State = LineStatusCode.NotAvailable;
                    break;
                case "loading voip-dsp":
                    State = LineStatusCode.Initializing;
                    break;
                case "loading bf-dsp":
                    State = LineStatusCode.Initializing;
                    break;
                case "calling":
                    State = LineStatusCode.Calling;
                    break;
                case "early":
                    State = LineStatusCode.Calling;
                    break;
                case "incoming":
                    State = LineStatusCode.ReceivingCall;
                    break;
                case "connecting":
                    State = LineStatusCode.Calling;
                    break;
                case "connected":
                    State = LineStatusCode.ConnectedCalled;
                    break;
                case "xcasting":
                    State = LineStatusCode.ConnectedCalled;
                    break;
                default:
                    State = LineStatusCode.ErrorGettingStatus;
                    break;
            }
            TxProtocol = m.Groups["tx"].Value;
            RxProtocol = m.Groups["rx"].Value;
        }
    }
}