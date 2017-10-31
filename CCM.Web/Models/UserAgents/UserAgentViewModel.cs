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
using System.ComponentModel.DataAnnotations;
using CCM.Core.Enums;

namespace CCM.Web.Models.UserAgents
{
    public class UserAgentViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Name_Required")]
        [Display(ResourceType = typeof(Resources), Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Name_Required")]
        [Display(ResourceType = typeof(Resources), Name = "Identifier")]
        public string Identifier { get; set; } // Match identifier

        [Display(ResourceType = typeof(Resources), Name = "Image")]
        public string Image { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "User_Interface", Description = "User_Interface_Description")]
        public string UserInterfaceLink { get; set; }

        public bool ActiveX { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Width")]
        public int Width { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Height")]
        public int Height { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "IdentifyType")]
        public MatchType MatchType { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Profiles")]
        public List<ProfileListItemViewModel> Profiles { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Comment")]
        public string Comment { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Api")]
        public string Api { get; set; }

        public Dictionary<string, string> CodecApis { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Lines")]
        public int Lines { get; set; }

        public int LinesInList { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Inputs")]
        public int Inputs { get; set; }

        public int InputsInList { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Min")]
        public int MinInputDb { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Max")]
        public int MaxInputDb { get; set; }

        public int InputDbInListMin { get; set; }
        public int InputDbInListMax { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Step")]
        public int InputGainStep { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Gpo_Names")]
        public string GpoNames { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Open_User_Interface", Description = "Open_User_Interface_Description")]
        public bool UserInterfaceIsOpen { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Use_Scrollbars")]
        public bool UseScrollbars { get; set; }

        public List<CodecPresetListItemViewModel> CodecPresets { get; set; }
    }
}