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

namespace CCM.Core.CodecControl.Interfaces
{
    public interface ICodecManager
    {
        bool Call(CodecInformation codecInformation, string callee, string profileName);
        bool HangUp(CodecInformation codecInformation);
        bool CheckIfAvailable(CodecInformation codecInformation);
        bool? GetGpo(CodecInformation codecInformation, int gpio);
        bool SetGpo(CodecInformation codecInformation, int gpo, bool active);

        // GetInputEnabled, SetInputEnabled, GetInputGainLevel och SetInputGainLevel fungerar inte på Quantum ST då den saknar styrbara ingångar.
        bool GetInputEnabled(CodecInformation codecInformation, int input);
        bool SetInputEnabled(CodecInformation codecInformation, int input, bool enabled);
        int GetInputGainLevel(CodecInformation codecInformation, int input);
        int SetInputGainLevel(CodecInformation codecInformation, int input, int gainLevel);
        LineStatus GetLineStatus(CodecInformation codecInformation, int line);
        string GetLoadedPresetName(CodecInformation codecInformation, string lastPresetName);
        VuValues GetVuValues(CodecInformation codecInformation);
        AudioStatus GetAudioStatus(CodecInformation codecInformation, int nrOfInputs, int nrOfGpos);
        AudioMode GetAudioMode(CodecInformation codecInformation);
        bool LoadPreset(CodecInformation codecInformation, string preset);
        bool Reboot(CodecInformation codecInformation);
    }
}