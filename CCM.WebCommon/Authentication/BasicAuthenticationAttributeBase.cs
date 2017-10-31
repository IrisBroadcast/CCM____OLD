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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Managers;
using CCM.Core.Radius;
using CCM.Core.Security;
using NLog;

namespace CCM.WebCommon.Authentication
{
    /// <remarks>
    /// http://www.asp.net/web-api/overview/security/authentication-filters
    /// </remarks>>
    public abstract class BasicAuthenticationAttributeBase : Attribute, IAuthenticationFilter
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        public bool AllowMultiple { get { return false; } }

        // Due to earlier intermittent errors during many simultanious API calls with authentication we're not using Ninject dependency injection here
        // The error occured because the same Ninject created instance were reused between calls. Possibly caused by ASP.NET reusing attributs/filters.
        public IRadiusProvider RadiusProvider { get { return new RadiusProvider(); } }
        //[Inject]
        //public IRadiusProvider RadiusProvider { get; set; }
        
        protected abstract CcmUser GetUser(string userName);
        protected abstract CcmRole GetUserRoles(CcmUser user);

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = context.Request;

            AuthenticationHeaderValue authorization = request.Headers.Authorization;

            if (authorization == null)
            {
                // No authentication was attempted (for this authentication method).
                // Do not set either Principal (which would indicate success) or ErrorResult (indicating an error).
                log.Debug("No authentication header in request for {0}", request.RequestUri);
                return;
            }

            if (authorization.Scheme != "Basic")
            {
                // No authentication was attempted (for this authentication method).
                // Do not set either Principal (which would indicate success) or ErrorResult (indicating an error).
                log.Debug("Not a Basic authentication header in request for {0}", request.RequestUri);
                return;
            }

            if (string.IsNullOrEmpty(authorization.Parameter))
            {
                // Authentication was attempted but failed. Set ErrorResult to indicate an error.
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                log.Debug("Missing authentication credentials in request for {0}", request.RequestUri);
                return;
            }

            Credentials credentials = BasicAuthenticationHelper.ParseCredentials(authorization.Parameter);

            if (credentials == null)
            {
                // Authentication was attempted but failed. Set ErrorResult to indicate an error.
                context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request);
                log.Debug("No username and password in request for {0}", request.RequestUri);
                return;
            }

            try
            {
                IPrincipal principal = await AuthenticateAsync(credentials.Username, credentials.Password, cancellationToken);

                if (principal == null)
                {
                    // Authentication was attempted but failed. Set ErrorResult to indicate an error.
                    context.ErrorResult = new AuthenticationFailureResult("Invalid username or password", request);
                    log.Debug("Invalid username or password in request for {0}", request.RequestUri);
                }
                else
                {
                    // Authentication was attempted and succeeded. Set Principal to the authenticated user.
                    context.Principal = principal;
                }
            }
            catch (Exception ex)
            {
                context.ErrorResult = new InternalServerErrorResult(request);
                log.Error(string.Format("Error in BasicAuthenticationAttribute on request to {0}", request.RequestUri), ex);
            }
        }

        protected async Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CcmUser user = null;
#if DEBUG
            if (userName == ApplicationSettings.DiscoveryUsername && password == ApplicationSettings.DiscoveryPassword)
            {
                // Always authenticated
                user = new CcmUser(userName);
            }
#endif

            if (user == null)
            {
                if (RadiusProvider.Authenticate(userName, password)) // TODO: Anropa asynkront
                {
                    user = GetUser(userName);
                }
            }

            if (user == null)
            {
                return null;
            }

            List<Claim> claims = new List<Claim> {new Claim(ClaimTypes.Name, userName)};

            var role = GetUserRoles(user);

            if (role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            ClaimsIdentity identity = new ClaimsIdentity(claims, AuthenticationTypes.Basic);

            var principal = new ClaimsPrincipal(identity);
            return principal;
        }
        
        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}