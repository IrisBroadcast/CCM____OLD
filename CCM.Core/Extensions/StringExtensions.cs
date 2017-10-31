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
using System.Globalization;
using System.Linq;
using System.Text;

namespace CCM.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNumeric(this string s)
        {
            s = s ?? string.Empty;
            return s.All(Char.IsDigit);
        }

        public static string CsvEscape(this string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return "\"\"";
            return value.IndexOfAny(new[] { '\r', '\n', '"', CsvSeparator }) >= 0 ? String.Format("\"{0}\"", value.Replace("\"", "\"\"")) : value;
        }

        public static StringBuilder AddCsvSeparator(this StringBuilder sb)
        {
            sb.Append(CsvSeparator);
            return sb;
        }

        public static StringBuilder AddCsvValue(this StringBuilder sb, string value)
        {
            sb.Append(value.CsvEscape());
            return sb;
        }

        public static StringBuilder AddCsvValue(this StringBuilder sb, int value)
        {
            sb.Append(value);
            return sb;
        }

        public static StringBuilder AddCsvValue(this StringBuilder sb, double value, int decimals = 1, CultureInfo culture = null)
        {
            var cultureInfo = culture ?? CultureInfo.InvariantCulture;
            if (decimals < 1)
            {
                sb.AppendFormat(cultureInfo, "{0}", Math.Round(value, MidpointRounding.ToEven));
                return sb;
            }
            var format = "{0:0." + "#".PadRight(decimals, '#') + "}";
            sb.AppendFormat(cultureInfo, format, value);
            return sb;
        }

        private static char CsvSeparator
        {
            get
            {
                return ';';
            }
        }

        /// <summary>
        /// Returnerar början av strängen fram till angiven sträng
        /// </summary>
        public static string LeftOf(this string s, string untilString)
        {
            s = s ?? string.Empty;
            untilString = untilString ?? string.Empty;

            if (string.IsNullOrEmpty(untilString))
            {
                return s;
            }

            var pos = s.IndexOf(untilString, StringComparison.CurrentCulture);
            return pos > -1 ? s.Substring(0, pos) : s;
        }

        public static string Shorten(this string s, int length)
        {
            s = s ?? string.Empty;
            if (s.Length <= length)
            {
                return s;
            }
            return string.Format("{0}...", s.Substring(0, length - 3));
        }

    }
}