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

﻿using System.Linq;
using System.Web.Mvc;
using CCM.Core.Interfaces.Managers;
using CCM.Core.Interfaces.Repositories;
using CCM.Web.Mappers;
using NLog;
using Microsoft.AspNet.SignalR;

namespace CCM.Web.Hubs
{
    public class WebGuiHub : HubBase
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly Throttler CodecsOnlineThrottler;
        private static readonly Throttler OngoingCallsThrottler;
        private static readonly Throttler OldCallsThrottler;

        static WebGuiHub()
        {
            CodecsOnlineThrottler = new Throttler("RegisteredCodecs", 200, UpdateCodecsOnline);
            OngoingCallsThrottler = new Throttler("OnGoingCalls", 300, UpdateOngoingCalls);
            OldCallsThrottler = new Throttler("OldCalls", 400, UpdateOldCalls);
        }

        public static void ThrottlingUpdateCodecsOnline()
        {
            CodecsOnlineThrottler.Trigger();
        }

        private static void UpdateCodecsOnline()
        {
            var registeredSipRepository = (IRegisteredSipRepository)DependencyResolver.Current.GetService(typeof(IRegisteredSipRepository));
            var settingsManager = (ISettingsManager)DependencyResolver.Current.GetService(typeof(ISettingsManager));

            var registeredSipsOnline = registeredSipRepository.GetCachedRegisteredSips();
            var sipDomain = settingsManager.SipDomain;

            var codecsOnline = registeredSipsOnline.Select(sip => RegisteredSipOverviewDtoMapper.MapToDto(sip, sipDomain)).ToList();

            log.Debug("SignalR updating list of codecs online on web gui clients. #Codecs=" + codecsOnline.Count);
            var webGuiHubContext = GlobalHost.ConnectionManager.GetHubContext<WebGuiHub>();
            webGuiHubContext.Clients.All.codecsOnline(codecsOnline);
        }

        public static void ThrottlingUpdateOldCalls()
        {
            OldCallsThrottler.Trigger();
        }

        private static void UpdateOldCalls()
        {
            var callHistoryRepository = (ICallHistoryRepository)DependencyResolver.Current.GetService(typeof(ICallHistoryRepository));
            var settingsManager = (ISettingsManager)DependencyResolver.Current.GetService(typeof(ISettingsManager));
            var oldCalls = callHistoryRepository.GetOldCalls(settingsManager.LatestCallCount, true);

            log.Debug("SignalR updating list of old calls on web gui clients. #Old calls=" + oldCalls.Count);
            var webGuiHubContext = GlobalHost.ConnectionManager.GetHubContext<WebGuiHub>();
            webGuiHubContext.Clients.All.oldCalls(oldCalls);
        }

        public static void ThrottlingUpdateOngoingCalls()
        {
            OngoingCallsThrottler.Trigger();
        }

        private static void UpdateOngoingCalls()
        {
            var callRepository = (ICallRepository)DependencyResolver.Current.GetService(typeof(ICallRepository));
            var onGoingCalls = callRepository.GetOngoingCalls(true);

            log.Debug("SignalR updating list of ongoing calls on web gui clients. #Ongoing calls=" + onGoingCalls.Count);
            var webGuiHubContext = GlobalHost.ConnectionManager.GetHubContext<WebGuiHub>();
            webGuiHubContext.Clients.All.ongoingCalls(onGoingCalls);
        }

    }
}