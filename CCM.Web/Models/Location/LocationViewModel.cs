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
using System.Net;
using System.Net.Sockets;

namespace CCM.Web.Models.Location
{
    public class LocationViewModel : IValidatableObject
    {
        public Guid Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "Name_Required")]
        [Display(ResourceType = typeof(Resources), Name = "Name")]
        public string Name { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Net")]
        public string Net { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Cidr")]
        public byte? Cidr { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "NetV6")]
        public string NetV6 { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "CidrV6")]
        public byte? CidrV6 { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Carrier_Connection_Id")]
        public string CarrierConnectionId { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "ProfileGroup")]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ProfileGroup_Required")]
        public Guid? ProfileGroup { get; set; }

        public List<ListItemViewModel> ProfileGroups { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Region")]
        public Guid Region { get; set; }

        public List<ListItemViewModel> Regions { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "City")]
        public Guid City { get; set; }

        public List<ListItemViewModel> Cities { get; set; }

        [MaxLength(8)]
        [Display(ResourceType = typeof(Resources), Name = "Location_Short_Name")]
        public string ShortName { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Comment")]
        public string Comment { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(Net))
            {
                if (Cidr == null)
                {
                    yield return new ValidationResult("CIDR för IPv4 saknas", new[] { "Cidr" });
                }
                else
                {
                    IPAddress ipAddress;
                    if (IPAddress.TryParse(Net, out ipAddress))
                    {
                        if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
                        {
                            yield return new ValidationResult("Endast IP v4-adresser kan anges", new[] { "Net" });
                        }
                    }
                    else
                    {
                        yield return new ValidationResult("Ogiltig IPv4-adress", new[] { "Net" });
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(NetV6))
            {
                if (CidrV6 == null)
                {
                    yield return new ValidationResult("CIDR för IPv6 saknas", new[] { "CidrV6" });
                }
                else
                {
                    IPAddress ipAddress;
                    if (IPAddress.TryParse(NetV6, out ipAddress))
                    {
                        if (ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            yield return new ValidationResult("Endast IPv6-adresser kan anges", new[] { "NetV6" });
                        }
                    }
                    else
                    {
                        yield return new ValidationResult("Ogiltig IPv6-adress", new[] { "NetV6" });
                    }
                }
            }

        }
    }
}