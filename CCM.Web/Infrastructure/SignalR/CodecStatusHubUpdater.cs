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

using System;
using System.Linq;
using CCM.Core.Entities;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Kamailio;
using CCM.Web.Hubs;
using CCM.Web.Mappers;
using CCM.Web.Models.ApiExternal;
using NLog;

namespace CCM.Web.Infrastructure.SignalR
{
    public class CodecStatusHubUpdater : IStatusHubUpdater
    {
        private readonly IRegisteredSipRepository _registeredSipRepository;
        private readonly ICallRepository _callRepository;
        private readonly ICallHistoryRepository _callHistoryRepository;
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public CodecStatusHubUpdater(IRegisteredSipRepository registeredSipRepository, ICallRepository callRepository, ICallHistoryRepository callHistoryRepository)
        {
            _registeredSipRepository = registeredSipRepository;
            _callRepository = callRepository;
            _callHistoryRepository = callHistoryRepository;
        }

        public void Update(KamailioMessageHandlerResult updateResult)
        {
            if (updateResult.ChangeStatus == KamailioMessageChangeStatus.CallStarted)
            {
                // Load call and update to and from codecs
                var callId = updateResult.ChangedObjectId;
                var callInfo = _callRepository.GetCallInfoById(callId);

                if (callInfo != null)
                {
                    log.Warn("Call started. From:{0} To:{1}", callInfo.FromId, callInfo.ToId);
                    UpdateCodecStatusByGuid(callInfo.FromId);
                    UpdateCodecStatusByGuid(callInfo.ToId);
                }
                else
                {
                    log.Warn("Call started but was not found in database. Call Id:{0}", callId);
                }
            }

            if (updateResult.ChangeStatus == KamailioMessageChangeStatus.CallClosed)
            {
                // Load call and update to and from codecs
                var callId = updateResult.ChangedObjectId;
                CallHistory call = _callHistoryRepository.GetCallHistoryByCallId(callId);

                if (call != null)
                {
                    log.Info("Call closed. From:{0} To:{1} Call ID:{2}", call.FromId, call.ToId, callId);
                    UpdateCodecStatusByGuid(call.FromId);
                    UpdateCodecStatusByGuid(call.ToId);
                }
                else
                {
                    log.Warn("Call closed but was not found in call history. Call Id:{0}", callId);
                }
            }

            if (updateResult.ChangeStatus == KamailioMessageChangeStatus.CodecAdded ||
                updateResult.ChangeStatus == KamailioMessageChangeStatus.CodecUpdated)
            {
                UpdateCodecStatusByGuid(updateResult.ChangedObjectId);
            }

            if (updateResult.ChangeStatus == KamailioMessageChangeStatus.CodecRemoved)
            {
                var codecStatus = new CodecStatus { State = CodecState.NotRegistered, SipAddress = updateResult.SipAddress };
                CodecStatusHub.UpdateCodecStatus(codecStatus);
            }

            log.Info("StatusHub is updating. status={0}, id={1}", updateResult.ChangeStatus, updateResult.ChangedObjectId);
        }

        private void UpdateCodecStatusByGuid(Guid id)
        {
            // Ignore external codecs
            if (id == Guid.Empty)
            {
                return;
            }

            var rs = _registeredSipRepository.GetCachedRegisteredSips().FirstOrDefault(s => s.Id == id);

            if (rs != null)
            {
                var codecStatus = CodecStatusMapper.MapToCodecStatus(rs);
                CodecStatusHub.UpdateCodecStatus(codecStatus);
            }
            else
            {
                log.Error("No codec online with id {0}. Can't update status hub.", id);
            }
        }
    }
}