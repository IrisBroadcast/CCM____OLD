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
using System.Diagnostics;

namespace CCM.CodecControl.Helpers
{
    /// <summary>
    /// Helper class for encoding and decoding values
    /// </summary>
    public static class ConvertHelper
    {
        public static int EncodeUInt(uint value, byte[] buffer, int offset)
        {
            buffer[offset + 0] = (byte)(value >> 0);
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
            return offset + 4;
        }

        public static int EncodeString(string s, byte[] buffer, int offset, int maxLength)
        {
            // Modify maxLength to take ending null byte in concideration
            Array.Clear(buffer, offset, maxLength);

            var stringBytes = System.Text.Encoding.ASCII.GetBytes(s);
            var length = Math.Min(stringBytes.Length, maxLength-1); // -1 because of null termination

            Array.Copy(stringBytes, 0, buffer, offset, length);
            return offset + maxLength;
        }

        public static uint DecodeUInt(byte[] buffer, int offset)
        {
            return ((uint)buffer[offset + 0] << 0)
                   + ((uint)buffer[offset + 1] << 8)
                   + ((uint)buffer[offset + 2] << 16)
                   + ((uint)buffer[offset + 3] << 24);
        }
    }
}