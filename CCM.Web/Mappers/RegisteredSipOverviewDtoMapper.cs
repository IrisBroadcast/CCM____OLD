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

using CCM.Core.Entities.Specific;
using CCM.Core.Helpers;
using CCM.Web.Models.Home;

namespace CCM.Web.Mappers
{
    public static class RegisteredSipOverviewDtoMapper
    {
        public static RegisteredSipOverviewDto MapToDto(RegisteredSipDto regSip, string sipDomain)
        {
            return new RegisteredSipOverviewDto
            {
                InCall = regSip.InCall,
                Sip = regSip.Sip,
                Id = regSip.Id,
                //DisplayName = regSip.DisplayName,


                DisplayName = DisplayNameHelper.GetDisplayName(regSip.DisplayName, regSip.UserDisplayName, 
                string.Empty, regSip.UserName, regSip.Sip, "", sipDomain),


                Location = regSip.LocationName,
                LocationShortName = regSip.LocationShortName,
                Comment = regSip.Comment,
                Image = regSip.Image,
                CodecTypeName = regSip.CodecTypeName,
                CodecTypeColor = regSip.CodecTypeColor,
                UserName = regSip.UserName,
                UserDisplayName = regSip.UserDisplayName,
                UserComment = regSip.Comment,
                InCallWithId = regSip.InCallWithId,
                InCallWithSip = DisplayNameHelper.AnonymizePhonenumber(regSip.InCallWithSip),
                InCallWithName = DisplayNameHelper.AnonymizeDisplayName(regSip.InCallWithName),
                RegionName = regSip.RegionName,
                Updated = regSip.Updated
            };
        }

    }
}