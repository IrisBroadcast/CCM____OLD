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
using System.Linq.Expressions;
using System.Data.Entity;
using CCM.Core.Entities;
using CCM.Core.Entities.Specific;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Repositories;
using CCM.Data.Entities;
using LazyCache;

namespace CCM.Data.Repositories
{
    public class ProfileRepository : BaseRepository, IProfileRepository
    {
        public ProfileRepository(IAppCache cache) : base(cache)
        {
        }

        public void Save(Profile profile)
        {
            using (var db = GetDbContext())
            {
                // Check if name already taken
                bool profileNameCollision = db.Profiles.Any(p => p.Name.ToLower() == profile.Name.ToLower() && p.Id != profile.Id);
                if (profileNameCollision)
                {
                    throw new DuplicateNameException();
                }

                ProfileEntity dbProfile;

                if (profile.Id != Guid.Empty)
                {
                    dbProfile = db.Profiles.SingleOrDefault(p => p.Id == profile.Id);

                    if (dbProfile == null)
                    {
                        throw new NullReferenceException("Profile");
                    }
                }
                else
                {
                    dbProfile = new ProfileEntity()
                    {
                        Id = Guid.NewGuid(),
                        CreatedBy = profile.CreatedBy,
                        CreatedOn = DateTime.UtcNow
                    };
                    profile.Id = dbProfile.Id;
                    profile.CreatedOn = dbProfile.CreatedOn;
                    db.Profiles.Add(dbProfile);
                    dbProfile.SortIndex = db.Profiles.Any() ? db.Profiles.Max(p => p.SortIndex) + 1 : 0;
                    profile.SortIndex = dbProfile.SortIndex;
                }


                dbProfile.Description = profile.Description;
                dbProfile.Name = profile.Name;
                dbProfile.Sdp = profile.Sdp;
                dbProfile.UpdatedBy = profile.UpdatedBy;
                dbProfile.UpdatedOn = DateTime.UtcNow;

                profile.UpdatedOn = dbProfile.UpdatedOn;

                db.SaveChanges();
            }
        }


        public Profile GetById(Guid id)
        {
            using (var db = GetDbContext())
            {
                var profile = db.Profiles
                    .Include(p => p.ProfileGroups)
                    .Include(p => p.UserAgents.Select(o => o.UserAgent))
                    .SingleOrDefault(p => p.Id == id);
                return MapToProfile(profile);
            }
        }

        public List<Profile> GetAll()
        {
            return GetProfilesByExpression(p => true);
        }

        public List<Profile> FindProfiles(string searchString)
        {
            return GetProfilesByExpression(p =>
                p.Description.ToLower().Contains(searchString) ||
                p.Name.ToLower().Contains(searchString) ||
                p.Sdp.ToLower().Contains(searchString));
        }

        private List<Profile> GetProfilesByExpression(Expression<Func<ProfileEntity, bool>> whereExpression)
        {
            using (var db = GetDbContext())
            {
                var profiles = db.Profiles
                    .Include(p => p.ProfileGroups)
                    .Include(p => p.UserAgents.Select(o => o.UserAgent))
                    .Where(whereExpression)
                    .ToList();

                return profiles.OrderBy(p => p.SortIndex).Select(MapToProfile).ToList();
            }
        }

        public IList<ProfileNameAndSdp> GetAllProfileNamesAndSdp()
        {
            using (var db = GetDbContext())
            {
                var profiles = db.Profiles.OrderBy(p => p.SortIndex)
                    .Select(p => new ProfileNameAndSdp() {Name = p.Name, Sdp = p.Sdp}).ToList();
                return profiles;
            }
        }

        public IList<ProfileInfo> GetAllProfileInfos()
        {
            using (var db = GetDbContext())
            {
                return db.Profiles.Select(p => new ProfileInfo() {Id = p.Id, Name = p.Name}).ToList();
            }
        }

        public void Delete(Guid id)
        {
            using (var db = GetDbContext())
            {
                var profile = db.Profiles.SingleOrDefault(p => p.Id == id);
                if (profile == null)
                {
                    return;
                }
                db.Profiles.Remove(profile);

                db.SaveChanges();
            }
        }

        public void SetProfileSortIndex(IList<Tuple<Guid, int>> profileTuples)
        {
            using (var db = GetDbContext())
            {
                foreach (var tuple in profileTuples)
                {
                    var id = tuple.Item1;
                    var sortIndex = tuple.Item2;

                    var profile = db.Profiles.SingleOrDefault(p => p.Id == id);
                    if (profile == null)
                    {
                        continue;
                    }

                    profile.SortIndex = sortIndex;
                }

                db.SaveChanges();
            }
        }

        private Profile MapToProfile(ProfileEntity dbProfile)
        {
            if (dbProfile == null) { return null; }

          var profile = new Profile
            {
                Description = dbProfile.Description,
                Id = dbProfile.Id,
                Name = dbProfile.Name,
                Sdp = dbProfile.Sdp,
                SortIndex = dbProfile.SortIndex,
                CreatedBy = dbProfile.CreatedBy,
                CreatedOn = dbProfile.CreatedOn,
                UpdatedBy = dbProfile.UpdatedBy,
                UpdatedOn = dbProfile.UpdatedOn,
                Groups = MapToProfileGroups(dbProfile.ProfileGroups),
                UserAgents = dbProfile.UserAgents == null ? new List<UserAgent>()
                        : dbProfile.UserAgents.Select(oua => MapToUserAgent(oua.UserAgent)).ToList()
            };

            return profile;
        }

        private ICollection<ProfileGroupInfo> MapToProfileGroups(ICollection<ProfileGroupProfileOrdersEntity> groups)
        {
            var l = new List<ProfileGroupInfo>();

            foreach (var group in groups)
            {
                l.Add(AutoMapper.Mapper.Map<ProfileGroupInfo>(group.ProfileGroup));
            }

            return l;
        }

        private static UserAgent MapToUserAgent(UserAgentEntity userAgent)
        {
            return userAgent == null ? null : new UserAgent()
            {
                Ax = userAgent.Ax,
                Height = userAgent.Height,
                Id = userAgent.Id,
                Identifier = userAgent.Identifier,
                Image = userAgent.Image,
                MatchType = userAgent.MatchType,
                Name = userAgent.Name,
                UserInterfaceLink = userAgent.UserInterfaceLink,
                Width = userAgent.Width,
                Api = userAgent.Api,
                Lines = userAgent.Lines,
                Inputs = userAgent.Inputs,
                InputMinDb = userAgent.MinInputDb,
                InputMaxDb = userAgent.MaxInputDb,
                Comment = userAgent.Comment
            };
        }

    }
}