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
using System.Net.Sockets;
using CCM.CodecControl.Helpers;
using CCM.CodecControl.Prodys.IkusNet.Sdk.Enums;

namespace CCM.CodecControl.Prodys.IkusNet.Sdk.Responses
{
    public class IkusNetGetGpiResponse : IkusNetGetGpioResponse
    {
        public IkusNetGetGpiResponse(Socket socket) :base(socket, Command.IkusNetGetGpi)
        {
        }
    }

    public  class IkusNetGetGpoResponse : IkusNetGetGpioResponse
    {
        public IkusNetGetGpoResponse(Socket socket) : base(socket, Command.IkusNetGetGpo)
        {
        }
    }

    public abstract class IkusNetGetGpioResponse : IkusNetStatusResponseBase
    {
        public bool? Active { get; protected set; }

        protected IkusNetGetGpioResponse(Socket socket, Command command)
        {
            ParseResponse(socket, command);
        }

        protected void ParseResponse(Socket socket, Command expectedCommand)
        {
            var buffer = new byte[8];
            socket.Receive(buffer);
            var command = (Command) ConvertHelper.DecodeUInt(buffer, 0);
            var length = (int) ConvertHelper.DecodeUInt(buffer, 4);

            if (command != expectedCommand || length != 4)
            {
                // This is usually a sign that no GPIO exists for the requested gpio number
                Active = null;
                return;
            }

            var payloadBytes = new byte[length];
            socket.Receive(payloadBytes);

            Active = Convert.ToBoolean(ConvertHelper.DecodeUInt(payloadBytes, 0));
        }
        
    }
}