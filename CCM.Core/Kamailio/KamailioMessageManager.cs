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
using System.Linq;
using System.Reflection;
using CCM.Core.Entities;
using CCM.Core.Entities.Specific;
using CCM.Core.Extensions;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Managers;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Kamailio.Messages;
using NLog;

namespace CCM.Core.Kamailio
{
    public class KamailioMessageManager : ISipMessageManager
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly ICallRepository _callRepository;
        private readonly IKamailioMessageParser _kamailioMessageParser;
        private readonly IRegisteredSipRepository _sipRepository;

        public KamailioMessageManager(IRegisteredSipRepository sipRepository, ICallRepository callRepository, IKamailioMessageParser kamailioMessageParser)
        {
            _sipRepository = sipRepository;
            _callRepository = callRepository;
            _kamailioMessageParser = kamailioMessageParser;
        }

        public KamailioMessageHandlerResult HandleMessage(string message)
        {
            KamailioMessageBase sipMessage = _kamailioMessageParser.Parse(message);

            if (sipMessage == null)
            {
                log.Warn("Incorrect Kamailio message format: {0}", message);
                return new KamailioMessageHandlerResult { ChangeStatus = KamailioMessageChangeStatus.NothingChanged };
            }

            if (log.IsInfoEnabled)
            {
                log.Debug("Parsed Kamailio Message {0}", sipMessage.ToDebugString());
            }

            var kamailioMessageResult = DoHandleMessage(sipMessage);
            log.Debug("Handled Kamailio message with result {0}. {1}", kamailioMessageResult.ChangeStatus, sipMessage.ToDebugString());
            return kamailioMessageResult;
        }

        public KamailioMessageHandlerResult DoHandleMessage(KamailioMessageBase message)
        {
            if (message is KamailioRegistrationMessage)
            {
                var regMessage = (KamailioRegistrationMessage)message;

                if (regMessage.Expires == 0)
                {
                    return UnregisterCodec(new KamailioRegistrationExpireMessage()
                    {
                        SipAddress = regMessage.Sip
                    });
                }
                else
                {
                    return RegisterSip((KamailioRegistrationMessage)message);
                }
            }


            if (message is KamailioRegistrationExpireMessage)
            {
                return UnregisterCodec((KamailioRegistrationExpireMessage)message);
            }

            var dialogMessage = message as KamailioDialogMessage;
            if (dialogMessage != null)
            {
                if (dialogMessage.Status == DialogStatus.Start)
                {
                    return RegisterCall(dialogMessage);
                }
                if (dialogMessage.Status == DialogStatus.End)
                {
                    // TODO: Check hangup reason. Only close calls where reason = Normal
                    return CloseCall(dialogMessage);
                }
                if (dialogMessage.Status == DialogStatus.SingleBye)
                {
                    // TODO: Handle single bye message
                    // TODO: Close call
                    log.Info("Received SingleBye command from Kamailio. {0}", message);
                    return NothingChangedResult;
                }
            }

            log.Info("Unhandled Kamailio message: {0}", message.ToDebugString());
            return NothingChangedResult;
        }

        public KamailioMessageHandlerResult RegisterSip(KamailioRegistrationMessage sipMessage)
        {
            var sip = new RegisteredSip
            {
                IP = sipMessage.Ip,
                Port = sipMessage.Port,
                ServerTimeStamp = sipMessage.UnixTimeStamp,
                SIP = sipMessage.Sip.UserAtHost,
                UserAgentHead = sipMessage.UserAgent,
                Username = sipMessage.Username,
                DisplayName = sipMessage.ToDisplayName,
                Expires = sipMessage.Expires
            };

            return _sipRepository.UpdateRegisteredSip(sip);
        }

        private KamailioMessageHandlerResult UnregisterCodec(KamailioRegistrationExpireMessage expireMessage)
        {
            return _sipRepository.DeleteRegisteredSip(expireMessage.SipAddress.UserAtHost);

        }

