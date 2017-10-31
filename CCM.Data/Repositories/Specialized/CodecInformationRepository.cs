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
using System.Data.Entity;
using System.Linq;
using CCM.Core.CodecControl.Entities;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Repositories.Specialized;
using CCM.Data.Entities;
using LazyCache;

namespace CCM.Data.Repositories.Specialized
{
    public class CodecInformationRepository : BaseRepository, ICodecInformationRepository
    {
        public CodecInformationRepository(IAppCache cache) : base(cache)
        {
        }

        public CodecInformation GetCodecInformationBySipAddress(string sipAddress)
        {
            using (var db = GetDbContext())
            {
                RegisteredSipEntity rs = db.RegisteredSips
                    .Include(o => o.UserAgent)
                    .SingleOrDefault(o => o.SIP == sipAddress);

                return rs == null ? null : MapToCodecInformation(rs);
            }
        }

        public CodecInformation GetCodecInformationById(Guid id)
        {
            using (var db = GetDbContext())
            {
                RegisteredSipEntity rs = db.RegisteredSips
                    .Include(o => o.UserAgent)
                    .SingleOrDefault(o => o.Id == id);

                return rs == null ? null : MapToCodecInformation(rs);
            }
        }

        private CodecInformation MapToCodecInformation(RegisteredSipEntity rs)
        {
            return new CodecInformation
            {
                SipAddress = rs.SIP,
                Api = rs.UserAgent.Api,
                Ip = rs.IP,
                GpoNames = rs.UserAgent.GpoNames,
                NrOfInputs = rs.UserAgent.Inputs
            };
        }
    }
}
