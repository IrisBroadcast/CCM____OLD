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
using System.Linq;
using CCM.Core.Entities;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Repositories;
using CCM.Data.Entities;
using LazyCache;

namespace CCM.Data.Repositories
{
    public class CodecPresetRepository : BaseRepository, ICodecPresetRepository
    {
        public CodecPresetRepository(IAppCache cache) : base(cache)
        {
        }

        public List<CodecPreset> GetAll()
        {
            using (var db = GetDbContext())
            {
                var dbCodecPresets = db.CodecPresets.ToList();
                return dbCodecPresets.Select(MapToCodecPreset).ToList();
            }
        }

        public void Save(CodecPreset codecPreset)
        {
            using (var db = GetDbContext())
            {
                CodecPresetEntity dbCodecPreset = null;
                
                if (codecPreset.Id != Guid.Empty)
                {
                    dbCodecPreset = db.CodecPresets.SingleOrDefault(c => c.Id == codecPreset.Id);
                }

                if (dbCodecPreset == null)
                {
                    dbCodecPreset = new CodecPresetEntity
                    {
                        Id = Guid.NewGuid(),
                        CreatedBy = codecPreset.CreatedBy,
                        CreatedOn = DateTime.UtcNow
                    };
                    codecPreset.Id = dbCodecPreset.Id;
                    codecPreset.CreatedOn = dbCodecPreset.CreatedOn;
                    db.CodecPresets.Add(dbCodecPreset);
                }

                dbCodecPreset.Name = codecPreset.Name;
                dbCodecPreset.UpdatedBy = codecPreset.UpdatedBy;
                dbCodecPreset.UpdatedOn = DateTime.UtcNow;
                codecPreset.UpdatedOn = dbCodecPreset.UpdatedOn;

                db.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            using (var db = GetDbContext())
            {
                var dbCodecPreset = db.CodecPresets.SingleOrDefault(c => c.Id == id);

                if (dbCodecPreset == null)
                {
                    return;
                }

                db.CodecPresets.Remove(dbCodecPreset);
                db.SaveChanges();
            }
        }

        public CodecPreset GetById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            using (var db = GetDbContext())
            {
                var dbCodecPreset = db.CodecPresets.SingleOrDefault(c => c.Id == id);
                return MapToCodecPreset(dbCodecPreset);
            }
        }

        public List<CodecPreset> Find(string search)
        {
            using (var db = GetDbContext())
            {
                var dbCodecPresets = db.CodecPresets
                    .Where(c => c.Name.ToLower().Contains(search.ToLower()))
                    .ToList();
                return dbCodecPresets.Select(MapToCodecPreset).OrderBy(c => c.Name).ToList();
            }
        }

        private CodecPreset MapToCodecPreset(CodecPresetEntity dbPreset)
        {
            return dbPreset == null ? null
                : new CodecPreset
                {
                    Id = dbPreset.Id,
                    Name = dbPreset.Name,
                    CreatedBy = dbPreset.CreatedBy,
                    CreatedOn = dbPreset.CreatedOn,
                    UpdatedBy = dbPreset.UpdatedBy,
                    UpdatedOn = dbPreset.UpdatedOn
                };
        }
    }
}