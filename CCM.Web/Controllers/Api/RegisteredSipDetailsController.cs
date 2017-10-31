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
using CCM.Core.Entities;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Managers;
using CCM.Web.Models.Home;
using System.Web.Http;
using CCM.Core.Entities.Specific;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Interfaces.Repositories.Specialized;

namespace CCM.Web.Controllers.Api
{
    public class RegisteredSipDetailsController : ApiController
    {
        #region Constructor and members

        private readonly ISettingsManager _settingsManager;
        private readonly ICallRepository _callRepository;
        private readonly IRegisteredSipDetailsRepository _registeredSipDetailsRepository;

        public RegisteredSipDetailsController(ISettingsManager settingsManager, ICallRepository callRepository, IRegisteredSipDetailsRepository registeredSipRepository)
        {
            _settingsManager = settingsManager;
            _callRepository = callRepository;
            _registeredSipDetailsRepository = registeredSipRepository;
        }
        #endregion

        public IHttpActionResult GetRegisteredSipInfo(Guid id)
        {
            // Called when the user clicked on a codec in the GUI to show detailed information, including codec control GUI, for the codec.

            RegisteredSipDetails regSipDetails = _registeredSipDetailsRepository.GetRegisteredSipById(id);
            if (regSipDetails == null)
            {
                return NotFound();
            }

            var call = _callRepository.GetCallBySipAddress(regSipDetails.Sip);

            var model = new RegisteredSipInfoViewModel
            {
                IsAuthenticated = User.Identity.IsAuthenticated,

                Id = regSipDetails.Id,
                Sip = regSipDetails.Sip,
                CodecControl = ShouldShowCodecControlView(regSipDetails),
                Comment = regSipDetails.Comment,
                DisplayName = GetDisplayName(regSipDetails),
                Ip = regSipDetails.Ip,
                UserAgentHeader = regSipDetails.UserAgentHeader,
                
                Image = regSipDetails.Image,
                ActiveX = regSipDetails.ActiveX,
                Width = regSipDetails.Width,
                Height = regSipDetails.Height,
                UserInterfaceLink = regSipDetails.UserInterfaceLink,
                UserInterfaceIsOpen = regSipDetails.UserInterfaceIsOpen,
                UseScrollbars = regSipDetails.UseScrollbars,
                Inputs = regSipDetails.Inputs,
                InputMaxDb = regSipDetails.InputMaxDb,
                InputMinDb = regSipDetails.InputMinDb,
                InputGainStep = regSipDetails.InputGainStep,
                Lines = regSipDetails.Lines,
                CodecPresets = regSipDetails.CodecPresets,

                LocationName = regSipDetails.LocationName,
                LocationComment = regSipDetails.LocationComment,
                CityName = regSipDetails.CityName,
                RegionName = regSipDetails.RegionName,
                InCall = call != null,
                InCallWithName = call != null ? GetInCallWith(regSipDetails, call) : string.Empty
            };
            
            return Ok(model);
        }

        private string GetDisplayName(RegisteredSipDetails registeredSip)
        {
            var sipDomain = _settingsManager.SipDomain;

            return DisplayNameHelper.GetDisplayName(
                registeredSip.DisplayName, registeredSip.UserDisplayName, string.Empty,
                registeredSip.Sip, string.Empty, string.Empty, sipDomain);
        }

        private bool ShouldShowCodecControlView(RegisteredSipDetails registeredSip)
        {
            return (User.IsInRole(ApplicationConstants.Admin) || User.IsInRole(ApplicationConstants.Remote)) 
                && !string.IsNullOrWhiteSpace(registeredSip.Api) 
                && _settingsManager.CodecControlActive;
        }

        private string GetInCallWith(RegisteredSipDetails registeredSip, Call call)
        {
            var sipDomain = _settingsManager.SipDomain;

            string inCallWith;

            if (call.FromSip == registeredSip.Sip)
            {
                inCallWith = call.To != null ? DisplayNameHelper.GetDisplayName(call.To, sipDomain) : call.ToSip;
            }
            else
            {
                inCallWith = call.From != null ? DisplayNameHelper.GetDisplayName(call.From, sipDomain) : call.FromSip;
            }

            return DisplayNameHelper.AnonymizeDisplayName(inCallWith);
        }
    }
}