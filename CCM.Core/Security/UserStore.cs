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
using System.Security.Claims;
using System.Threading.Tasks;
using CCM.Core.Interfaces.Managers;
using Microsoft.AspNet.Identity;

namespace CCM.Core.Security
{
    public class UserStore : IUserClaimStore<CcmUser>, IUserLoginStore<CcmUser>, IUserPasswordStore<CcmUser>, IUserRoleStore<CcmUser>
    {
        private readonly ICcmUserManager userManager;

        public UserStore(ICcmUserManager userManager)
        {
            this.userManager = userManager;
        }

        public Task AddClaimAsync(CcmUser user, Claim claim)
        {
            throw new NotImplementedException("Not yet implemented");
        }

        public Task<IList<Claim>> GetClaimsAsync(CcmUser user)
        {
            return Task.FromResult<IList<Claim>>(new List<Claim>());
        }

        public Task RemoveClaimAsync(CcmUser user, Claim claim)
        {
            throw new NotImplementedException("Not yet implemented");
        }

        public Task AddLoginAsync(CcmUser user, UserLoginInfo login)
        {
            throw new NotImplementedException("Not yet implemented");
        }

        public Task<CcmUser> FindAsync(UserLoginInfo login)
        {
            throw new NotImplementedException("Not yet implemented");
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(CcmUser user)
        {
            throw new NotImplementedException("Not yet implemented");
        }

        public Task RemoveLoginAsync(CcmUser user, UserLoginInfo login)
        {
            throw new NotImplementedException("Not yet implemented");
        }

        public Task<string> GetPasswordHashAsync(CcmUser user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasPasswordAsync(CcmUser user)
        {
            return Task.FromResult(true);
        }

        public Task SetPasswordHashAsync(CcmUser user, string passwordHash)
        {
            return null;
        }

        public Task AddToRoleAsync(CcmUser user, string role)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> GetRolesAsync(CcmUser user)
        {
            return Task.FromResult(userManager.GetUserRolesAsStrings(user));
        }

        public Task<bool> IsInRoleAsync(CcmUser user, string role)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromRoleAsync(CcmUser user, string role)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(CcmUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            userManager.Create(user);
            return null;
        }

        public Task DeleteAsync(CcmUser user)
        {
            if (user != null)
            {
                userManager.Delete(user);
            }

            return null;
        }

        public Task<CcmUser> FindByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Null or empty argument: userId");
            }

            CcmUser user = userManager.FindById(userId);

            if (user != null)
            {
                return Task.FromResult(user);
            }

            return null;
        }

        public Task<CcmUser> FindByNameAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("Null or empty argument: userName");
            }

            CcmUser user = userManager.FindByUserName(userName);

            if (user != null)
            {
                return Task.FromResult(user);
            }

            return null;
        }

        public Task UpdateAsync(CcmUser user)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}