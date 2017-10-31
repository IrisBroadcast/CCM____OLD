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
using CCM.Core.Attributes;
using CCM.Core.Entities;
using CCM.Core.Entities.Specific;
using CCM.Core.Interfaces.Managers;
using CCM.Core.Interfaces.Repositories;

namespace CCM.Core.Managers
{
    /// <summary>
    /// Handles the filter properties for RegisteredSip.
    /// </summary>
    public class FilterManager : IFilterManager
    {
        private readonly IFilterRepository _filterRepository;

        public FilterManager(IFilterRepository filterRepository)
        {
            _filterRepository = filterRepository;
        }

        /// <summary>
        /// Hämtar lista med egenskaper hos CachedRegisteredSip-objektet som är möjliga att filtrera på.
        /// </summary>
        /// <returns></returns>
        public List<AvailableFilter> GetFilterProperties()
        {
            var filterProperties = (typeof(RegisteredSipDto)).GetProperties().Where(p => p.GetCustomAttributes(false).Any(a => a is FilterPropertyAttribute));

            List<AvailableFilter> availableFilters = new List<AvailableFilter>();

            foreach (var property in filterProperties)
            {
                var filterAttribute = property.GetCustomAttributes(typeof(FilterPropertyAttribute), false).FirstOrDefault() as FilterPropertyAttribute;

                if (filterAttribute != null)
                {
                    var filter = new AvailableFilter
                    {
                        FilteringName = property.Name,
                        TableName = filterAttribute.TableName,
                        ColumnName = filterAttribute.ColumnName
                    };
                    availableFilters.Add(filter);
                }
                
            }

            return availableFilters;
        }

        /// <summary>
        /// Checks the filter name availability.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public bool CheckFilterNameAvailability(string name, Guid id)
        {
            return _filterRepository.CheckFilterNameAvailability(name, id);
        }

        public void Save(Filter filter)
        {
            _filterRepository.Save(filter);
        }

        public List<Filter> GetAllFilters()
        {
            return _filterRepository.GetAll() ?? new List<Filter>();
        }

        /// <summary>
        /// Hämtar aktiva filter inklusive lista med deras möjliga val
        /// </summary>
        /// <returns></returns>
        public List<AvailableFilter> GetAvailableFiltersIncludingOptions()
        {
            var filters = GetAllFilters() ?? new List<Filter>();

            return filters.Select(filter => new AvailableFilter
            {
                Name = filter.Name,
                FilteringName = filter.FilteringName,
                TableName = filter.TableName,
                ColumnName = filter.ColumnName,
                Options = _filterRepository.GetFilterPropertyValues(filter.TableName, filter.ColumnName)
            }).ToList();
        }

        public Filter GetFilter(Guid id)
        {
            return _filterRepository.GetById(id);
        }

        public void Delete(Guid id)
        {
            _filterRepository.Delete(id);
        }

        public List<Filter> FindFilters(string search)
        {
            return _filterRepository.Find(search);
        }
    }
}