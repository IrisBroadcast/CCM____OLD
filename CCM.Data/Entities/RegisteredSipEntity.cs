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
using System.ComponentModel.DataAnnotations.Schema;
using CCM.Core.Attributes;

namespace CCM.Data.Entities
{
    [Table("RegisteredSips")]
    public class RegisteredSipEntity
    {
        [Key]
        public Guid Id { get; set; }

        [MetaProperty]
        public string SIP { get; set; }

        [MetaProperty]
        public string UserAgentHead { get; set; }

        [MetaProperty]
        public string Username { get; set; }

        [MetaProperty]
        public string DisplayName { get; set; }

        [MetaProperty]
        public string IP { get; set; }

        [MetaProperty]
        public int Port { get; set; }

        public long ServerTimeStamp { get; set; }
        public DateTime Updated { get; set; }
        public int Expires { get; set; }
        public Guid? Location_LocationId { get; set; }

        [MetaType]
        [ForeignKey("Location_LocationId")]
        public virtual LocationEntity Location { get; set; }

        public Guid? User_UserId { get; set; }

        [MetaType]
        [ForeignKey("User_UserId")]
        public virtual UserEntity User { get; set; }

        public Guid? UserAgentId { get; set; }

        [MetaType]
        [ForeignKey("UserAgentId")]
        public virtual UserAgentEntity UserAgent { get; set; }
    }
}
