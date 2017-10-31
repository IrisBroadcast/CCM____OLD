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
using System.Net;
using System.Web.Http;
using CCM.Core.CodecControl.Entities;
using CCM.Core.CodecControl.Interfaces;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Interfaces.Repositories.Specialized;
using CCM.Web.Mappers;
using CCM.Web.Models.ApiExternal;
using CCM.Web.Models.CodecControl;
using NLog;

namespace CCM.Web.Controllers.Api
{
    [StudioMonitorExceptionFilter]
    public class StudioMonitorApiController : ApiController
    {
        #region Constructor and members
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IStudioRepository _studioRepository;
        private readonly ICodecInformationRepository _codecInformationRepository;
        private readonly ICodecManager _codecManager;
        private readonly IRegisteredSipRepository _registeredSipRepository;

        public StudioMonitorApiController(IStudioRepository studioRepository, ICodecInformationRepository codecInformationRepository, ICodecManager codecManager,
            IRegisteredSipRepository registeredSipRepository)
        {
            _studioRepository = studioRepository;
            _codecInformationRepository = codecInformationRepository;
            _codecManager = codecManager;
            _registeredSipRepository = registeredSipRepository;
        }
        #endregion 

        [ActionName("IsCodecAvailable")]
        [HttpGet]
        public bool IsCodecAvailable(Guid studioId)
        {
            var studio = _studioRepository.GetById(studioId);
            var codecInformation = GetCodecInformationBySipAddress(studio.CodecSipAddress);
            var available = _codecManager.CheckIfAvailable(codecInformation);
            return available;
        }

        [ActionName("SetGpo")]
        [HttpPost]
        public StudioGpoViewModel SetGpo(SetGpoModel model)
        {
            var studio = _studioRepository.GetById(model.StudioId);
            var codecInformation = GetCodecInformationBySipAddress(studio.CodecSipAddress);

            _codecManager.SetGpo(codecInformation, model.Number, model.Active);
            var isActive = _codecManager.GetGpo(codecInformation, model.Number) ?? false;

            return new StudioGpoViewModel { Active = isActive };
        }

        [ActionName("GetVuValues")]
        [HttpGet]
        public VuValuesViewModel GetVuValues(Guid studioId)
        {
            try
            {
                var studio = _studioRepository.GetById(studioId);
                var codecInformation = GetCodecInformationBySipAddress(studio.CodecSipAddress);

                var vuValues = _codecManager.GetVuValues(codecInformation);

                var model = new VuValuesViewModel
                {
                    RxLeft = vuValues.RxLeft,
                    RxRight = vuValues.RxRight,
                    TxLeft = vuValues.TxLeft,
                    TxRight = vuValues.TxRight
                };

                return model;
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when getting VU values for studio with id {0}", studioId);
                return null;
            }
        }

        [ActionName("GetAudioStatus")]
        [HttpGet]
        public AudioStatusViewModel GetAudioStatus([FromUri]Guid studioId)
        {
            var studio = _studioRepository.GetById(studioId);
            var codecInformation = GetCodecInformationBySipAddress(studio.CodecSipAddress);
            var audioStatus = _codecManager.GetAudioStatus(codecInformation, studio.NrOfAudioInputs, studio.NrOfGpos);

            var model = new AudioStatusViewModel();

            foreach (InputStatus status in audioStatus.InputStatuses)
            {
                model.InputStatuses.Add(new InputGainAndStatus { Enabled = status.Enabled, Level = status.GainLevel });
            }

            foreach (bool gpoActive in audioStatus.Gpos)
            {
                model.GpoValues.Add(new StudioGpoViewModel { Active = gpoActive });
            }

            model.VuValues = new VuValuesViewModel
            {
                RxLeft = audioStatus.VuValues.RxLeft,
                RxRight = audioStatus.VuValues.RxRight,
                TxLeft = audioStatus.VuValues.TxLeft,
                TxRight = audioStatus.VuValues.TxRight
            };

            return model;
        }

        [ActionName("SetInputGainLevel")]
        [HttpPost]
        public IHttpActionResult SetInputGainLevel(SetLevelModel model)
        {
            try
            {
                var studio = _studioRepository.GetById(model.StudioId);
                var codecInformation = GetCodecInformationBySipAddress(studio.CodecSipAddress);

                var newLevel = _codecManager.SetInputGainLevel(codecInformation, model.Input, model.Level);

                return Ok(new SetInputLevelResponse { Input = model.Input, Level = newLevel });
            }
            catch (Exception ex)
            {
                log.Error(ex, "Exception in SetInputGainLevel");
                return InternalServerError();
            }
        }

        [ActionName("SetInputEnabled")]
        [HttpPost]
        public IHttpActionResult SetInputEnabled(SetInputEnabledModel model)
        {
            var studio = _studioRepository.GetById(model.StudioId);
            var codecInformation = GetCodecInformationBySipAddress(studio.CodecSipAddress);

            var success = _codecManager.SetInputEnabled(codecInformation, model.Input, model.Enabled);

            if (!success)
            {
                return StatusCode(HttpStatusCode.BadRequest);
            }

            bool enabled = _codecManager.GetInputEnabled(codecInformation, model.Input);
            return Ok(new SetInputEnabledResponse { Input = model.Input, Enabled = enabled });
        }

        [HttpGet]
        public CodecStatus GetCodecStatus(Guid studioId)
        {
            var studio = _studioRepository.GetById(studioId);

            if (studio == null || studio.CodecSipAddress == null)
            {
                return null;
            }

            var allRegisteredSips = _registeredSipRepository.GetCachedRegisteredSips();
            var regSip = allRegisteredSips.FirstOrDefault(s => s.Sip == studio.CodecSipAddress);

            if (regSip == null)
            {
                return new CodecStatus() { SipAddress = studio.CodecSipAddress, State = CodecState.NotRegistered };
            }

            return CodecStatusMapper.MapToCodecStatus(regSip);
        }

        private CodecInformation GetCodecInformationBySipAddress(string sipAddress)
        {
            var codecInfo = _codecInformationRepository.GetCodecInformationBySipAddress(sipAddress);
            return codecInfo == null || string.IsNullOrEmpty(codecInfo.Api) ? null : codecInfo;
        }

    }

    public class AudioStatusViewModel
    {
        public List<InputGainAndStatus> InputStatuses { get; set; }
        public List<StudioGpoViewModel> GpoValues { get; set; }
        public VuValuesViewModel VuValues { get; set; }

        public AudioStatusViewModel()
        {
            InputStatuses = new List<InputGainAndStatus>();
            GpoValues = new List<StudioGpoViewModel>();
        }
    }

    public class StudioGpoViewModel
    {
        public bool Active { get; set; }
    }

    public class InputGainAndStatus
    {
        public bool Enabled { get; set; }
        public int Level { get; set; }
    }

    public class SetGpoModel
    {
        public Guid StudioId { get; set; }
        public int Number { get; set; }
        public bool Active { get; set; }
    }

    public class SetLevelModel
    {
        public Guid StudioId { get; set; }
        public int Input { get; set; }
        public int Level { get; set; }
    }

    public class SetInputEnabledModel
    {
        public Guid StudioId { get; set; }
        public int Input { get; set; }
        public bool Enabled { get; set; }
    }

    public class SetInputEnabledResponse
    {
        public int Input { get; set; }
        public bool Enabled { get; set; }
    }

    public class SetInputLevelResponse
    {
        public int Input { get; set; }
        public int Level { get; set; }
    }
}
