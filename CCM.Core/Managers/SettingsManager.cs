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
using System.Linq;
using System.Reflection;
using CCM.Core.Entities;
using CCM.Core.Enums;
using CCM.Core.Interfaces.Managers;
using CCM.Core.Interfaces.Repositories;
using NLog;

namespace CCM.Core.Managers
{
    public class SettingsManager : ISettingsManager
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly ISettingsRepository _settingsRepository;

        public SettingsManager(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public string SipDomain { get { return GetSetting(SettingsEnum.SIPDomain); } }
        public int LatestCallCount { get { return GetSetting<int>(SettingsEnum.LatestCallCount); } }
        public int MaxRegistrationAge { get { return GetSetting<int>(SettingsEnum.MaxRegistrationAge); } }
        public bool CodecControlActive { get { return GetSetting<bool>(SettingsEnum.CodecControlActive); } }

        public List<Setting> GetSettings()
        {
            return _settingsRepository.GetAll();
        }

        public void SaveSettings(List<Setting> newSettings, string userName)
        {
            _settingsRepository.Save(newSettings, userName);
        }

        private string GetSetting(SettingsEnum enumName)
        {
            var setting = GetSettings().SingleOrDefault(s => s.Name == enumName.ToString());
            return setting == null ? string.Empty : setting.Value;
        }

        private T GetSetting<T>(SettingsEnum enumName)
        {
            try
            {
                var value = GetSetting(enumName);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Exception when reading setting {0} of type {1}", enumName, typeof(T)), ex);
                return default(T);
            }
        }
       
    }
}