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
using NLog;
using NLog.Config;

namespace CCM.Core.Managers
{
    public class LogLevelManager
    {
        public static LogLevel GetCurrentLevel()
        {
            LoggingRule rule = LogManager.Configuration.LoggingRules.FirstOrDefault();
            var minLevel = rule != null ? rule.Levels.Min() ?? LogLevel.Off : LogLevel.Off;
            return minLevel;
        }

        public static bool SetLogLevel(string logLevel)
        {
            if (string.IsNullOrEmpty(logLevel))
            {
                return false;
            }

            LogLevel level = LogLevel.FromString(logLevel);

            if (level == null)
            {
                return false;
            }

            foreach (var rule in LogManager.Configuration.LoggingRules)
            {
                foreach (var l in LogLevel.AllLevels)
                {
                    if (level > l)
                        rule.DisableLoggingForLevel(l);
                    else
                        rule.EnableLoggingForLevel(l);
                }
            }

            LogManager.ReconfigExistingLoggers();
            return true;
        }
    }
}
