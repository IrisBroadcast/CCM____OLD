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

﻿using CCM.CodecControl.Helpers;
using CCM.CodecControl.Prodys.IkusNet.Sdk.Commands.Base;
using CCM.CodecControl.Prodys.IkusNet.Sdk.Enums;

namespace CCM.CodecControl.Prodys.IkusNet.Sdk.Commands
{
    public class CommandIkusNetCall : CommandBase
    {
        public CommandIkusNetCall() : base(Command.IkusNetCall, 524) {}

        public IkusNetCodec Codec { get; set; }
        public IkusNetCallContent CallContent { get; set; }
        public IkusNetIPCallType CallType { get; set; }
        public string Profile { get; set; }
        public string Address { get; set; }

        protected override int EncodePayload(byte[] bytes, int offset)
        {
            offset = ConvertHelper.EncodeUInt((uint)Codec, bytes, offset);
            offset = ConvertHelper.EncodeUInt((uint)CallContent, bytes, offset);
            offset = ConvertHelper.EncodeUInt((uint)CallType, bytes, offset);
            offset = ConvertHelper.EncodeString(Profile, bytes, offset, 256);
            offset = ConvertHelper.EncodeString(Address, bytes, offset, 256);
            return offset;
        }
    }
    
}