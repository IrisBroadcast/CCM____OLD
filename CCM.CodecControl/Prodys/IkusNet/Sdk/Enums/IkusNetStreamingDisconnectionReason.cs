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

﻿namespace CCM.CodecControl.Prodys.IkusNet.Sdk.Enums
{
    public enum IkusNetStreamingDisconnectionReason
    {
        None,
        UnableToResolveName,
        ErrorConnecting,
        DifferentProtocol,
        ConnectionRejected,
        RemoteDisconnected,
        HangUp,
        ConnectionDropped,
        NotReady,

        // SIP Error codes
        SipTrying = 100,
        SipRinging = 180,
        SipCallBeingForwarded = 181,
        SipQueued = 182,
        SipProgress = 183,
        SipOk = 200,
        SipAccepted = 202,
        SipMultipleChoices = 300,
        SipMovedPermanently = 301,
        SipMovedTemporarily = 302,
        SipUseProxy = 305,
        SipAlternativeService = 380,
        SipBadRequest = 400,
        SipUnauthorized = 401,
        SipPaymentRequired = 402,
        SipForbidden = 403,
        SipNotFound = 404,
        SipMethodNotAllowed = 405,
        SipNotAcceptable = 406,
        SipProxyAuthenticationRequired = 407,
        SipRequestTimeout = 408,
        SipGone = 410,
        SipRequestEntityTooLarge = 413,
        SipRequestUriTooLong = 414,
        SipUnsupportedMediaType = 415,
        SipUnsupportedUriScheme = 416,
        SipBadExtension = 420,
        SipExtensionRequired = 421,
        SipSessionTimerTooSmall = 422,
        SipIntervalTooBrief = 423,
        SipTemporarilyUnavailable = 480,
        SipCallTsxDoesNotExist = 481,
        SipLoopDetected = 482,
        SipTooManyHops = 483,
        SipAddressIncomplete = 484,
        SipAmbiguous = 485,
        SipBusyHere = 486,
        SipRequestTerminated = 487,
        SipNotAcceptableHere = 488,
        SipBadEvent = 489,
        SipRequestUpdated = 490,
        SipRequestPending = 491,
        SipUndecipherable = 493,
        SipInternalServerError = 500,
        SipNotImplemented = 501,
        SipBadGateway = 502,
        SipServiceUnavailable = 503,
        SipServerTimeout = 504,
        SipVersionNotSupported = 505,
        SipMessageTooLarge = 513,
        SipPreconditionFailure = 580,
        SipBusyEverywhere = 600,
        SipDecline = 603,
        SipDoesNotExistAnywhere = 604,
        SipNotAcceptableAnywhere = 606
    }
}