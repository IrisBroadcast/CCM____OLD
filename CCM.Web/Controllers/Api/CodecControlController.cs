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
using System.Web.Http;
using AutoMapper;
using CCM.Core.CodecControl.Entities;
using CCM.Core.CodecControl.Enums;
using CCM.Core.CodecControl.Interfaces;
using CCM.Core.Exceptions;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Repositories.Specialized;
using CCM.Web.Authentication;
using CCM.Web.Models.CodecControl;
using CCM.Web.Models.CodecControl.Base;
using NLog;

namespace CCM.Web.Controllers.Api
{
    public class CodecControlController : ApiController
    {
        #region Constructor and members
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly ICodecManager _codecManager;
        private readonly ICodecInformationRepository _codecInformationRepository;

        public CodecControlController(ICodecManager codecManager, ICodecInformationRepository simpleRegisteredSipRepository)
        {
            _codecManager = codecManager;
            _codecInformationRepository = simpleRegisteredSipRepository;
        }
        #endregion

        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("CheckCodecAvailable")]
        [HttpGet]
        public bool CheckCodecAvailable(Guid id)
        {
            var codecInformation = GetCodecInformationById(id);
            return _codecManager.CheckIfAvailable(codecInformation);
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("GetAvailableGpos")]
        [HttpPost]
        public AvailableGposViewModel GetAvailableGpos(dynamic data)
        {
            Guid id = data.id;
            int nrOfGpos = data.nrOfGpos ?? 10;

            var model = new AvailableGposViewModel { Gpos = new List<GpoViewModel>() };

            if (id == Guid.Empty)
            {
                model.Error = "Missing parameter";
                return model;
            }

            CodecInformation codecInformation = GetCodecInformationById(id);

            if (codecInformation == null)
            {
                model.Error = Resources.No_Codec_Found;
                return model;
            }

            try
            {
                for (int i = 0; i < nrOfGpos; i++)
                {
                    bool? active = _codecManager.GetGpo(codecInformation, i);

                    if (!active.HasValue)
                    {
                        // GPO saknas. Vi antar att vi passerat sista GPO:en.
                        break;
                    }

                    var gpoName = GetGpoName(codecInformation.GpoNames, i);

                    model.Gpos.Add(new GpoViewModel()
                    {
                        Active = active.Value,
                        Name = string.IsNullOrWhiteSpace(gpoName) ? string.Format("GPO {0}", i) : gpoName,
                        Number = i
                    });
                }
            }
            catch (Exception ex)
            {
                if (model.Gpos.Count == 0)
                {
                    model.Error = Resources.No_Gpo_Found;
                }
            }

            return model;
        }

