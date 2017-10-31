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

using System.Reflection;
using System.Web.Http.Controllers;
using NLog;

namespace CCM.Web.Authentication
{
    public class CcmApiAuthorizeAttribute : ApiAuthorizeAttribute
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            log.Info("Authorization");

            if (!IsAuthorized(actionContext))
            {
                HandleUnauthorizedRequest(actionContext);
            }
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (!actionContext.RequestContext.Principal.Identity.IsAuthenticated)
            {
                log.Info("Authorization failed because user is not authenticated");
                return false;
            }

            log.Info("Attribute roles: {0}", Roles);

            if (Roles.Length > 0)
            {
                var roles = Roles.Split(',');
                foreach (var role in roles)
                {
                    if (actionContext.RequestContext.Principal.IsInRole(role.Trim()))
                    {
                        log.Info("User {0} is in role {1}", actionContext.RequestContext.Principal.Identity.Name, role);
                        return true;
                    }
                    else
                    {
                        log.Info("User {0} is NOT in role {1}", actionContext.RequestContext.Principal.Identity.Name, role);
                    }
                }
                return false;
            }

            return true;
        }

        
     
    }
}