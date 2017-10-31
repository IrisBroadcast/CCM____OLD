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
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Managers;
using FP.Radius;
using NLog;

namespace CCM.Core.Radius
{
    /*
     * Based on example code from
     * https://github.com/frontporch/Radius.NET
     */

    public class RadiusProvider : IRadiusProvider
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public bool Authenticate(string username, string password)
        {
            string hostname = ApplicationSettings.RadiusHost;
            string sharedKey = ApplicationSettings.RadiusSecret;

            log.Debug("User {0} authenticating to Radius server {1}", username, hostname);

            var rc = new RadiusClient(hostname, sharedKey);
            RadiusPacket authPacket = rc.Authenticate(username, password);
            RadiusPacket receivedPacket = rc.SendAndReceivePacket(authPacket).Result;

            if (receivedPacket == null)
            {
                throw new Exception("Can't connect to radius server");
            }

            switch (receivedPacket.PacketType)
            {
                case RadiusCode.ACCESS_ACCEPT:
                    log.Debug("User {0} successfully authenticated", username);
                    return true;
                case RadiusCode.ACCESS_CHALLENGE:
                    return false;
                default:
                    return false;
            }
        }

        
    }
}