        public string GetGpoName(string gpoNames, int index)
        {
            var names = (gpoNames ?? string.Empty).Split(',');
            return index < names.Length ? names[index].Trim() : string.Empty;
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("GetAudioStatus")]
        [HttpGet]
        public Models.CodecControl.AudioStatusViewModel GetAudioStatus([FromUri]Guid id, [FromUri]int nrOfInputs = 2, [FromUri]int nrOfGpos = 2)
        {
            CodecInformation codecInformation = GetCodecInformationById(id);

            var model = ExecuteCodecCommand(codecInformation, (Action<Models.CodecControl.AudioStatusViewModel, CodecInformation>)((theModel, codecInfo) =>
            {
                var audioStatus = _codecManager.GetAudioStatus(codecInfo, nrOfInputs, nrOfGpos);
                Mapper.Map(audioStatus, theModel);
            }));

            return model;
        }


        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("GetInputGainAndStatus")]
        [HttpGet]
        public InputGainAndStatusViewModel GetInputGainAndStatus([FromUri]Guid id, [FromUri]int input)
        {
            CodecInformation codecInformation = GetCodecInformationById(id);

            var model = ExecuteCodecCommand(codecInformation, (Action<InputGainAndStatusViewModel, CodecInformation>)((theModel, codecInfo) =>
           {
               theModel.Enabled = _codecManager.GetInputEnabled(codecInfo, input);
               theModel.GainLevel = _codecManager.GetInputGainLevel(codecInfo, input);
           }));

            return model;
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("GetInputStatus")]
        [HttpGet]
        public InputStatusViewModel GetInputStatus(Guid id, int input)
        {
            CodecInformation codecInformation = GetCodecInformationById(id);

            var model = ExecuteCodecCommand(codecInformation, (Action<InputStatusViewModel, CodecInformation>)((theModel, codecInfo) =>
           {
               theModel.Enabled = _codecManager.GetInputEnabled(codecInfo, input);
           }));

            return model;
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("GetLineStatusBySipAddress")]
        [HttpGet]
        public LineStatusViewModel GetLineStatusBySipAddress(string sipAddress, int line)
        {
            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            return GetLineStatus(line, codecInformation);
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("GetLineStatus")]
        [HttpGet]
        public LineStatusViewModel GetLineStatus(Guid id, int line)
        {
            CodecInformation codecInformation = GetCodecInformationById(id);
            return GetLineStatus(line, codecInformation);
        }

        private LineStatusViewModel GetLineStatus(int line, CodecInformation codecInformation)
        {
            var model = ExecuteCodecCommand(codecInformation,
                (Action<LineStatusViewModel, CodecInformation>)((theModel, codecInfo) =>
               {
                   LineStatus result = _codecManager.GetLineStatus(codecInfo, line);
                   if (result == null || result.StatusCode == LineStatusCode.ErrorGettingStatus)
                   {
                       theModel.Error = Resources.ResourceManager.GetString(LineStatusCode.ErrorGettingStatus.ToString());
                   }
                   else
                   {
                       theModel.Status = Resources.ResourceManager.GetString(string.Format("LineStatus_{0}", result.StatusCode));
                       theModel.DisconnectReason = result.DisconnectReason;
                       theModel.RemoteAddress = result.RemoteAddress;
                       theModel.LineStatus = result.StatusCode;
                   }
               }));

            return model;
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("GetLoadedPreset")]
        [HttpPost]
        public PresetViewModel GetLoadedPreset(dynamic data)
        {
            Guid id = data.id;
            CodecInformation codecInformation = GetCodecInformationById(id);

            var model = ExecuteCodecCommand(codecInformation, (Action<PresetViewModel, CodecInformation>)((theModel, codecInfo) =>
           {
               theModel.LoadedPreset = _codecManager.GetLoadedPresetName(codecInfo, string.Empty);
           }));

            return model;
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("GetVuValues")]
        [HttpGet]
        public VuValuesViewModel GetVuValues(Guid id)
        {
            CodecInformation codecInformation = GetCodecInformationById(id);

            var model = ExecuteCodecCommand(codecInformation, (Action<VuValuesViewModel, CodecInformation>)((theModel, codecInfo) =>
           {
               var result1 = _codecManager.GetVuValues(codecInfo);

               theModel.RxLeft = result1.RxLeft;
               theModel.RxRight = result1.RxRight;
               theModel.TxLeft = result1.TxLeft;
               theModel.TxRight = result1.TxRight;
           }));

            return model;
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("GetAudioMode")]
        [HttpGet]
        public AudioModeViewModel GetAudioMode(Guid id)
        {
            return GetAudioMode(GetCodecInformationById(id));
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("GetAudioMode")]
        [HttpGet]
        public AudioModeViewModel GetAudioMode(string sipAddress)
        {
            return GetAudioMode(GetCodecInformationBySipAddress(sipAddress));
        }

        private AudioModeViewModel GetAudioMode(CodecInformation codecInformation)
        {
            var model = ExecuteCodecCommand(codecInformation,
                (Action<AudioModeViewModel, CodecInformation>)((theModel, codecInfo) =>
               {
                   AudioMode result = _codecManager.GetAudioMode(codecInfo);
                   theModel.EncoderAudioMode = result.EncoderAudioAlgoritm;
                   theModel.DecoderAudioMode = result.DecoderAudioAlgoritm;
               }));

            return model;
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("LoadPreset")]
        [HttpPost]
        public CodecViewModelBase LoadPreset(LoadPresetParameters parameters)
        {
            CodecInformation codecInformation = GetCodecInformationById(parameters.Id);

            var model = ExecuteCodecCommand(codecInformation, (Action<CodecViewModelBase, CodecInformation>)((theModel, codecInfo) =>
           {
               _codecManager.LoadPreset(codecInfo, parameters.Name);
           }));

            return model;
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("SetGpo")]
        [HttpGet]
        public GpoViewModel SetGpo(string sipAddress, int number, bool active)
        {
            return SetGpo(GetCodecInformationBySipAddress(sipAddress), number, active);
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("SetGpo")]
        [HttpGet]
        public GpoViewModel SetGpo(Guid id, int number, bool active)
        {
            return SetGpo(GetCodecInformationById(id), number, active);
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("SetInputEnabled")]
        [HttpPost]
        public InputStatusViewModel SetInputEnabled(SetInputEnabledParameters parameters)
        {
            // Guid id, int input, bool enabled
            CodecInformation codecInformation = GetCodecInformationById(parameters.Id);

            var model = ExecuteCodecCommand(codecInformation, (Action<InputStatusViewModel, CodecInformation>)((theModel, codecInfo) =>
           {
               theModel.Enabled = _codecManager.SetInputEnabled(codecInfo, parameters.Input, parameters.Enabled);
           }));

            return model;
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [System.Web.Mvc.HttpPost]
        [ActionName("SetInputGainLevel")]
        [HttpPost]
        public InputGainLevelViewModel SetInputGainLevel(dynamic data)
        {
            Guid id = data.id;
            int input = data.input;
            int level = data.level;
            CodecInformation codecInformation = GetCodecInformationById(id);

            var model = ExecuteCodecCommand(codecInformation, (Action<InputGainLevelViewModel, CodecInformation>)((theModel, codecInfo) =>
           {
               theModel.GainLevel = _codecManager.SetInputGainLevel(codecInfo, input, level);
           }));

            return model;
        }

        [CcmAuthorize(Roles = ApplicationConstants.Admin)]
        [ActionName("RebootCodec")]
        [HttpPost]
        public bool RebootCodec(RebootCodecParameters parameters)
        {
            var codecInformation = GetCodecInformationById(parameters.Id);

            if (codecInformation == null)
            {
                return false;
            }

            return _codecManager.Reboot(codecInformation);
        }

        [System.Web.Mvc.HttpPost]
        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("Call")]
        [HttpGet]
        public bool Call(Guid id, string callee, string profileName)
        {
            var codecInformation = GetCodecInformationById(id);

            if (codecInformation == null)
            {
                return false;
            }

            return _codecManager.Call(codecInformation, callee, profileName);
        }

        [System.Web.Mvc.HttpPost]
        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("Hangup")]
        [HttpGet]
        public bool Hangup(Guid id)
        {
            var codecInformation = GetCodecInformationById(id);

            if (codecInformation == null)
            {
                return false;
            }

            return _codecManager.HangUp(codecInformation);
        }

        [System.Web.Mvc.HttpPost]
        [CcmAuthorize(Roles = "Admin, Remote")]
        [ActionName("Hangup")]
        [HttpGet]
        public bool Hangup(string sipAddress)
        {
            var codecInformation = GetCodecInformationBySipAddress(sipAddress);

            if (codecInformation == null)
            {
                return false;
            }

            return _codecManager.HangUp(codecInformation);
        }

        private GpoViewModel SetGpo(CodecInformation codecInformation, int number, bool active)
        {
            var model = ExecuteCodecCommand(codecInformation, (Action<GpoViewModel, CodecInformation>)((theModel, codecInfo) =>
            {
                _codecManager.SetGpo(codecInfo, number, active);
                theModel.Number = number;
                theModel.Active = _codecManager.GetGpo(codecInfo, number) ?? false;
            }));

            return model;
        }

        private CodecInformation GetCodecInformationBySipAddress(string sipAddress)
        {
            var codecInfo = _codecInformationRepository.GetCodecInformationBySipAddress(sipAddress);
            return codecInfo == null || string.IsNullOrEmpty(codecInfo.Api) ? null : codecInfo;
        }

        private CodecInformation GetCodecInformationById(Guid id)
        {
            var codecInfo = _codecInformationRepository.GetCodecInformationById(id);
            return codecInfo == null || string.IsNullOrEmpty(codecInfo.Api) ? null : codecInfo;
        }

        private T ExecuteCodecCommand<T>(CodecInformation codecInformation, Action<T, CodecInformation> codecCommandAction) where T : CodecViewModelBase, new()
        {
            var model = new T();

            if (codecInformation == null)
            {
                model.Error = Resources.No_Codec_Found;
                return model;
            }

            try
            {
                codecCommandAction(model, codecInformation);
            }
            catch (CodecApiNotFoundException)
            {
                model.Error = Resources.No_Codec_Found;
            }
            catch (UnableToResolveAddressException)
            {
                model.Error = Resources.Unable_To_Resolve_Address;
            }
            catch (UnableToConnectException)
            {
                model.Error = Resources.Unable_To_Connect_To_Codec;
            }
            catch (Exception ex)
            {
                log.Warn("Exception when sending codec control command to " + codecInformation.SipAddress, ex);
                model.Error = ex.Message;
            }
            return model;
        }
    }
}