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
using System.Web.Http;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Managers;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Security;
using CCM.Web.Authentication;
using CCM.Web.Models.ApiExternal;
using CCM.WebCommon.Authentication;
using CCM.WebCommon.Infrastructure.WebApi;
using NLog;

namespace CCM.Web.Controllers.ApiExternal
{

    [BasicAuthentication]
    [CcmApiAuthorize(Roles = "Admin,AccountManager")]
    [RoutePrefix("api/account")]
    public class AccountController : ApiController
    {
        private readonly ICcmUserManager _userManager;
        private readonly IOwnersRepository _ownersRepository;
        private readonly ICodecTypeRepository _codecTypeRepository;
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public AccountController(ICcmUserManager userManager, IOwnersRepository ownersRepository, ICodecTypeRepository codecTypeRepository)
        {
            _userManager = userManager;
            _ownersRepository = ownersRepository;
            _codecTypeRepository = codecTypeRepository;
        }

        [HttpGet]
        [Route("get")]
        public IHttpActionResult Get(string username)
        {
            CcmUser user = _userManager.FindByUserName(username);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserModel()
            {
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Comment = user.Comment
            });
        }

        [ValidateModel]
        [HttpPost]
        [Route("add")]
        public IHttpActionResult Add(AddUserModel model)
        {
            log.Debug("Call to ExternalAccountController.AddUser");

            if (model == null)
            {
                return BadRequest("Parameters missing.");
            }

            var user = new CcmUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = model.UserName.Trim(),
                DisplayName = model.DisplayName,
                Comment = model.Comment,
                UserType = UserType.SIP,
                Owner = _ownersRepository.GetByName(ApplicationConstants.SrOwnerName),
                CodecType = _codecTypeRepository.Find(ApplicationConstants.PersonligaCodecTypeName).FirstOrDefault(),
                CreatedBy = User.Identity.Name,
                UpdatedBy = User.Identity.Name
            };

            try
            {
                var existingUser = _userManager.FindByUserName(user.UserName);
                if (existingUser != null)
                {
                    return Conflict();
                }

                _userManager.Create(user, model.Password);
                return Created(Url.Content("get?username=" + user.UserName), "User created");
            }
            catch (Exception ex)
            {
                log.Error("Could not create user", ex);
                return InternalServerError();
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("update")]
        public IHttpActionResult Update(UserModel model)
        {
            log.Debug("Call to ExternalAccountController.EditUser");

            if (model == null)
            {
                return BadRequest("Parameters missing.");
            }

            var user = _userManager.FindByUserName(model.UserName);

            if (user == null)
            {
                return NotFound();
            }

            // Sätt nya värden på uppdateringsbara egenskaper
            user.DisplayName = model.DisplayName;
            user.Comment = model.Comment;

            try
            {
                _userManager.Update(user);
                return Ok("User updated");
            }
            catch (Exception ex)
            {
                log.Error("Could not update user", ex);
                return InternalServerError(new ApplicationException("User could not be updated."));
            }

        }

        [HttpPost]
        [ValidateModel]
        [Route("updatepassword")]
        public IHttpActionResult UpdatePassword(ChangePasswordModel model)
        {
            log.Debug("Call to ExternalAccountController.UpdatePassword");

            if (model == null)
            {
                return BadRequest("Parameters missing.");
            }
            
            var user = _userManager.FindByUserName(model.UserName);

            if (user == null)
            {
                return NotFound();
            }
            
            try
            {
                _userManager.UpdatePassword(user.RadiusId, model.NewPassword, user.UserType);
                return Ok("Password updated");
            }
            catch (Exception ex)
            {
                log.Error("Could not update password", ex);
                return InternalServerError(new ApplicationException("Password could not be updated."));
            }
        }

        [ValidateModel]
        [HttpDelete]
        [Route("delete")]
        public IHttpActionResult Delete(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return BadRequest("User name missing");
            }

            CcmUser user = _userManager.FindByUserName(userName);

            if (user == null)
            {
                return NotFound();
            }

            try
            {
                if (!_userManager.Delete(user))
                {
                    return InternalServerError(new ApplicationException("User not deleted"));
                }
                
                return Ok("User deleted");
            }
            catch (Exception ex)
            {
                log.Error("Could not delete user", ex);
                return InternalServerError(new ApplicationException("User could not be deleted. " + ex.Message));
            }
        }
       
    }
}
