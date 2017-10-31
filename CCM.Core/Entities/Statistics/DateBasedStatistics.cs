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

namespace CCM.Core.Entities.Statistics
{
    public class DateBasedStatistics
    {
        public double AverageTime
        {
            get { return NumberOfCalls == 0 ? 0 : TotaltTimeForCalls/NumberOfCalls; }
        }

        public DateTime Date { get; set; }
        public double MaxCallTime { get; private set; }
        public double MinCallTime { get; private set; }
        public int NumberOfCalls { get; private set; }
        public double TotaltTimeForCalls { get; private set; }

        public void AddTime(double timeInMinutes)
        {
            if (timeInMinutes < MinCallTime && NumberOfCalls > 0)
            {
                MinCallTime = timeInMinutes;
            }
            else if (NumberOfCalls == 0)
            {
                MinCallTime = timeInMinutes;
            }

            if (timeInMinutes > MaxCallTime)
            {
                MaxCallTime = timeInMinutes;
            }

            TotaltTimeForCalls += timeInMinutes;
            NumberOfCalls++;
        }
    }

    public class DateBasedCallEvent
    {
        public DateTime Date { get; set; }
        public double Duration { get; set; }

        public static IEnumerable<DateBasedCallEvent> GetEvents(IList<CallHistory> callHistories, DateTime reportPeriodStart, DateTime reportPeriodEnd)
        {
            return callHistories.SelectMany(callHistory => GetEvents(callHistory, reportPeriodStart, reportPeriodEnd));
        }

        private static IEnumerable<DateBasedCallEvent> GetEvents(CallHistory callHistory, DateTime reportPeriodStart, DateTime reportPeriodEnd)
        {
            var minDate = callHistory.Started >= reportPeriodStart ? callHistory.Started : reportPeriodStart;
            var maxDate = callHistory.Ended <= reportPeriodEnd ? callHistory.Ended : reportPeriodEnd;

            var currentDate = minDate.ToLocalTime().Date.ToUniversalTime();
            while (currentDate < maxDate)
            {
                var next = currentDate.AddDays(1.0);
                var duration = (maxDate > next ? next : maxDate) - (minDate < currentDate ? currentDate : minDate);
                yield return new DateBasedCallEvent { Date = currentDate.ToLocalTime(), Duration = duration.TotalMinutes};
                currentDate = next;
            }
        }
    }
}