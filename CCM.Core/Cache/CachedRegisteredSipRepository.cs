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
using CCM.Core.Entities;
using CCM.Core.Entities.Specific;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Kamailio;
using LazyCache;
using NLog;
using System.Linq;

namespace CCM.Core.Cache
{
    public class CachedRegisteredSipRepository : IRegisteredSipRepository
    {
        #region Members and constructor
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IRegisteredSipRepository _internalRepository;
        private readonly IAppCache _lazyCache;

        public CachedRegisteredSipRepository(IAppCache cache, IRegisteredSipRepository internalRepository)
        {
            _lazyCache = cache;
            _internalRepository = internalRepository;
        }
        #endregion

        public KamailioMessageHandlerResult UpdateRegisteredSip(RegisteredSip registeredSip)
        {
            var result = _internalRepository.UpdateRegisteredSip(registeredSip);

            // When reregistration of codec already in cache, just update timestamp
            if (result.ChangeStatus == KamailioMessageChangeStatus.NothingChanged && result.ChangedObjectId != Guid.Empty)
            {
                var regSip = GetCachedRegisteredSips().FirstOrDefault(rs => rs.Id == result.ChangedObjectId);
                if (regSip != null)
                {
                    regSip.Updated = DateTime.UtcNow;
                    return result;
                }
            }

            // Otherwise reload cache.
            _lazyCache.ClearRegisteredSips();
            return result;
        }
        
        public List<RegisteredSipDto> GetCachedRegisteredSips()
        {
            return _lazyCache.GetOrAddRegisteredSips(() => _internalRepository.GetCachedRegisteredSips());
        }
        
        public KamailioMessageHandlerResult DeleteRegisteredSip(string sipAddress)
        {
            var result = _internalRepository.DeleteRegisteredSip(sipAddress);

            if (result.ChangeStatus == KamailioMessageChangeStatus.CodecRemoved)
            {
                _lazyCache.ClearRegisteredSips();
            }

            return result;
        }
    }
}