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
using CCM.Core.Security;
using CCM.Data.Entities;
using LazyCache;

namespace CCM.Data.Repositories
{
    public class OwnersRepository : BaseRepository, IOwnersRepository
    {
        public OwnersRepository(IAppCache cache) : base(cache)
        {
        }

        public void Save(Owner owner)
        {
            using (var db = GetDbContext())
            {
                OwnerEntity dbOwner = null;


                if (owner.Id != Guid.Empty)
                {
                    dbOwner = db.Owners.SingleOrDefault(o => o.Id == owner.Id);
                }

                if (dbOwner == null)
                {
                    dbOwner = new OwnerEntity()
                    {
                        Id = Guid.NewGuid(),
                        CreatedBy = owner.CreatedBy,
                        CreatedOn = DateTime.UtcNow
                    };
                    db.Owners.Add(dbOwner);
                    owner.Id = dbOwner.Id;
                    owner.CreatedOn = dbOwner.CreatedOn;
                }


                dbOwner.Name = owner.Name;
                dbOwner.UpdatedBy = owner.UpdatedBy;
                dbOwner.UpdatedOn = DateTime.UtcNow;
                owner.UpdatedOn = dbOwner.UpdatedOn;

                db.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            using (var db = GetDbContext())
            {
                OwnerEntity owner = db.Owners.SingleOrDefault(o => o.Id == id);
                if (owner == null)
                {
                    return;
                }

                db.Owners.Remove(owner);
                db.SaveChanges();
            }
        }

        public List<Owner> GetAll()
        {
            using (var db = GetDbContext())
            {
                var dbOwners = db.Owners.ToList();
                return dbOwners.Select(owner => MapOwner(owner)).OrderBy(o => o.Name).ToList();
            }
        }

        public Owner GetById(Guid id)
        {
            using (var db = GetDbContext())
            {
                var owner = db.Owners.SingleOrDefault(o => o.Id == id);
                return owner == null ? null : MapOwner(owner);
            }
        }

        public Owner GetByName(string name)
        {
            using (var db = GetDbContext())
            {
                name = (name ?? string.Empty).ToLower();

                var owner = db.Owners.SingleOrDefault(o => o.Name.ToLower() == name);
                return owner == null ? null : MapOwner(owner, false);
            }
        }

        public List<Owner> FindOwners(string search)
        {
            using (var db = GetDbContext())
            {
                var dbOwners = db.Owners.Where(o => o.Name.ToLower().Contains(search.ToLower())).ToList();
                return dbOwners.Select(owner => MapOwner(owner)).OrderBy(o => o.Name).ToList();
            }
        }

        private Owner MapOwner(OwnerEntity owner, bool includeUsers = true)
        {
            return owner == null ? null : new Owner
                {
                    Id = owner.Id,
                    Name = owner.Name,
                    CreatedBy = owner.CreatedBy,
                    CreatedOn = owner.CreatedOn,
                    UpdatedBy = owner.UpdatedBy,
                    UpdatedOn = owner.UpdatedOn,
                    Users = includeUsers && owner.Users != null
                        ? owner.Users.Select(MapUser).OrderBy(u => u.UserName).ToList()
                        : new List<CcmUser>()
                };
        }

        private CcmUser MapUser(UserEntity dbUser)
        {
            return new CcmUser
            {
                Comment = dbUser.Comment,
                FirstName = dbUser.FirstName,
                Id = dbUser.Id.ToString(),
                LastName = dbUser.LastName,
                UserType = dbUser.UserType,
                UserName = dbUser.UserName,
                DisplayName = dbUser.DisplayName
            };
        }

    }
}