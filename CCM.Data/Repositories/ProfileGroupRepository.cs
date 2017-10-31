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
using System.Data;
using System.Linq;
using System.Data.Entity;
using AutoMapper;
using CCM.Core.Entities;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Repositories;
using CCM.Data.Entities;
using LazyCache;

namespace CCM.Data.Repositories
{
    public class ProfileGroupRepository : BaseRepository, IProfileGroupRepository
    {
        public ProfileGroupRepository(IAppCache cache) : base(cache)
        {
        }

        public List<ProfileGroup> GetAll()
        {
            using (var db = GetDbContext())
            {
                return db.ProfileGroups
                    .Include(g => g.OrderedProfiles.Select(op => op.Profile))
                    .ToList()
                    .Select(g => Mapper.Map<ProfileGroup>(g)).ToList();
            }
        }

        public ProfileGroup GetById(Guid id)
        {
            using (var db = GetDbContext())
            {
                var group = db.ProfileGroups
                    .Include(g => g.OrderedProfiles.Select(op => op.Profile))
                    .SingleOrDefault(g => g.Id == id);
                var profileGroup = Mapper.Map<ProfileGroup>(group);

                return profileGroup;
            }
        }

        public void Save(ProfileGroup profileGroup)
        {
            using (var db = GetDbContext())
            {
                bool nameCollision = db.ProfileGroups.Any(p => p.Name == profileGroup.Name && p.Id != profileGroup.Id);
                if (nameCollision)
                {
                    throw new DuplicateNameException();
                }

                ProfileGroupEntity dbProfileGroup;

                if (profileGroup.Id != Guid.Empty)
                {
                    // Uppdatera
                    dbProfileGroup = db.ProfileGroups.SingleOrDefault(g => g.Id == profileGroup.Id);
                    if (dbProfileGroup == null)
                    {
                        throw new Exception("Group could not be found");
                    }

                    Mapper.Map(profileGroup, dbProfileGroup);

                    dbProfileGroup.OrderedProfiles.Where(op => !profileGroup.Profiles.Any(sp => sp.Id == op.ProfileId))
                        .ToList()
                        .ForEach(pg =>
                            {
                                dbProfileGroup.OrderedProfiles.Remove(pg);
                            }
                        );

                    var newProfiles = new List<ProfileGroupProfileOrdersEntity>();

                    profileGroup.Profiles.Where(sp => !dbProfileGroup.OrderedProfiles.Any(op => op.ProfileId == sp.Id))
                        .ToList()
                        .ForEach(sp =>
                        {
                            var pgpo = new ProfileGroupProfileOrdersEntity()
                            {
                                ProfileGroupId = dbProfileGroup.Id,
                                ProfileId = sp.Id,
                            };
                            newProfiles.Add(pgpo);
                            dbProfileGroup.OrderedProfiles.Add(pgpo);
                        });

                    int i = 0;
                    foreach (var p in profileGroup.Profiles.OrderBy(sp => sp.SortIndex))
                    {
                        dbProfileGroup.OrderedProfiles.Where(op => p.Id == op.ProfileId).SingleOrDefault().SortIndex =
                            i++;
                    }
                }
                else
                {
                    // Ny
                    profileGroup.Id = Guid.NewGuid();
                    dbProfileGroup = Mapper.Map<ProfileGroupEntity>(profileGroup);
                    dbProfileGroup.OrderedProfiles = new List<ProfileGroupProfileOrdersEntity>();

                    profileGroup.Profiles.ForEach(profile =>
                        dbProfileGroup.OrderedProfiles.Add(new ProfileGroupProfileOrdersEntity()
                            {
                                ProfileGroupId = dbProfileGroup.Id,
                                ProfileId = profile.Id,
                            }
                        ));

                    int i = 0;
                    foreach (var p in profileGroup.Profiles.OrderBy(sp => sp.SortIndex))
                    {
                        dbProfileGroup.OrderedProfiles.Where(op => p.Id == op.ProfileId).SingleOrDefault().SortIndex =
                            i++;
                    }

                    dbProfileGroup.CreatedBy = profileGroup.CreatedBy;
                    dbProfileGroup.CreatedOn = profileGroup.CreatedOn;

                    db.ProfileGroups.Add(dbProfileGroup);
                }

                db.SaveChanges();
            }
        }


        public void Delete(Guid id)
        {
            using (var db = GetDbContext())
            {
                var group = db.ProfileGroups.SingleOrDefault(g => g.Id == id);
                if (group != null)
                {
                    db.ProfileGroups.Remove(group);
                    db.SaveChanges();
                }
            }
        }

    }
}