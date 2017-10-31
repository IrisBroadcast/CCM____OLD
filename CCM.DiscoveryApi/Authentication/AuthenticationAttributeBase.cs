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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Managers;
using NLog;
using Ninject;

namespace CCM.DiscoveryApi.Authentication
{
    /// <summary>
    /// Based on code / example from
    /// http://www.asp.net/web-api/overview/security/authentication-filters
    /// </summary>
    public abstract class AuthenticationAttributeBase : Attribute, IAuthenticationFilter
    {
        public bool AllowMultiple { get { return false; } }

        [Inject]
        public IRadiusProvider RadiusProvider { get; set; }

        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public abstract Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken);

        protected virtual async Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

#if DEBUG
            bool authenticated = (userName == ApplicationSettings.DiscoveryUsername && password == ApplicationSettings.DiscoveryPassword); // Alltid authenticerad
#else
            bool authenticated = false;
#endif

            if (!authenticated)
            {
                authenticated = RadiusProvider.Authenticate(userName, password); // TODO: Anropa asynkront
            }

            if (!authenticated) { return null; }

            // Create a ClaimsIdentity with all the claims for this user.
            Claim nameClaim = new Claim(ClaimTypes.Name, userName);
            List<Claim> claims = new List<Claim> { nameClaim };

            ClaimsIdentity identity = new ClaimsIdentity(claims, AuthenticationTypes.Basic);
            var principal = new ClaimsPrincipal(identity);

            return principal;
        }

        public virtual Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}