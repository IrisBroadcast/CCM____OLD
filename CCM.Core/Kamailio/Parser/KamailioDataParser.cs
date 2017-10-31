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
using CCM.Core.Interfaces;
using CCM.Core.Kamailio.Messages;
using NLog;

namespace CCM.Core.Kamailio.Parser
{
    public class KamailioDataParser : IKamailioDataParser
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public KamailioData ParseToKamailioData(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                log.Warn("Message body empty");
                return null;
            }

            string[] dataFields = message.Split('|');

            if (dataFields.Length == 0)
            {
                return null;
            }

            KamailioMessageType msgType;
            if (!Enum.TryParse(dataFields[0], true, out msgType))
            {
                log.Warn("Unable to get message type from {0}", dataFields[0]);
                return null;
            }

            var fieldsDictionary = dataFields
                .Select(x => x.Split(new[] { "::" }, StringSplitOptions.None))
                .Where(x => x.Length == 2 && !String.IsNullOrEmpty(x[0].Trim()))
                .ToDictionary(x => x[0].Trim(), x => x[1] == "<null>" ? string.Empty : x[1].Trim());

            return new KamailioData { MessageType = msgType, Fields = fieldsDictionary };
        }

    }
}