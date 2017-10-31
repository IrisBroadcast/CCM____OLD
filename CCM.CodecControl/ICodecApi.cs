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

﻿using CCM.Core.CodecControl.Entities;
using CCM.Core.CodecControl.Enums;

namespace CCM.CodecControl
{
    /// <remarks>
    /// Gpi = General Purpose Input
    /// Gpo = General Purpose Output
    /// VuValues = ljudnivåvärden
    /// </remarks>
    public interface ICodecApi
    {
        bool Call(string hostAddress, Call call);
        bool HangUp(string hostAddress, Codec codec);
        bool CheckIfAvailable(string ip);
        
        bool? GetGpo(string ipp, int gpio);
        bool GetInputEnabled(string ip, int input);
        int GetInputGainLevel(string ip, int input);
        LineStatus GetLineStatus(string ip, int line);
        string GetLoadedPresetName(string ip, string lastPresetName);
        VuValues GetVuValues(string ip);
        AudioMode GetAudioMode(string ip);
        AudioStatus GetAudioStatus(string hostAddress, int nrOfInputs, int nrOfGpos);

        bool SetGpo(string ip, int gpo, bool active);
        bool SetInputEnabled(string ip, int input, bool enabled);
        bool SetInputGainLevel(string ip, int input, int gainLevel);
        
        bool LoadPreset(string ip, string presetName);
        bool Reboot(string ip);
    }
}