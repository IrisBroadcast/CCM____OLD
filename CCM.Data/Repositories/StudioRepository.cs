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
using AutoMapper;
using CCM.Core.Entities;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Repositories;
using CCM.Data.Entities;
using LazyCache;

namespace CCM.Data.Repositories
{
    public class StudioRepository : BaseRepository, IStudioRepository
    {
        public StudioRepository(IAppCache cache) : base(cache)
        {
        }

        public void Save(Studio studio)
        {
            using (var db = GetDbContext())
            {
                StudioEntity dbStudio;
                var timeStamp = DateTime.UtcNow;

                if (studio.Id != Guid.Empty)
                {
                    dbStudio = db.Studios.SingleOrDefault(g => g.Id == studio.Id);

                    if (dbStudio == null)
                    {
                        throw new Exception("Studio could not be found");
                    }
                }
                else
                {
                    dbStudio = new StudioEntity()
                    {
                        Id = Guid.NewGuid(),
                        CreatedBy = studio.CreatedBy,
                        CreatedOn = timeStamp,
                    };

                    studio.Id = dbStudio.Id;
                    studio.CreatedOn = dbStudio.CreatedOn;
                    db.Studios.Add(dbStudio);
                }


                dbStudio.Name = studio.Name;
                dbStudio.CodecSipAddress = studio.CodecSipAddress;
                dbStudio.CameraAddress = studio.CameraAddress;
                dbStudio.CameraActive = studio.CameraActive;
                dbStudio.CameraUsername = studio.CameraUsername;
                dbStudio.CameraPassword = studio.CameraPassword;
                dbStudio.CameraVideoUrl = studio.CameraVideoUrl;
                dbStudio.CameraImageUrl = studio.CameraImageUrl;
                dbStudio.CameraPlayAudioUrl = studio.CameraPlayAudioUrl;
                dbStudio.AudioClipNames = studio.AudioClipNames;
                dbStudio.InfoText = studio.InfoText;
                dbStudio.MoreInfoUrl = studio.MoreInfoUrl;
                dbStudio.NrOfAudioInputs = studio.NrOfAudioInputs;
                dbStudio.AudioInputNames = studio.AudioInputNames;
                dbStudio.AudioInputDefaultGain = studio.AudioInputDefaultGain;
                dbStudio.NrOfGpos = studio.NrOfGpos;
                dbStudio.GpoNames = studio.GpoNames;
                dbStudio.InactivityTimeout = studio.InactivityTimeout;
                dbStudio.UpdatedBy = studio.UpdatedBy;
                studio.UpdatedOn = dbStudio.UpdatedOn = timeStamp;

                db.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            using (var db = GetDbContext())
            {
                var dbStudio = db.Studios.SingleOrDefault(g => g.Id == id);
                if (dbStudio != null)
                {
                    db.Studios.Remove(dbStudio);
                    db.SaveChanges();
                }
            }
        }

        public Studio GetById(Guid id)
        {
            using (var db = GetDbContext())
            {
                var dbStudio = db.Studios.SingleOrDefault(g => g.Id == id);
                return MapToStudio(dbStudio);
            }
        }

        public List<Studio> GetAll()
        {
            using (var db = GetDbContext())
            {
                var dbStudios = db.Studios.ToList();
                return dbStudios.Select(MapToStudio).OrderBy(g => g.Name).ToList();
            }
        }

        public List<Studio> FindStudios(string search)
        {
            using (var db = GetDbContext())
            {
                search = (search ?? string.Empty).ToLower();
                var dbStudios = db.Studios.Where(r => r.Name.ToLower().Contains(search)).ToList();
                return dbStudios.Select(MapToStudio).OrderBy(g => g.Name).ToList();
            }
        }
        
        private Studio MapToStudio(StudioEntity dbStudio)
        {
            return Mapper.Map<Studio>(dbStudio);
        }
        
    }
}