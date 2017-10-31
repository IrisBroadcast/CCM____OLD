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

﻿using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using NLog;

namespace CCM.Web.Hubs
{
    public abstract class HubBase : Hub
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected string Referer => Context.Headers["Referer"] ?? string.Empty;
        protected string RemoteIp => Context.Request.Environment["server.RemoteIpAddress"] as string ?? string.Empty;

        public override Task OnConnected()
        {
            if (log.IsInfoEnabled)
            {
                log.Info("SignalR client on {0} connected to {1}. Id={2} ({3})", RemoteIp, GetType().Name, Context.ConnectionId, Referer);
            }
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            if (stopCalled)
            {
                if (log.IsInfoEnabled)
                {
                    log.Info("SignalR client on {0} disconnected gracefully from {1}. Connection id={2} ({3})",
                    RemoteIp, GetType().Name, Context.ConnectionId, Referer);
                }
            }
            else
            {
                log.Warn("SignalR client on {0} disconnected ungracefully from {1}. Connection id={2}  ({3})",
                    RemoteIp, GetType().Name, Context.ConnectionId, Referer);
            }
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            if (log.IsInfoEnabled)
            {
                log.Info("SignalR client on {0} reconnected to {1}. Connection id={2} ({3})", 
                    RemoteIp, GetType().Name, Context.ConnectionId, Referer);
            }
            return base.OnReconnected();
        }

    }
}