        public KamailioMessageHandlerResult RegisterCall(KamailioDialogMessage sipMessage)
        {
            log.Debug("Register call from {0} to {1}, call id \"{2}\", hash id:\"{3}\", hash entry:\"{4}\"",
                sipMessage.FromSipUri.UserAtHost, sipMessage.ToSipUri.UserAtHost, sipMessage.CallId, sipMessage.HashId, sipMessage.HashEntry);

            if (_callRepository.CallExists(sipMessage.CallId, sipMessage.HashId, sipMessage.HashEntry))
            {
                log.Debug("Call with id {0}, hash id:{1}, hash entry:{2} already exists", sipMessage.CallId, sipMessage.HashId, sipMessage.HashEntry);
                return NothingChangedResult;
            }

            var call = new Call();

            // Om user-delen är numerisk antar vi att det är ett telefonnummer (trots att sip-adresser egentligen kan vara numeriska)
            var fromSip = sipMessage.FromSipUri.User.IsNumeric() ? sipMessage.FromSipUri.User : sipMessage.FromSipUri.UserAtHost;
            var from = _sipRepository.GetCachedRegisteredSips().SingleOrDefault(rs => rs.Sip == fromSip);
            call.FromSip = fromSip;
            call.FromDisplayName = sipMessage.FromDisplayName;
            call.FromId = from != null ? from.Id : Guid.Empty;

            var toSip = sipMessage.ToSipUri.User.IsNumeric() ? sipMessage.ToSipUri.User : sipMessage.ToSipUri.UserAtHost;
            var to = _sipRepository.GetCachedRegisteredSips().SingleOrDefault(rs => rs.Sip == toSip);
            call.ToSip = toSip;
            call.ToDisplayName = sipMessage.ToDisplayName;
            call.ToId = to != null ? to.Id : Guid.Empty;

            call.Started = DateTime.UtcNow;
            call.CallId = sipMessage.CallId;
            call.DlgHashId = sipMessage.HashId;
            call.DlgHashEnt = sipMessage.HashEntry;
            call.Updated = DateTime.UtcNow;
            call.ToTag = sipMessage.ToTag;
            call.FromTag = sipMessage.FromTag;
            call.IsPhoneCall = sipMessage.FromSipUri.User.IsNumeric() || sipMessage.ToSipUri.User.IsNumeric(); // Sätts till true om någon parts adress är numeriskt. 
            _callRepository.UpdateCall(call);
            return SipMessageResult(KamailioMessageChangeStatus.CallStarted, call.Id);
        }

        public KamailioMessageHandlerResult CloseCall(KamailioDialogMessage sipMessage)
        {
            log.Debug("Closing call with id:\"{0}\", hash id:\"{1}\", hash entry:\"{2}\"", sipMessage.CallId, sipMessage.HashId, sipMessage.HashEntry);

            try
            {
                CallInfo call = _callRepository.GetCallInfo(sipMessage.CallId, sipMessage.HashId, sipMessage.HashEntry);

                if (call == null)
                {
                    log.Warn("Unable to find call with call id: {0}, hash id:{1}, hash entry:{2}", sipMessage.CallId, sipMessage.HashId, sipMessage.HashEntry);
                    return NothingChangedResult;
                }

                if (call.Closed)
                {
                    log.Warn("Call with call id: {0} already closed", sipMessage.CallId);
                    return NothingChangedResult;
                }

                _callRepository.CloseCall(call.Id);
                return SipMessageResult(KamailioMessageChangeStatus.CallClosed, call.Id);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error while closing call with call id: {0}", sipMessage.CallId);
                return NothingChangedResult;
            }
        }



        private KamailioMessageHandlerResult NothingChangedResult => SipMessageResult(KamailioMessageChangeStatus.NothingChanged);
        private KamailioMessageHandlerResult SipMessageResult(KamailioMessageChangeStatus status) { return new KamailioMessageHandlerResult() { ChangeStatus = status }; }
        private KamailioMessageHandlerResult SipMessageResult(KamailioMessageChangeStatus status, Guid id) { return new KamailioMessageHandlerResult() { ChangeStatus = status, ChangedObjectId = id }; }

    }
}