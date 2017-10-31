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
using System.Net.Sockets;
using CCM.CodecControl.Helpers;
using CCM.CodecControl.Prodys.IkusNet.Sdk.Enums;

namespace CCM.CodecControl.Prodys.IkusNet.Sdk.Responses
{
    public abstract class IkusNetStatusResponseBase
    {
        protected static byte[] GetResponseBytes(Socket socket, Command expectedCommand, int expectedResponseLength)
        {
            // Read fixed part of header
            var buffer = new byte[8];
            socket.Receive(buffer);
            var command = (Command) ConvertHelper.DecodeUInt(buffer, 0);
            var length = (int) ConvertHelper.DecodeUInt(buffer, 4);
            
            if (command != expectedCommand || length != expectedResponseLength)
            {
                throw new Exception(string.Format(
                    "Invalid response from codec. Command was {0} and length {1}. Expected {2} with length {3}",
                    command, length, expectedCommand, expectedResponseLength));
            }

            // Read variable part of header
            var result = new byte[length];
            socket.Receive(result);
            return result;
        }
    }
}