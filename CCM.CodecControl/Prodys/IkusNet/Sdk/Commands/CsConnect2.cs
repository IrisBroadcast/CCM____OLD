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
using System.Security.Cryptography;
using System.Text;
using CCM.CodecControl.Helpers;
using CCM.CodecControl.Prodys.IkusNet.Sdk.Enums;

namespace CCM.CodecControl.Prodys.IkusNet.Sdk.Commands
{
    public class CsConnect2 : ConnectCommandBase
    {
        public override Command Command { get { return Command.CsConnect2; } }
        private string SecurityString = IkusNet.ExternalProtocolIpSecurityString;
        private string UserName = IkusNet.ExternalProtocolUserName;

        private byte[] PasswordHash
        {
            get
            {
                using (SHA512 shaM = new SHA512Managed())
                {
                    var data = Encoding.UTF8.GetBytes(IkusNet.ExternalProtocolPassword);
                    return shaM.ComputeHash(data);
                }
            }
        } 

        public override byte[] GetBytes()
        {
            var bytes = new byte[348];

            ConvertHelper.EncodeUInt((uint)Command, bytes, 0);
            ConvertHelper.EncodeUInt(340, bytes, 4);
            ConvertHelper.EncodeString(SecurityString, bytes, 8, 20);
            ConvertHelper.EncodeString(UserName, bytes, 28, 256);
            var passwordHash = PasswordHash;
            passwordHash.CopyTo(bytes,284);

            return bytes;
        }
    }
}