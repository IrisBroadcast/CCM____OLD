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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CCM.Core.Entities;
using CCM.Core.Entities.Specific;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Repositories.Specialized;
using LazyCache;

namespace CCM.Data.Repositories.Specialized
{
    public class RegisteredSipDetailsRepository : BaseRepository, IRegisteredSipDetailsRepository
    {
        public RegisteredSipDetailsRepository(IAppCache cache) : base(cache)
        {
        }

        public RegisteredSipDetails GetRegisteredSipById(Guid id)
        {
            using (var db = GetDbContext())
            {
                Entities.RegisteredSipEntity dbSip = db.RegisteredSips
                    .Include(rs => rs.Location)
                    .Include(rs => rs.Location.City)
                    .Include(rs => rs.Location.Region)
                    .Include(rs => rs.UserAgent)
                    .Include(rs => rs.User)
                    .SingleOrDefault(r => r.Id == id);

                return MapToRegisteredSipDetails(dbSip);
            }
        }

        private RegisteredSipDetails MapToRegisteredSipDetails(Entities.RegisteredSipEntity rs)
        {
            if (rs == null)
            {
                return null;
            }

            var model = new RegisteredSipDetails
            {
                Id = rs.Id,
                Sip = rs.SIP,
                DisplayName = rs.DisplayName,
                Ip = rs.IP,
                UserAgentHeader = rs.UserAgentHead,

                Comment = rs.User != null ? rs.User.Comment : string.Empty,
                UserDisplayName = rs.User != null ? rs.User.DisplayName : string.Empty,
                Api = rs.UserAgent != null ? rs.UserAgent.Api ?? string.Empty : string.Empty,
                Image = rs.UserAgent != null && rs.UserAgent.Image != null ? rs.UserAgent.Image : string.Empty,
                ActiveX = rs.UserAgent != null && rs.UserAgent.Ax,
                Width = rs.UserAgent != null ? rs.UserAgent.Width: 1000,
                Height = rs.UserAgent != null ? rs.UserAgent.Height: 1000,
                UserInterfaceLink = rs.UserAgent != null ? rs.UserAgent.UserInterfaceLink ?? "" : String.Empty,
                UserInterfaceIsOpen = rs.UserAgent != null && rs.UserAgent.UserInterfaceIsOpen,
                UseScrollbars = rs.UserAgent != null && rs.UserAgent.UseScrollbars,
                Inputs = rs.UserAgent != null ? rs.UserAgent.Inputs : 0,
                InputGainStep = rs.UserAgent != null ? rs.UserAgent.InputGainStep : 0,
                InputMaxDb = rs.UserAgent != null ? rs.UserAgent.MaxInputDb : 0,
                InputMinDb = rs.UserAgent != null ? rs.UserAgent.MinInputDb : 0,
                Lines = rs.UserAgent != null ? rs.UserAgent.Lines : 0,
                
                CodecPresets = rs.UserAgent != null ? rs.UserAgent.CodecPresets.Select(MapToCodecPreset).ToList() :  new List<CodecPreset>(),

                LocationName = rs.Location != null ? rs.Location.Name : string.Empty,
                LocationComment = rs.Location != null ? rs.Location.Comment : string.Empty,
                CityName = rs.Location != null && rs.Location.City != null ? rs.Location.City.Name : string.Empty,
                RegionName = rs.Location != null && rs.Location.Region != null ? rs.Location.Region.Name : string.Empty,
            };

            return model;
        }

        private CodecPreset MapToCodecPreset(Entities.CodecPresetEntity codecPreset)
        {
            return new CodecPreset()
            {
                Id = codecPreset.Id,
                Name = codecPreset.Name
            };
        }

    }
}
