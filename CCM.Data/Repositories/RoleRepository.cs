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
using System.Data;
using System.Linq;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Security;
using CCM.Data.Entities;
using LazyCache;

namespace CCM.Data.Repositories
{
    public class RoleRepository : BaseRepository, IRoleRepository
    {
        public RoleRepository(IAppCache cache) : base(cache)
        {
        }

        public void Create(CcmRole ccmRole)
        {
            using (var db = GetDbContext())
            {
                if (ccmRole == null)
                {
                    throw new ArgumentNullException("ccmRole");
                }

                if (db.Roles.Any(r => r.Name == ccmRole.Name))
                {
                    throw new DuplicateNameException(ccmRole.Name);
                }

                var role = new RoleEntity
                {
                    Id = new Guid(ccmRole.Id),
                    Name = ccmRole.Name
                };

                db.Roles.Add(role);

                db.SaveChanges();
            }
        }

        public void Delete(CcmRole ccmRole)
        {
            if (ccmRole == null)
            {
                throw new ArgumentNullException("ccmRole");
            }

            using (var db = GetDbContext())
            {
                RoleEntity role = db.Roles.SingleOrDefault(r => r.Id.ToString() == ccmRole.Id);
                if (role != null)
                {
                    db.Roles.Remove(role);
                    db.SaveChanges();
                }
            }
        }

        public void Update(CcmRole ccmRole)
        {
            if (ccmRole == null)
            {
                throw new ArgumentNullException("ccmRole");
            }

            using (var db = GetDbContext())
            {
                RoleEntity role = db.Roles.SingleOrDefault(r => r.Id.ToString() == ccmRole.Id);
                if (role == null)
                {
                    throw new Exception("Could not find role");
                }

                role.Name = ccmRole.Name;

                db.SaveChanges();
            }
        }

        public CcmRole FindById(string roleId)
        {
            using (var db = GetDbContext())
            {
                RoleEntity role = db.Roles.SingleOrDefault(r => r.Id.ToString() == roleId);
                if (role == null)
                {
                    return null;
                }

                var ccmRole = new CcmRole(role.Id.ToString(), role.Name);
                return ccmRole;
            }
        }

        public CcmRole FindByName(string name)
        {
            using (var db = GetDbContext())
            {
                RoleEntity role = db.Roles.SingleOrDefault(r => r.Name == name);
                if (role == null)
                {
                    return null;
                }

                var ccmRole = new CcmRole(role.Id.ToString(), role.Name);
                return ccmRole;
            }
        }

        public Guid GetRoleIdByName(string name)
        {
            using (var db = GetDbContext())
            {
                RoleEntity role =
                    db.Roles.SingleOrDefault(r => r.Name.Trim().ToLower() ==
                                                       (name ?? string.Empty).Trim().ToLower());
                return role == null ? Guid.Empty : role.Id;
            }
        }
    }
}