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
using System.Data.Entity;
using CCM.Core.Entities;
using CCM.Core.Helpers;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Security;
using CCM.Data.Entities;
using LazyCache;
using NLog;

namespace CCM.Data.Repositories
{

    public class UserRepository : BaseRepository, ICcmUserRepository
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public UserRepository(IAppCache cache) : base(cache)
        {
        }

        public void Create(CcmUser ccmUser, bool currentUserLocalAdmin = false)
        {
            if (ccmUser == null)
            {
                throw new ArgumentNullException("ccmUser");
            }

            using (var db = GetDbContext())
            {
                var dbUser = new UserEntity();
                CcmUserToUser(db, ccmUser, dbUser, currentUserLocalAdmin);

                dbUser.CreatedBy = ccmUser.CreatedBy;
                dbUser.CreatedOn = DateTime.UtcNow;
                ccmUser.CreatedOn = dbUser.CreatedOn;

                db.Users.Add(dbUser);
                db.SaveChanges();
            }
        }

        public void Delete(CcmUser ccmUser)
        {
            if (ccmUser == null)
            {
                throw new ArgumentNullException("ccmUser");
            }

            using (var db = GetDbContext())
            {
                UserEntity user = db.Users.SingleOrDefault(u => u.Id == new Guid(ccmUser.Id));

                if (user != null)
                {
                    if (user.RegisteredSips != null)
                    {
                        user.RegisteredSips.Clear();
                    }

                    db.Users.Remove(user);
                    db.SaveChanges();
                }
                else
                {
                    log.Info(string.Format("Could not find user {0}", ccmUser.Id));
                }
            }
        }

        public CcmUser GetUser(string userId)
        {
            using (var db = GetDbContext())
            {
                UserEntity user = db.Users.SingleOrDefault(u => u.Id == new Guid(userId));
                var ccmUser = MapToCcmUser(user);
                return ccmUser;
            }
        }

        public List<CcmUser> GetAllUsers()
        {
            using (var db = GetDbContext())
            {
                var users = db.Users
                    .Include(u => u.Owner)
                    .Include(u => u.CodecType)
                    .Include(u => u.Role)
                    .Where(u => u.LocalUser == false)
                    .ToList();

                return users.Select(MapToCcmUser).OrderBy(u => u.UserName).ToList();
            }
        }

        public List<CcmUser> FindUsers(string startsWith)
        {
            using (var db = GetDbContext())
            {
                var users = db.Users
                    .Where(u => u.UserName.Contains(startsWith) && u.LocalUser == false)
                    .ToList();
                return users.Select(MapToCcmUser).OrderBy(u => u.UserName).ToList();
            }
        }

        public CcmUser FindById(string userId)
        {
            using (var db = GetDbContext())
            {
                UserEntity user = db.Users.SingleOrDefault(u => u.Id == new Guid(userId));
                return MapToCcmUser(user);
            }
        }

        public CcmUser FindByUserName(string userName)
        {
            using (var db = GetDbContext())
            {
                UserEntity user = db.Users.SingleOrDefault(u => u.UserName == userName);
                return MapToCcmUser(user);
            }
        }

        public void Update(CcmUser ccmUser, bool currentUserLocalAdmin = false)
        {
            using (var db = GetDbContext())
            {
                UserEntity dbUser = db.Users.SingleOrDefault(u => u.Id == new Guid(ccmUser.Id));
                if (dbUser != null)
                {
                    CcmUserToUser(db, ccmUser, dbUser, currentUserLocalAdmin);
                    db.SaveChanges();
                }
            }
        }

        public void UpdateComment(string username, string comment)
        {
            using (var db = GetDbContext())
            {
                UserEntity dbUser = db.Users.SingleOrDefault(u => u.UserName == username);

                if (dbUser != null)
                {
                    dbUser.Comment = comment;
                    db.SaveChanges();
                }
            }
        }

        public List<CcmRole> GetUserRoles(CcmUser ccmUser)
        {
            using (var db = GetDbContext())
            {
                var user = db.Users.SingleOrDefault(u => u.Id == new Guid(ccmUser.Id));

                if (user == null || user.Role == null)
                {
                    return new List<CcmRole>();
                }

                var ccmRoles = new List<CcmRole> {new CcmRole(user.Role.Id.ToString(), user.Role.Name)};
                return ccmRoles;
            }
        }

        public CcmUser GetUserByRadiusId(int id)
        {
            using (var db = GetDbContext())
            {
                UserEntity user = db.Users.SingleOrDefault(u => u.RadiusId == id);
                if (user != null)
                {
                    var ccmUser = MapToCcmUser(user);
                    return ccmUser;
                }
            }
            return null;
        }

