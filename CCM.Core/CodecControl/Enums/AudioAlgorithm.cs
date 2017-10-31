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

﻿using System.ComponentModel;

namespace CCM.Core.CodecControl.Enums
{
    /// <summary>
    /// Ljudkodningsalgoritm, baserat på Prodys implementation
    /// </summary>
    public enum AudioAlgorithm
    {
        [Description("Okänd/saknad kodning")] Error = 0,
        [Description("MPEG 4 AAC-HE")] Mpeg4AacHe = 1,
        [Description("MPEG 4 AAC-LC")] Mpeg4AacLc = 2,
        [Description("MPEG 4 AAC-ELD")] Mpeg4AacEld = 3,
        [Description("E-aptX")] Eaptx = 4,
        [Description("OPUS")] Opus = 5,
        [Description("PCM")] Pcm = 6,
        [Description("G711")] G711 = 7,
        [Description("G722")] G722 = 8,
        [Description("MPEG 4 AAC-LD")] Mpeg4AacLd = 9,
        [Description("MPEG L2")] MpegL2 = 10
    }
}