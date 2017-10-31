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
using CCM.CodecControl.Mandozzi.Umac;
using CCM.CodecControl.Prodys.IkusNet;
using CCM.Core.CodecControl.Entities;
using CCM.Core.CodecControl.Enums;
using CCM.Core.CodecControl.Interfaces;
using CCM.Core.Exceptions;
using CCM.Core.Interfaces.Managers;
using NLog;

namespace CCM.CodecControl
{
    /// <summary>
    /// Manager for connecting with Code APIs
    /// </summary>
    public class CodecManager : ICodecManager
    {
        private readonly ISettingsManager _settingsManager;
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public CodecManager(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public static Dictionary<string, CodecApiInformation> AvailableApis
        {
            get
            {
                var apis = new List<CodecApiInformation>
                {
                    new CodecApiInformation { DisplayName = "Prodys IkusNet", Name = "IkusNet" },
                    new CodecApiInformation { DisplayName = "Mandozzi Umac", Name = "Umac" }
                };
                return apis.ToDictionary(c => c.Name);
            }
        }

        private ICodecApi CreateCodecApi(CodecInformation codecInformation)
        {
            // Pga en bugg in Prodys Quantum, där den kan hänga sig då man skickar kommandon, 
            // införs möjligheten att helt kunna stänga av kodarstyrningsfunktionen.
            var codecControlActive = _settingsManager.CodecControlActive;

            if (!codecControlActive)
            {
                throw new CodecApiNotFoundException("Codec control is disabled.");
            }

            if (codecInformation == null)
            {
                throw new CodecApiNotFoundException("Missing codec api information.");
            }

            switch (codecInformation.Api)
            {
                case "IkusNet":
                    return new IkusNetApi();
                case "Umac":
                    return new UmacApi();
                default:
                    throw new CodecApiNotFoundException(string.Format("Could not load API {0}.", codecInformation.Api));
            }
        }


        public bool Call(CodecInformation codecInformation, string callee, string profileName)
        {
            var codecApi = CreateCodecApi(codecInformation);
            
            // TODO: first check codec call status. Do not execute the call method if the codec is already in a call.
            // Some codecs will hangup the current call and dial up the new call without hesitation.

            var call = new Call()
            {
                Address = callee,
                CallType = IpCallType.UnicastBidirectional,
                Codec = Codec.Program,
                Content = CallContent.Audio,
                Profile = profileName
            };

            return codecApi.Call(codecInformation.Ip, call);
        }

        public bool HangUp(CodecInformation codecInformation)
        {
            var codecApi = CreateCodecApi(codecInformation);
            return codecApi.HangUp(codecInformation.Ip, Codec.Program);
        }

        public bool CheckIfAvailable(CodecInformation codecInformation)
        {
            try
            {
                var codecApi = CreateCodecApi(codecInformation);
                return codecApi.CheckIfAvailable(codecInformation.Ip);
            }
            catch (Exception ex)
            {
                log.Warn("Exception in CheckIfAvailable", ex);
                return false;
            }
        }

        public bool? GetGpo(CodecInformation codecInformation, int gpio)
        {
            try
            {
                var codecApi = CreateCodecApi(codecInformation);
                var gpo = codecApi.GetGpo(codecInformation.Ip, gpio);
                return gpo;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool GetInputEnabled(CodecInformation codecInformation, int input)
        {
            var codecApi = CreateCodecApi(codecInformation);
            return codecApi.GetInputEnabled(codecInformation.Ip, input);
        }

        public int GetInputGainLevel(CodecInformation codecInformation, int input)
        {
            var codecApi = CreateCodecApi(codecInformation);
            return codecApi.GetInputGainLevel(codecInformation.Ip, input);
        }

        public LineStatus GetLineStatus(CodecInformation codecInformation, int line)
        {
            var codecApi = CreateCodecApi(codecInformation);
            return codecApi.GetLineStatus(codecInformation.Ip, line);
        }

        public string GetLoadedPresetName(CodecInformation codecInformation, string lastPresetName)
        {
            var codecApi = CreateCodecApi(codecInformation);
            return codecApi.GetLoadedPresetName(codecInformation.Ip, lastPresetName);
        }

        public VuValues GetVuValues(CodecInformation codecInformation)
        {
            var codecApi = CreateCodecApi(codecInformation);
            return codecApi.GetVuValues(codecInformation.Ip);
        }

        public AudioStatus GetAudioStatus(CodecInformation codecInformation, int nrOfInputs, int nrOfGpos)
        {
            var codecApi = CreateCodecApi(codecInformation);
            return codecApi.GetAudioStatus(codecInformation.Ip, nrOfInputs, nrOfGpos);
        }

        public AudioMode GetAudioMode(CodecInformation codecInformation)
        {
            var codecApi = CreateCodecApi(codecInformation);
            return codecApi.GetAudioMode(codecInformation.Ip);
        }

        public bool LoadPreset(CodecInformation codecInformation, string preset)
        {
            var codecApi = CreateCodecApi(codecInformation);
            return codecApi.LoadPreset(codecInformation.Ip, preset);
        }

        public bool Reboot(CodecInformation codecInformation)
        {
            var codecApi = CreateCodecApi(codecInformation);
            return codecApi.Reboot(codecInformation.Ip);
        }

        public bool SetGpo(CodecInformation codecInformation, int gpo, bool active)
        {
            var codecApi = CreateCodecApi(codecInformation);
            return codecApi.SetGpo(codecInformation.Ip, gpo, active);
        }

        public bool SetInputEnabled(CodecInformation codecInformation, int input, bool enabled)
        {
            var codecApi = CreateCodecApi(codecInformation);
            return codecApi.SetInputEnabled(codecInformation.Ip, input, enabled);
        }

        public int SetInputGainLevel(CodecInformation codecInformation, int input, int gainLevel)
        {
            var codecApi = CreateCodecApi(codecInformation);
            codecApi.SetInputGainLevel(codecInformation.Ip, input, gainLevel);
            return codecApi.GetInputGainLevel(codecInformation.Ip, input);
        }

    }
}