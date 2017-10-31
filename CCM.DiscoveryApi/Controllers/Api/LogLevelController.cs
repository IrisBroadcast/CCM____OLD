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

﻿using System.Web.Http;
using CCM.Core.Managers;
using NLog;

namespace CCM.DiscoveryApi.Controllers.Api
{
    public class LevelModel
    {
        public string LogLevel { get; set; }
    }

    public class LogLevelController : ApiController
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public LevelModel Get()
        {
            var currentLevel = LogLevelManager.GetCurrentLevel();
            log.Debug("Current log level is " + currentLevel.Name);
            return new LevelModel { LogLevel = currentLevel.Name };
        }

        public LevelModel Post(LevelModel levelModel)
        {
            if (levelModel != null)
            {
                var isSet = LogLevelManager.SetLogLevel(levelModel.LogLevel);
                if (isSet)
                {
                    log.Info("Log level changed to {0}", levelModel.LogLevel);
                }
                else
                {
                    log.Info("Log level was NOT changed.");
                }
            }

            return Get();
        }
    }
}
