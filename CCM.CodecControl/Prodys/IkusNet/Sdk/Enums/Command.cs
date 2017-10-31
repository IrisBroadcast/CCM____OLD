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

﻿namespace CCM.CodecControl.Prodys.IkusNet.Sdk.Enums
{
    public enum Command
    {
        CsAlive = 0,
        CsConnect = 1,
        CsCommandResponse = 3,
        CsConnect2 = 4,
        // 5- 99 Ej definierade nr
        IkusNetSysGetDeviceName = 100,
        IkusNetSysSetDeviceName = 101,
        IkusNetSysRebootDevice = 102,
        IkusNetGetLoadedPresetName = 103,
        IkusNetPresetLoad = 104,
        IkusNetGetVumeters = 105,
        IkusNetSetInputGainLevel = 106,
        IkusNetGetInputGainLevel = 107,
        IkusNetSetInputEnabled = 108,
        IkusNetGetInputEnabled = 109,
        IkusNetCall = 110,
        IkusNetHangUp = 111,
        IkusNetGetGpi = 112,
        IkusNetGetGpo = 113,
        IkusNetSetGpo = 114,
        IkusNetGetLineStatus = 115,
        IkusNetCallV2 = 116,
        IkusNetGetCurrentProfile = 117,
        IkusNetEncoderGetAudioMode = 118,
        // 119-138 Definierade men ej implementerade, se IkusNet SDK User Manual
        IkusNetDecoderGetAudioMode = 139
        // 140-    Definierade men ej implementerade

    }
}