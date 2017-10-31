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
using System.Reflection;
using System.Threading.Tasks;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Managers;
using NLog;
using Microsoft.AspNet.Identity;

namespace CCM.Core.Security
{
    /// <summary>
    /// Special class to be able to create and authenticate against FreeRadius server
    /// </summary>
    public class RadiusUserManager : UserManager<CcmUser>, IRadiusUserManager
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly ICcmUserManager _ccmUserManager;
        private readonly IRadiusProvider _radiusProvider;

        public RadiusUserManager(ICcmUserManager userManager, IRadiusProvider radiusProvider, IUserStore<CcmUser> store) : base(store)
        {
            _ccmUserManager = userManager;
            _radiusProvider = radiusProvider;
        }

        public override Task<CcmUser> FindAsync(string userName, string password)
        {
            if (_radiusProvider.Authenticate(userName, password))
            {
                Task<CcmUser> user = FindByNameAsync(userName);
                return user;
            }
            return null;
        }

        public Task<CcmUser> FindLocalAsync(string userName, string password)
        {
            if (_ccmUserManager.AuthenticateLocal(userName, password))
            {
                Task<CcmUser> user = FindByNameAsync(userName);
                return user;
            }

            return null;
        }

        public override async Task<IdentityResult> CreateAsync(CcmUser user, string password)
        {
            throw new NotImplementedException();
            //try
            //{
            //    _ccmUserManager.Create(user, password);
            //}
            //catch (Exception ex)
            //{
            //    return IdentityResult.Failed(ex.Message);
            //}
            //return IdentityResult.Success;
        }
    }
}