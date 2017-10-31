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
using CCM.Core.Cache;
using CCM.Core.Entities.Specific;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Managers;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Security;
using LazyCache;
using NLog;

namespace CCM.Core.Managers
{
    public class CcmUserManager : ICcmUserManager
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly ICcmUserRepository _userRepository;
        private readonly IRadiusUserRepository _radiusUserRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IAppCache _cache;

        public CcmUserManager(ICcmUserRepository ccmUserRepository, IRadiusUserRepository radiusUserRepository, IRoleRepository roleRepository, IAppCache cache)
        {
            _userRepository = ccmUserRepository;
            _radiusUserRepository = radiusUserRepository;
            _roleRepository = roleRepository;
            _cache = cache;
        }

        public void Create(CcmUser user)
        {
            _userRepository.Create(user);
        }

        public void Create(CcmUser user, string password, bool currentUserIsLocalAdmin = false)
        {
            if (_radiusUserRepository.UserExists(user.UserName))
            {
                log.Warn("Can't create user. Username {0} already exists in RADIUS database", user.UserName);
                throw new ApplicationException("Användarnamnet finns redan i RADIUS databas.");
            }

            if (_userRepository.FindByUserName(user.UserName) != null)
            {
                log.Warn("Can't create user. Username {0} already exists in CCM database", user.UserName);
                throw new ApplicationException("Användarnamnet finns redan i CCMs databas.");
            }

            log.Debug("Creating user in radius.");
            user.RadiusId = _radiusUserRepository.CreateUser(user.UserName, password, user.UserType);

            if (user.RadiusId <= 0)
            {
                throw new ApplicationException("Användaren kunde inte skapas i RADIUS.");
            }

            log.Debug("Creating user in ccm db");
            _userRepository.Create(user, currentUserIsLocalAdmin);
        }

        public bool Delete(CcmUser user)
        {
            if (_radiusUserRepository.DeleteUser(user.RadiusId))
            {
                _userRepository.Delete(user);
                return true;
            }
            return false;
        }

        public CcmUser GetUser(string userId)
        {
            return _userRepository.GetUser(userId);
        }

        public List<CcmUser> GetAllUsers()
        {
            return _userRepository.GetAllUsers() ?? new List<CcmUser>();
        }

        public List<CcmUser> FindUsers(string startsWith)
        {
            return _userRepository.FindUsers(startsWith) ?? new List<CcmUser>();
        }

        public CcmUser FindById(string userId)
        {
            return _userRepository.FindById(userId);
        }

        public CcmUser FindByUserName(string userName)
        {
            return _userRepository.FindByUserName(userName);
        }

        public void Update(CcmUser user, bool currentUserLocalAdmin = false)
        {
            var existingUser = _userRepository.GetUser(user.Id);
            
            // Endast lokal admin får tilldela admin-rollen
            var adminId = _roleRepository.GetRoleIdByName(ApplicationConstants.Admin).ToString();
            if (user.RoleId == adminId && existingUser.RoleId != adminId && !currentUserLocalAdmin)
            {
                throw new Exception("Endast Root-användaren får sätta Admin-rollen.");
            }

            if (existingUser.UserName != user.UserName)
            {
                if (!_radiusUserRepository.ChangeUserName(existingUser.RadiusId, user.UserName))
                {
                    throw new Exception("Unable to change username");
                }
            }

            _userRepository.Update(user, currentUserLocalAdmin);
        }

        public void SaveComment(RegisteredSipComment sipComment)
        {
            _userRepository.UpdateComment(sipComment.RegisteredSipUserName, sipComment.Comment);

            // Invalidate registered sip in cache
            _cache.ClearRegisteredSips();
        }
        
        public void UpdatePassword(long userId, string password, UserType userType)
        {
            if (!_radiusUserRepository.UpdatePassword(userId, password, userType))
            {
                throw new Exception("Unable to update user");
            }
        }

        /// <summary>
        /// Gets the user roles as strings.
        /// </summary>
        public IList<string> GetUserRolesAsStrings(CcmUser user)
        {
            var roles = _userRepository.GetUserRoles(user);
            return roles.Select(role => role.Name).ToList();
        }

        /// <summary>
        /// Imports the users.
        /// </summary>
        /// <returns>Number of created users</returns>
        public int ImportUsers()
        {
            var radiusUsers = _radiusUserRepository.GetUsers();
            int created = 0;

            foreach (var radiusUser in radiusUsers)
            {
                if (_userRepository.GetUserByRadiusId(radiusUser.Id) == null && _userRepository.FindByUserName(radiusUser.Username) == null)
                {
                    var user = new CcmUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = radiusUser.Username,
                        RadiusId = radiusUser.Id,
                        UserType = UserType.SIP
                    };

                    _userRepository.Create(user);
                    created++;
                }
            }

            return created;
        }

        public bool AuthenticateLocal(string username, string password)
        {
            return _userRepository.AuthenticateLocal(username, password);
        }

        public bool UserIsLocalAdmin(string username)
        {
            return _userRepository.UserIsLocalAdmin(username);
        }

        public List<CcmRole> GetRoles()
        {
            return _userRepository.GetRoles();
        }

        public void ToggleUserLock(Guid id)
        {
            var user = _userRepository.GetUser(id.ToString());

            if (user == null)
            {
                return;
            }

            if (user.AccountLocked)
            {
                if (_radiusUserRepository.Unlock(user.RadiusId, user.UserName))
                {
                    user.AccountLocked = false;
                }
            }
            else
            {
                if (_radiusUserRepository.Lock(user.RadiusId))
                {
                    user.AccountLocked = true;
                }
            }

            _userRepository.Update(user);
        }
    }
}