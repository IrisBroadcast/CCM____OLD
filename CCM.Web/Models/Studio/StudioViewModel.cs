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
using System.ComponentModel.DataAnnotations;

namespace CCM.Web.Models.Studio
{
    public class StudioViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(40, ErrorMessage = "Namnet är för långt")]
        [Display(Name = "Studionamn")]
        public string Name { get; set; }

        [Display(Name="Ljudkodarens SIP-adress")]
        public string CodecSipAddress { get; set; }

        [Display(Name = "Kamerans adress eller IP-nummer")]
        public string CameraAddress { get; set; }

        [Display(Name = "Kamera aktiv", Description = "")]
        public bool CameraActive { get; set; }

        [Display(Name = "Användarnamn", Description = "")]
        public string CameraUsername { get; set; }

        [Display(Name = "Lösenord", Description = "")]
        public string CameraPassword { get; set; }
        [Display(Name="Sökväg video", Description = "")]
        public string CameraVideoUrl { get; set; }

        [Display(Name = "Sökväg stillbild", Description = "")]
        public string CameraImageUrl { get; set; }
        
        [Display(Name = "Sökväg ljuduppspelning")]
        public string CameraPlayAudioUrl { get; set; }

        [Display(Name = "Ljudklippens namn")]
        public string AudioClipNames { get; set; }

        [Display(Name = "Informationstext")]
        public string InfoText { get; set; }

        [Display(Name = "Sökväg till 'manual'-sidan")]
        public string MoreInfoUrl { get; set; }

        [Display(Name = "Antal ingångar")]
        public int NrOfAudioInputs { get; set; }

        [Display(Name = "Ingångarnas namn")]
        public string AudioInputNames { get; set; }

        [Display(Name = "Förvald ingångsnivå")]
        public string AudioInputDefaultGain { get; set; }

        [Display(Name = "Antal GPOs")]
        public int NrOfGpos { get; set; }

        [Display(Name = "GPO Namnlista")]
        public string GpoNames { get; set; }

        [Required]
        [Range(0, 60)]
        [Display(Name = "Antal minuter innan monitorsidan blir inaktiv")]
        public int InactivityTimeout { get; set; }
    }
}