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

﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using NLog;

namespace CCM.WebCommon.Infrastructure.WebApi
{
    public class StopwatchAttribute : ActionFilterAttribute
    {
        private readonly bool _logStartAction;
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const string StopwatchKey = "StopwatchKey";

        public StopwatchAttribute(bool logStartAction = false)
        {
            _logStartAction = logStartAction;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            if (log.IsDebugEnabled)
            {
                if (_logStartAction)
                {
                    IEnumerable<string> headers = actionContext.Request.Headers.Select(h => string.Format("{0}:{1}", h.Key, string.Join(", ", h.Value)));
                    log.Debug("Web api request to {0} {1}", actionContext.Request.RequestUri.AbsolutePath, string.Join("; ", headers));
                }
                var stopWatch = Stopwatch.StartNew();
                actionContext.Request.Properties[StopwatchKey] = stopWatch;
                stopWatch.Reset();
                stopWatch.Start();
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            if (log.IsDebugEnabled)
            {
                var stopwatch = actionExecutedContext.Request.Properties[StopwatchKey] as Stopwatch;
                if (stopwatch != null)
                {
                    stopwatch.Stop();
                    log.Debug("Web api request to {0} took {1} ms", actionExecutedContext.Request.RequestUri.AbsolutePath, stopwatch.ElapsedMilliseconds);
                }
            }

        }
    }

}