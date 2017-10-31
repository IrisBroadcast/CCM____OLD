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

﻿using System.ComponentModel;

namespace CCM.Core.CodecControl.Enums
{
    public enum DisconnectReason
    {
        // TODO: Dessa beskrivningar är inte bra, se över
        [Description("Ingen orsak angiven")] None,
        [Description("Kan inte tolka angiven titel")] UnableToResolveName,
        [Description("Problem vid anslutning")] ErrorConnecting,
        [Description("Annat protokoll används")] DifferentProtocol,
        [Description("Samtal avvisat")] ConnectionRejected,
        [Description("Mottagaren har avslutat")]RemoteDisconnected,
        [Description("Samtalet är pålagt")]HangUp,
        [Description("Samtalet avbrutet")]ConnectionDropped,
        [Description("Inte redo")]NotReady,
        [Description("Undersöker förbindelse")]SipTrying = 100,
        [Description("Begäran kopplas upp")]SipRinging = 180,
        [Description("Samtalet vidarebefordras")]SipCallBeingForwarded = 181,
        [Description("Väntar på mottagaren")]SipQueued = 182,
        [Description("Status om framsteg")]SipProgress = 183,
        [Description("Begäran lyckades")]SipOk = 200,
        [Description("Begäran är accepterad")]SipAccepted = 202,
        [Description("Flera val tillgängliga")]SipMultipleChoices = 300,
        [Description("Mottagaren har flyttat permanent")]SipMovedPermanently = 301,
        [Description("Mottagaren har flyttat temporärt")]SipMovedTemporarily = 302,
        [Description("För att nå mottagaren behövs en proxy")]SipUseProxy = 305,
        [Description("Samtalet misslyckades, andra alternativ finns")]SipAlternativeService = 380,
        [Description("Felformaterad begäran")]SipBadRequest = 400,
        [Description("Begäran kräver auktorisering")]SipUnauthorized = 401,
        [Description("Begäran kräver betalning")]SipPaymentRequired = 402,
        [Description("Begäran kom fram men den kommer inte att uppfyllas")]SipForbidden = 403,
        [Description("Mottagaren finns inte")]SipNotFound = 404,
        [Description("Begäran är inte tillåten för specifierad mottagare")]SipMethodNotAllowed = 405,
        [Description("Begäran kan inte genomföras med angivna kriterier")]SipNotAcceptable = 406,
        [Description("Förfrågan kräver autentisering mot proxyn")]SipProxyAuthenticationRequired = 407,
        [Description("Mottagaren svarade inte inom angiven timeout")]SipRequestTimeout = 408,
        [Description("Mottagaren har existerat men är inte tillgänglig längre")]SipGone = 410,
        [Description("Begäran innehåller för många tecken")]SipRequestEntityTooLarge = 413,
        [Description("Addressen angiven innehåller för många tecken, servern kommer inte tolka detta")]SipRequestUriTooLong = 414,
        [Description("Formatet på begäran stöds inte")]SipUnsupportedMediaType = 415,
        [Description("Addressen är felformaterad")]SipUnsupportedUriScheme = 416,
        [Description("Anknytningsprotokollet som används går inte att tolka")]SipBadExtension = 420,
        [Description("Servern behöver ett anknytningsprotokoll som inte är angivet")]SipExtensionRequired = 421,
        [Description("Sessionens förfallotid i begäran är satt för lågt")]SipSessionTimerTooSmall = 422,
        [Description("Källans förfallotid är satt för lågt")]SipIntervalTooBrief = 423,
        [Description("Samtal till mottagaren är inte tillgängligt just nu")]SipTemporarilyUnavailable = 480,
        [Description("Begäran kan inte matchas mot något pågående samtal")]SipCallTsxDoesNotExist = 481,
        [Description("En loop har upptäckts av servern")]SipLoopDetected = 482,
        [Description("Antalet vidarebefordringar har nått sin maxgräns")]SipTooManyHops = 483,
        [Description("Addressen i begäran är okomplett")]SipAddressIncomplete = 484,
        [Description("Addressen i begäran är tvetydlig")]SipAmbiguous = 485,
        [Description("Mottagaren är upptagen")]SipBusyHere = 486,
        [Description("Mottagaren har avbrutit samtalet")]SipRequestTerminated = 487,
        [Description("Begäran är inte accepterad av mottagaren")]SipNotAcceptableHere = 488,
        [Description("Begäran stötte på en dålig förfrågan")]SipBadEvent = 489,
        [Description("Begäran är uppdaterad")]SipRequestUpdated = 490,
        [Description("Begäran avvaktar")]SipRequestPending = 491,
        [Description("Omöjligt att tolka begäran")]SipUndecipherable = 493,
        [Description("Internt serverfel")]SipInternalServerError = 500,
        [Description("Inte implementerad")]SipNotImplemented = 501,
        [Description("Dålig gateway")]SipBadGateway = 502,
        [Description("Tjänsten är inte tillgänglig")]SipServiceUnavailable = 503,
        [Description("Servern nådde sin förfallotid")]SipServerTimeout = 504,
        [Description("Versionen stöds inte")]SipVersionNotSupported = 505,
        [Description("Meddelandet är för långt")]SipMessageTooLarge = 513,
        [Description("Förhandsvillkoren misslyckades")]SipPreconditionFailure = 580,
        [Description("Upptaget överallt")]SipBusyEverywhere = 600,
        [Description("Mottagaren avböjde samtalet")]SipDecline = 603,
        [Description("Önskad åtgärd finns inte någonstans")]SipDoesNotExistAnywhere = 604,
        [Description("Begäran finns inte någonstans")] SipNotAcceptableAnywhere = 606
    }
}