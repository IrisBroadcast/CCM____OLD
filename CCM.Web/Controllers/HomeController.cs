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
using System.Linq;
using System.Web.Mvc;
using CCM.Core.Entities.Specific;
using CCM.Core.Interfaces.Managers;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Kamailio;
using CCM.Web.Authentication;
using CCM.Web.Infrastructure;
using CCM.Web.Infrastructure.SignalR;
using CCM.Web.Models.Home;

namespace CCM.Web.Controllers
{
    public class HomeController : BaseController
    {
        #region Constructor and members

        private readonly ICodecTypeRepository _codecTypeRepository;
        private readonly IRegionRepository _regionRepository;
        private readonly IRegisteredSipRepository _registeredSipRepository;
        private readonly ICcmUserManager _userManager;
        private readonly IGuiHubUpdater _guiHubUpdater;
        private readonly IStatusHubUpdater _statusHubUpdater;

        public HomeController(IRegionRepository regionRepository, ICodecTypeRepository codecTypeRepository, IRegisteredSipRepository registeredSipRepository,
            ICcmUserManager userManager, IGuiHubUpdater guiHubUpdater, IStatusHubUpdater statusHubUpdater)
        {
            _regionRepository = regionRepository;
            _codecTypeRepository = codecTypeRepository;
            _registeredSipRepository = registeredSipRepository;
            _userManager = userManager;
            _guiHubUpdater = guiHubUpdater;
            _statusHubUpdater = statusHubUpdater;
        }
        #endregion

        public ActionResult Index()
        { 
            var vm = new HomeViewModel
            {
                CodecTypes = _codecTypeRepository.GetAll().Select(codecType1 => new CodecTypeViewModel() { Name = codecType1.Name, Color = codecType1.Color }),
                Regions = _regionRepository.GetAll().Select(r => r.Name),
            };

            return View(vm);
        }

        [CcmAuthorize(Roles = "Admin, Remote")]
        [HttpGet]
        public ActionResult EditRegisteredSipComment(Guid id)
        {
            var sip = _registeredSipRepository.GetCachedRegisteredSips().FirstOrDefault(rs => rs.Id == id);

            if (sip == null)
            {
                return null;
            }

            return PartialView("_SipCommentForm", new RegisteredSipComment() { Comment = sip.Comment, RegisteredSipId = sip.Id, RegisteredSipUserName = sip.UserName });
        }

        [ValidateAntiForgeryToken]
        [CcmAuthorize(Roles = "Admin, Remote")]
        [HttpPost]
        public ActionResult EditRegisteredSipComment(RegisteredSipComment sipComment)
        {
            if (sipComment.RegisteredSipId != Guid.Empty)
            {
                _userManager.SaveComment(sipComment);
            }

            var updateResult = new KamailioMessageHandlerResult()
            {
                ChangeStatus = KamailioMessageChangeStatus.CodecUpdated,
                ChangedObjectId = sipComment.RegisteredSipId
            };

            _guiHubUpdater.Update(updateResult);
            _statusHubUpdater.Update(updateResult);

            return null;
        }

    }
}