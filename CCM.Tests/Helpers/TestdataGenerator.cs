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
using System.Linq.Expressions;
using System.Reflection;

namespace CCM.Tests.Helpers
{
    public class TestdataGenerator
    {
        private static Random rnd = new Random();

        /// <summary>
        /// Skapar ett objekt av typ T och sätter dess enkla datatyper till slumpade värden
        /// </summary>
        public static T CreateRandomObject<T>()
        {
            return SetRandomPropertyValues(Activator.CreateInstance<T>());
        }

        /// <summary>
        /// Uppdaterar värden på egenskaper till slumpade värden.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="excludedProperties">Dessa egenskaper uppdateras inte</param>
        /// <returns></returns>
        public static T SetRandomPropertyValues<T>(T o, params Expression<Func<T, object>>[] excludedProperties)
        {
            string[] propNames = excludedProperties.Select(GetPropertyName).ToArray();
            return SetRandomPropertyValues(o, propNames);
        }

        /// <summary>
        /// Sätter enkla datatyper till slumpade värden på objektet
        /// </summary>
        private static T SetRandomPropertyValues<T>(T o, string[] exceptions = null)
        {
            List<PropertyInfo> propertyInfos = o.GetType().GetProperties().ToList();
            if (exceptions != null)
            {
                propertyInfos = propertyInfos.Where(x => !exceptions.Contains(x.Name)).ToList();
            }
            foreach (var propertyInfo in propertyInfos)
            {
                object value = null;
                switch (propertyInfo.PropertyType.Name.ToLower())
                {
                    case "string":
                        value = string.Format("{0}_{1}", propertyInfo.Name, Guid.NewGuid().ToString().Substring(0, 4));
                        break;
                    case "int32":
                    case "long":
                        value = rnd.Next(1, 10000);
                        break;
                    case "bool":
                        value = Convert.ToBoolean(rnd.Next(0, 1));
                        break;
                    case "datetime":
                        var dt = DateTime.Now.Subtract(new TimeSpan(rnd.Next(400), rnd.Next(24), rnd.Next(60), rnd.Next(60)));
                        value = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second); // Milliseconds always 0
                        break;
                }

                if (value != null)
                {
                    propertyInfo.SetValue(o, Convert.ChangeType(value, propertyInfo.PropertyType), null);
                }
            }

            return o;
        }

        public static string GetPropertyName<T>(Expression<Func<T, object>> prop)
        {
            MemberExpression expr;

            if (prop.Body is MemberExpression)
                // .Net interpreted this code trivially like t => t.Id
                expr = (MemberExpression)prop.Body;
            else
                // .Net wrapped this code in Convert to reduce errors, meaning it's t => Convert(t.Id) - get at the
                // t.Id inside
                expr = (MemberExpression)((UnaryExpression)prop.Body).Operand;

            string name = expr.Member.Name;

            return name;
        }


    }
}
