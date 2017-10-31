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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Caching;
using System.Security.Authentication;
using System.Web.Mvc;

namespace CCM.DiscoveryApi.Controllers
{
    public class DebugController : Controller
    {
        private const string SecretKey = "skruvsallad";

        public ActionResult CacheInfo(string key)
        {
            if (key != SecretKey) throw new AuthenticationException();

            return View();
        }

        public ActionResult GetCacheInfo()
        {
            var memoryCache = MemoryCache.Default;

            string name = memoryCache.Name;
            long count = memoryCache.GetCount();
            int hashCode = memoryCache.GetHashCode();

            var model = new CacheViewModel()
            {
                Name = name,
                Count = count,
                HashCode = hashCode,
                CachedItems = new List<CachedItem>()
            };

            Debug.WriteLine(string.Format("Cache name: {0}", name));
            Debug.WriteLine(string.Format("Antal cachade objekt: {0}", count));
            Debug.WriteLine(string.Format("Cache hash code: {0}", hashCode));
            Debug.WriteLine("");

            Debug.WriteLine("Cachens innehåll");

            var cacheEnumberable = (IEnumerable)memoryCache;
            foreach (DictionaryEntry item in cacheEnumberable)
            {
                IList cachedList = item.Value as IList;

                var cachedItem = new CachedItem
                {
                    CacheKey = item.Key.ToString(),
                    CachedObject = item.Value,
                    CachedType = item.Value.GetType(),
                    ListCount = cachedList == null ? (int?)null : cachedList.Count
                };
                model.CachedItems.Add(cachedItem);

                Debug.WriteLine(string.Format("Cachenyckel: {0}", cachedItem.CacheKey));
                Debug.WriteLine(string.Format("Cachad type: {0}", cachedItem.CachedType));

                if (cachedList != null)
                {
                    Debug.WriteLine("Cachat objekt är en lista");
                    Debug.WriteLine(string.Format("Antal objekt i cachad lista: {0}", cachedItem.ListCount));

                    foreach (var listItem in cachedList)
                    {
                        Debug.WriteLine(String.Format("\t{0}", listItem));
                    }
                }
                Debug.WriteLine("");
            }

            return View(model);
        }
    }

    public class CachedItem
    {
        public string CacheKey { get; set; }
        public Type CachedType { get; set; }
        public int? ListCount { get; set; }
        public object CachedObject { get; set; }
    }

    public class CacheViewModel
    {
        public string Name { get; set; }
        public long Count { get; set; }
        public int HashCode { get; set; }
        public IList<CachedItem> CachedItems { get; set; }
    }

}