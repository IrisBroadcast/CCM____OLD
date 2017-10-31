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
using System.Reflection;
using System.Web.Mvc;
using CCM.Core.Entities;
using CCM.Core.Helpers;
using CCM.Core.Interfaces.Managers;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Security;
using CCM.Web.Authentication;
using CCM.Web.Infrastructure;
using CCM.Web.Models.AccountAdmin;
using NLog;

namespace CCM.Web.Controllers
{
    using System.Collections.Generic;

    [CcmAuthorize(Roles = ApplicationConstants.Admin)]
    public class AccountAdminController : BaseController
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly ICcmUserManager _userManager;
        private readonly ICodecTypeRepository _codecTypeRepository;
        private readonly IOwnersRepository _ownersRepository;

        public AccountAdminController(ICcmUserManager userManager, ICodecTypeRepository codecTypeRepository, IOwnersRepository ownersRepository)
        {
            _userManager = userManager;
            _codecTypeRepository = codecTypeRepository;
            _ownersRepository = ownersRepository;
        }

        public ActionResult Index(string search = "")
        {
            var model = new AccountAdminViewModel();

            if (!string.IsNullOrWhiteSpace(search))
            {
                model.Users = SortUsers(_userManager.FindUsers(search));
                model.Filter = "all";
            }
            else
            {
                model.Users = SortUsers(_userManager.GetAllUsers());
                model.Filter = "SIP";
            }

            return View(model);
        }

        [HttpGet]
        //[RequireHttps]
        public ActionResult Create()
        {
            var model = new UserFormViewModel
            {
                Id = Guid.NewGuid().ToString(),
            };

            SetListData(model);

            // Admin-valet bara tillgängligt för lokal admin
            if (!model.CurrentUserIsLocalAdmin)
            {
                var adminRole = model.Roles.FirstOrDefault(r => r.Name == ApplicationConstants.Admin);
                if (adminRole != null)
                {
                    model.Roles.Remove(adminRole);
                }
            }

            return View(model);
        }

        [HttpPost]
        //[RequireHttps]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new CcmUser
                {
                    Comment = model.Comment,
                    DisplayName = model.DisplayName,
                    FirstName = model.UserType != UserType.SIP ? model.FirstName : string.Empty,
                    Id = Guid.NewGuid().ToString(),
                    LastName = model.UserType != UserType.SIP ? model.LastName : string.Empty,
                    UserName = model.UserName.Trim(),
                    UserType = model.UserType,
                    Owner = model.UserType == UserType.SIP ? _ownersRepository.GetById(model.OwnerId) : null,
                    CodecType = model.UserType == UserType.SIP ? _codecTypeRepository.GetById(model.CodecTypeId) : null,
                    CreatedBy = User.Identity.Name,
                    UpdatedBy = User.Identity.Name,
                    RoleId = model.RoleId
                };
                
                try
                {
                    var userIsLocalAdmin = _userManager.UserIsLocalAdmin(User.Identity.Name);
                    _userManager.Create(user, model.Password, userIsLocalAdmin);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    log.Error("Could not create user", ex);
                    if (ex is ApplicationException)
                    {
                        ModelState.AddModelError("CreateUser", ex.Message);
                    }
                    else
                    {
                        ModelState.AddModelError("CreateUser", "Användaren kunde inte skapas");
                    }
                    
                }
            }

            SetListData(model);

            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            CcmUser user = _userManager.GetUser(id);

            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var model = new UserFormViewModel
            {
                AccountLocked = user.AccountLocked,
                Comment = user.Comment,
                DisplayName = user.DisplayName,
                FirstName = user.FirstName,
                Id = user.Id,
                RadiusId = user.RadiusId,
                LastName = user.LastName,
                UserName = user.UserName,
                UserType = user.UserType,
                RoleId = user.RoleId,
                OwnerId = user.Owner != null ? user.Owner.Id : Guid.Empty,
                CodecTypeId = user.CodecType != null ? user.CodecType.Id : Guid.Empty,
            };

            SetListData(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[RequireHttps]
        public ActionResult Edit(UserFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new CcmUser();
                user.Comment = model.Comment;
                user.DisplayName = model.DisplayName;
                user.FirstName = model.UserType != UserType.SIP ? model.FirstName : string.Empty;
                user.Id = model.Id;
                user.LastName = model.UserType != UserType.SIP ? model.LastName : string.Empty;
                user.UserName = model.UserName.Trim();
                user.UserType = model.UserType;
                user.RadiusId = model.RadiusId;
                user.Owner = model.UserType == UserType.SIP ? _ownersRepository.GetById(model.OwnerId) : null;
                user.RoleId = model.RoleId;
                user.CodecType = model.UserType == UserType.SIP ? _codecTypeRepository.GetById(model.CodecTypeId) : null;
                user.UpdatedBy = User.Identity.Name;

                try
                {
                    if (!string.IsNullOrWhiteSpace(model.Password) && user.RadiusId > 0)
                    {
                        _userManager.UpdatePassword(user.RadiusId, model.Password, user.UserType);
                    }

                    var currentUserLocalAdmin = _userManager.UserIsLocalAdmin(User.Identity.Name);
                    _userManager.Update(user, currentUserLocalAdmin);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("UpdatePassword", ex.Message);
                }
            }

            SetListData(model);

            return View(model);
        }

        [HttpGet]
        public ActionResult Delete(Guid id)
        {
            CcmUser user = _userManager.GetUser(id.ToString());

            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var model = new DeleteUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(DeleteUserViewModel model)
        {
            CcmUser user = _userManager.GetUser(model.Id);

            if (user != null)
            {
                _userManager.Delete(user);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult RadiusImport()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RadiusImport(bool agree)
        {
            if (agree)
            {
                ViewBag.importedCount = _userManager.ImportUsers();
            }
            else
            {
                ModelState.AddModelError(string.Empty, Resources.User_Import_Radius_Agree_Needed);
            }

            return View();
        }

        public ActionResult ToggleAccountLock(Guid id)
        {
            if (id != Guid.Empty)
            {
                _userManager.ToggleUserLock(id);
                return RedirectToAction("Edit", new { @id = id });
            }

            return RedirectToAction("Index");
        }

        private List<CcmUser> SortUsers(List<CcmUser> ccmUsers)
        {
            var users = ccmUsers.Where(u => u.UserType == UserType.CcmUser).ToList();

            var sipAccounts = ccmUsers.Where(u => u.UserType == UserType.SIP).GroupBy(u => u.CodecType == null ? string.Empty : u.CodecType.Name);
            foreach (var userGroup in sipAccounts.OrderBy(grouping => grouping.Key))
            {
                users.AddRange(userGroup);
            }

            return users;
        }

        private void SetListData(UserFormViewModel model)
        {
            model.Owners = _ownersRepository.GetAll();
            model.Owners.Insert(0, new Owner { Id = Guid.Empty, Name = string.Empty });

            model.CodecTypes = _codecTypeRepository.GetAll();
            model.CodecTypes.Insert(0, new CodecType() { Id = Guid.Empty, Name = string.Empty });

            model.CurrentUserIsLocalAdmin = _userManager.UserIsLocalAdmin(User.Identity.Name);

            model.Roles = _userManager.GetRoles();
            model.Roles.Insert(0, new CcmRole() { Name = string.Empty, Id = string.Empty });
        }

    }
}