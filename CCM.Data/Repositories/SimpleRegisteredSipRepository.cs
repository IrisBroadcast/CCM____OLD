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
using System.Linq.Expressions;
using CCM.Core.Entities;
using CCM.Core.Entities.Specific;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Managers;
using CCM.Core.Interfaces.Repositories;
using CCM.Data.Entities;
using LazyCache;

namespace CCM.Data.Repositories
{
    public class SimpleRegisteredSipRepository : BaseRepository, ISimpleRegisteredSipRepository
    {
        private readonly ISettingsManager _settingsManager;

     public SimpleRegisteredSipRepository(ISettingsManager settingsManager, IAppCache cache) : base(cache)
        {
            _settingsManager = settingsManager;
        }

        public RegisteredSipInfo GetRegisteredSipInfoById(Guid id)
        {
            using (var db = GetDbContext())
            {
                Entities.RegisteredSipEntity dbSip = db.RegisteredSips
                    .Include(rs => rs.User)
                    .SingleOrDefault(r => r.Id == id);

                return dbSip == null
                    ? null
                    : new RegisteredSipInfo
                    {
                        SipAddress = dbSip.SIP,
                        DisplayName = DisplayNameHelper.GetDisplayName(dbSip.DisplayName,
                            dbSip.User != null ? dbSip.User.DisplayName : string.Empty, string.Empty, dbSip.Username,
                            dbSip.SIP, "", _settingsManager.SipDomain)
                    };
            }
        }

        public RegisteredSip GetRegisteredSipById(Guid id)
        {
            using (var db = GetDbContext())
            {
                var dbSip = db.RegisteredSips.SingleOrDefault(r => r.Id == id);
                return MapToRegisteredSip(dbSip);
            }
        }

        public List<Guid> GetRegisteredSipIdsForUser(Guid userId)
        {
            using (var db = GetDbContext())
            {
                return db.RegisteredSips
                    .Where(r => r.User.Id == userId)
                    .Select(rs => rs.Id)
                    .ToList();
            }
        }

        private RegisteredSip MapToRegisteredSip(RegisteredSipEntity dbRegisteredSip)
        {
            if (dbRegisteredSip == null)
            {
                return null;
            }

            var registredSip = new RegisteredSip
            {
                DisplayName = dbRegisteredSip.DisplayName,
                Expires = dbRegisteredSip.Expires,
                Id = dbRegisteredSip.Id,
                IP = dbRegisteredSip.IP,
                Port = dbRegisteredSip.Port,
                SIP = dbRegisteredSip.SIP,
                ServerTimeStamp = dbRegisteredSip.ServerTimeStamp,
                Updated = dbRegisteredSip.Updated,
                UserAgentHead = dbRegisteredSip.UserAgentHead,
                Username = dbRegisteredSip.Username,
            };

            return registredSip;
        }
        
    }
}
