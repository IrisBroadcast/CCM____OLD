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
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Repositories;
using LazyCache;
using NLog;

namespace CCM.Data.Repositories
{
    public class SettingsRepository : BaseRepository, ISettingsRepository
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public SettingsRepository(IAppCache cache) : base(cache)
        {
        }

        public List<Setting> GetAll()
        {
            using (var db = GetDbContext())
            {
                //log.Debug("Hash codes. Settings repository: {0}, Context:{1}", this.GetHashCode(), Context.GetHashCode());

                var settings = db.Settings.ToList();

                var list = settings.Select(setting => new Setting
                {
                    Name = setting.Name,
                    Id = setting.Id,
                    Value = setting.Value,
                    Description = setting.Description
                }).ToList();

                //log.Debug("Getting settings from database. {0}", string.Join(" ", list.Select(s => string.Format("{0}:{1}", s.Name,s.Value))));

                return list;
            }
        }

        public void Save(List<Setting> settings, string userName)
        {
            using (var db = GetDbContext())
            {
                DbSet<Entities.SettingEntity> existing = db.Settings;

                foreach (Setting setting in settings)
                {
                    Entities.SettingEntity dbSetting = existing.SingleOrDefault(s => s.Id == setting.Id);

                    if (dbSetting != null && dbSetting.Value != setting.Value)
                    {
                        dbSetting.Value = setting.Value;
                        dbSetting.UpdatedOn = DateTime.UtcNow;
                        dbSetting.UpdatedBy = userName;
                    }
                }

                db.SaveChanges();
            }
        }

    }
}