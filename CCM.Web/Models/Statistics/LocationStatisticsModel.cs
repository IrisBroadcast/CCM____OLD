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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using CCM.Core.Entities.Statistics;

namespace CCM.Web.Models.Statistics
{
    public class LocationStatisticsModel
    {
        private static readonly CultureInfo SvCulture = CultureInfo.CreateSpecificCulture("sv-SE");

        public LocationStatisticsMode Mode { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } 
        public Guid RegionId { get; set; }
        public Guid OwnerId { get; set; } 
        public Guid CodecTypeId { get; set; }
        public List<LocationStatistics> Statistics { get; set; }

        public string ColumnLabel 
        {
            get
            {
                if (Mode == LocationStatisticsMode.MaxSimultaneousCalls) return Resources.Stats_NumberOfSimultaneousCalls;
                if (Mode == LocationStatisticsMode.TotaltTimeForCalls) return Resources.Stats_TotalCallTimeInMinutes;
                return Resources.Stats_NumberOfCalls;
            }
        }

        public IEnumerable<LocationStatisticsRow> GetRows()
        {
            if (Statistics == null || Statistics.Count == 0)
                yield break;
            var maxValue = Math.Max(GetMaxValue(), 1.0);
            var multiplier = 1.0/maxValue;
            foreach (var stats in Statistics)
            {
                yield return new LocationStatisticsRow
                {
                    Label = string.IsNullOrWhiteSpace(stats.LocationName) ? "-" : stats.LocationName,
                    Width = GetWidth(Mode, stats, multiplier),
                    Value = GetValue(Mode, stats),
                    ToolTip = GetToolTip(Mode, stats)
                };
            }
        }

        private double GetMaxValue()
        {
            if (Statistics == null || Statistics.Count == 0) return 0;
            if (Mode == LocationStatisticsMode.MaxSimultaneousCalls)
                return Statistics.Max(s => s.MaxSimultaneousCalls);
            if (Mode == LocationStatisticsMode.TotaltTimeForCalls)
                return Statistics.Max(s => s.TotaltTimeForCalls);
            return Statistics.Max(s => s.NumberOfCalls);
        }

        private static string GetWidth(LocationStatisticsMode mode, LocationStatistics stats, double multiplier)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.#}",
                Math.Max(0.1, GetRawValue(mode, stats)*multiplier*50));
        }

        private static string GetValue(LocationStatisticsMode mode, LocationStatistics stats)
        {
            return string.Format(SvCulture, "{0}", Math.Round(GetRawValue(mode, stats), MidpointRounding.ToEven));
        }

        private static double GetRawValue(LocationStatisticsMode mode, LocationStatistics stats)
        {
            if (mode == LocationStatisticsMode.MaxSimultaneousCalls) return stats.MaxSimultaneousCalls;
            if (mode == LocationStatisticsMode.TotaltTimeForCalls) return stats.TotaltTimeForCalls;
            return stats.NumberOfCalls;
        }

        private static string GetToolTip(LocationStatisticsMode mode, LocationStatistics stats)
        {
            if (mode != LocationStatisticsMode.MaxSimultaneousCalls) return string.Empty;
            if (stats.MaxSimultaneousCalls <= 1) return string.Empty;

            var dates = stats.MaxSimultaneousEventDates.ToList();
            var sb = new StringBuilder();
            var format = dates.Count > 1
                ? Resources.Stats_SimultaneousCallsToolTipM
                : Resources.Stats_SimultaneousCallsToolTip1;
            sb.AppendFormat(format, stats.MaxSimultaneousCalls, dates.Count);
            dates.ForEach(d => sb.AppendLine().AppendFormat("{0:yyyy-MM-dd}", d));
            return sb.ToString();
        }
    }

    public class LocationStatisticsRow
    {
        public string Label { get; set; }
        public string Width { get; set; }
        public string Value { get; set; }
        public string ToolTip { get; set; }
    }

    public enum LocationStatisticsMode
    {
        NumberOfCalls,
        TotaltTimeForCalls,
        MaxSimultaneousCalls
    }
}