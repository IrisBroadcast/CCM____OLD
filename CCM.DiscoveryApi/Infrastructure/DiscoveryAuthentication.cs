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
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using CCM.DiscoveryApi.Models;
using CCM.WebCommon.Authentication;

namespace CCM.DiscoveryApi.Infrastructure
{
    /// <summary>
    /// Authenticeringsklass.
    /// Via denna klass kan användare authenticeras via Authorize-attribut.
    /// Klassen läser även andra parametrar, tolkar till objekt och lagrar på request-objektet
    /// </summary>
    public class DiscoveryAuthentication : AuthenticationAttributeBase
    {
        public override async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = context.Request;

            try
            {
                SrDiscoveryParameters parameters = ParseParameters(request);

                if (parameters == null)
                {
                    context.ErrorResult = new AuthenticationFailureResult("Missing parameters", request, HttpStatusCode.BadRequest);
                    return;
                }

                request.Properties.Add("SRDiscoveryParameters", parameters);

                var userName = parameters.UserName ?? string.Empty;
                var pwdhash = parameters.Pwdhash ?? string.Empty;

                // If there are no credentials, do nothing.
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(pwdhash))
                {
                    context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                    return;
                }

                IPrincipal principal = await AuthenticateAsync(userName, pwdhash, cancellationToken);

                if (principal == null)
                {
                    context.ErrorResult = new AuthenticationFailureResult("Invalid username or password", request);
                }
                else
                {
                    context.Principal = principal;
                }
            }
            catch (Exception ex)
            {
                context.ErrorResult = new InternalServerErrorResult(request);
            }
        }

        private SrDiscoveryParameters ParseParameters(HttpRequestMessage request)
        {
            try
            {
                if (request.Content.Headers.ContentType.MediaType != "application/x-www-form-urlencoded")
                {
                    return null;
                }

                NameValueCollection parameters = request.Content.ReadAsFormDataAsync().Result;

                var nonFilterKeys = new List<string>() { "username", "pwdhash", "caller", "callee", "includeCodecsInCall" };

                IList<KeyValuePair<string,string>> filters = parameters.AllKeys
                    .Where(k => !nonFilterKeys.Contains(k.ToLower()))
                    .Select(key => new KeyValuePair<string,string>(key, parameters[key]))
                    .ToList();

                bool includeCodecsInCall;
                bool.TryParse(parameters["includeCodecsInCall"], out includeCodecsInCall);

                var p = new SrDiscoveryParameters()
                {
                    UserName = parameters["username"] ?? string.Empty,
                    Pwdhash = parameters["pwdhash"] ?? string.Empty,
                    Caller = parameters["caller"] ?? string.Empty,
                    Callee = parameters["callee"] ?? string.Empty,
                    IncludeCodecsInCall = includeCodecsInCall,
                    Filters = filters
                };

                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Discovery request to {5} params. User name:'{0}' Password:{1} Caller:'{2}' Callee:'{3}' Filters: {4}",
                        p.UserName, string.IsNullOrEmpty(p.Pwdhash) ? "<missing>" : "********",
                        string.IsNullOrEmpty(p.Caller) ? "<missing>" : p.Caller, string.IsNullOrEmpty(p.Callee) ? "<missing>" : p.Callee,
                        string.Join(", ", p.Filters.Select(f => string.Format("{0}={1}", f.Key, f.Value))),
                        request.RequestUri.OriginalString);
                }

                return p;
            }
            catch (Exception ex)
            {
                log.Error("Error when parsing parameters", ex);
                return null;
            }
        }
    }
}