        public bool IsLocalUser(string username)
        {
            using (var db = GetDbContext())
            {
                return db.Users.Any(u => u.UserName == username && u.LocalUser == true);
            }
        }

        public bool UserIsLocalAdmin(string username)
        {
            using (var db = GetDbContext())
            {
                return db.Users.Any(u => u.UserName == username && u.Role.Name == ApplicationConstants.Admin &&
                                              u.UserType == UserType.CcmUser && u.LocalUser == true);
            }
        }

        public bool AuthenticateLocal(string username, string password)
        {
            using (var db = GetDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.UserName == username);
                if (user == null || !user.LocalUser)
                {
                    return false;
                }

                var localPassword = db.LocalPasswords.SingleOrDefault(l => l.UserId == user.Id);
                if (localPassword == null)
                {
                    return false;
                }

                string hash = CryptoHelper.Md5HashSaltedPassword(password, localPassword.Salt);

                return hash == localPassword.Password;
            }
        }


        public List<CcmRole> GetRoles()
        {
            using (var db = GetDbContext())
            {
                var roles = db.Roles.OrderBy(r => r.Name)
                    .Select(r => new CcmRole() {Id = r.Id.ToString(), Name = r.Name})
                    .ToList();

                return roles;
            }
        }

        public List<CcmUser> GetAllSipUsers()
        {
            using (var db = GetDbContext())
            {
                var users = db.Users
                    .Where(u => u.UserType == UserType.SIP)
                    .ToList();
                return users.Select(MapToCcmUser).OrderBy(u => u.UserName).ToList();
            }
        }

        private static CcmUser MapToCcmUser(UserEntity user)
        {
            return user == null ? null : new CcmUser
                {
                    RadiusId = user.RadiusId,
                    Comment = user.Comment,
                    DisplayName = user.DisplayName,
                    FirstName = user.FirstName,
                    Id = user.Id.ToString(),
                    LastName = user.LastName,
                    UserName = user.UserName,
                    UserType = user.UserType,
                    Owner = user.Owner != null ? new Owner() {Id = user.Owner.Id, Name = user.Owner.Name} : null,
                    LocalUser = user.LocalUser,
                    RoleId = user.Role != null ? user.Role.Id.ToString() : string.Empty,
                    CodecType = user.CodecType != null ? new CodecType()
                        {
                            Id = user.CodecType.Id,
                            Name = user.CodecType.Name,
                            Color = user.CodecType.Color
                        } : null,
                    AccountLocked = user.AccountLocked,
                    CreatedBy = user.CreatedBy,
                    CreatedOn = user.CreatedOn,
                    UpdatedBy = user.UpdatedBy,
                    UpdatedOn = user.UpdatedOn
                };
        }

        private void CcmUserToUser(CcmDbContext cxt, CcmUser ccmUser, UserEntity dbUser, bool currentUserLocalAdmin = false)
        {
            dbUser.RadiusId = ccmUser.RadiusId;
            dbUser.Comment = ccmUser.Comment;
            dbUser.DisplayName = ccmUser.DisplayName;
            dbUser.FirstName = ccmUser.FirstName;
            dbUser.LastName = ccmUser.LastName;
            dbUser.Id = new Guid(ccmUser.Id);
            dbUser.UserName = ccmUser.UserName;
            dbUser.UserType = ccmUser.UserType;
            dbUser.Owner = ccmUser.Owner != null ? cxt.Owners.SingleOrDefault(o => o.Id == ccmUser.Owner.Id) : null;
            dbUser.LocalUser = ccmUser.LocalUser;
            dbUser.CodecType = ccmUser.CodecType != null ? cxt.CodecTypes.SingleOrDefault(c => c.Id == ccmUser.CodecType.Id) : null;
            dbUser.AccountLocked = ccmUser.AccountLocked;
            dbUser.UpdatedBy = ccmUser.UpdatedBy;
            dbUser.UpdatedOn = DateTime.UtcNow;
            ccmUser.UpdatedOn = dbUser.UpdatedOn;

            var role = string.IsNullOrWhiteSpace(ccmUser.RoleId) ? null : cxt.Roles.SingleOrDefault(r => r.Id == new Guid(ccmUser.RoleId));

            // Only admins allowed to assign admin role to account
            if (currentUserLocalAdmin || role == null || role.Name != ApplicationConstants.Admin)
            {
                dbUser.Role = role;
            }
            

        }
    }
}