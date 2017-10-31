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

﻿using System.Net.Sockets;
using CCM.CodecControl.Helpers;
using CCM.CodecControl.Prodys.IkusNet.Sdk.Enums;

namespace CCM.CodecControl.Prodys.IkusNet.Sdk.Responses
{
    public class IkusNetGetVumetersResponse: IkusNetStatusResponseBase
    {

        public int ProgramTxLeft { get; private set; }
        public int ProgramTxRight { get; private set; }
        public int ProgramRxLeft { get; private set; }
        public int ProgramRxRight { get; private set; }
        public int TalkbackTxLeft { get; private set; }
        public int TalkbackTxRight { get; private set; }
        public int TalkbackRxLeft { get; private set; }
        public int TalkbackRxRight { get; private set; }

        public IkusNetGetVumetersResponse(Socket socket)
        {
            var responseBytes = GetResponseBytes(socket, Command.IkusNetGetVumeters, 32);
            ProgramTxLeft = (int) ConvertHelper.DecodeUInt(responseBytes, 0);
            ProgramTxRight = (int) ConvertHelper.DecodeUInt(responseBytes, 4);
            ProgramRxLeft = (int) ConvertHelper.DecodeUInt(responseBytes, 8);
            ProgramRxRight = (int) ConvertHelper.DecodeUInt(responseBytes, 12);
        }

    }

  
}