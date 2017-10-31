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
using System.Collections.ObjectModel;
using System.Linq;
using System.Data.Entity;
using CCM.Core.Entities;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Repositories;
using LazyCache;
using NLog;

namespace CCM.Data.Repositories
{
    public class CityRepository : BaseRepository, ICityRepository
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public CityRepository(IAppCache cache) : base(cache)
        {
        }

        public void Save(City city)
        {
            using (var db = GetDbContext())
            {
                Entities.CityEntity dbCity;

                if (city.Id != Guid.Empty)
                {
                    dbCity = db.Cities.SingleOrDefault(g => g.Id == city.Id);
                    if (dbCity == null)
                    {
                        throw new Exception("Region could not be found");
                    }

                    dbCity.Locations.Clear();

                }
                else
                {
                    dbCity = new Entities.CityEntity
                    {
                        Id = Guid.NewGuid(),
                        CreatedBy = city.CreatedBy,
                        CreatedOn = DateTime.UtcNow
                    };
                    city.Id = dbCity.Id;
                    city.CreatedOn = dbCity.CreatedOn;
                    db.Cities.Add(dbCity);
                    dbCity.Locations = new Collection<Entities.LocationEntity>();
                }
                
                dbCity.Name = city.Name;
                dbCity.UpdatedBy = city.UpdatedBy;
                dbCity.UpdatedOn = DateTime.UtcNow;
                city.UpdatedOn = dbCity.UpdatedOn;

                foreach (var location in city.Locations)
                {
                    var dbLocation = db.Locations.SingleOrDefault(l => l.Id == location.Id);
                    if (dbLocation != null)
                    {
                        dbCity.Locations.Add(dbLocation);
                    }
                }

                db.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            using (var db = GetDbContext())
            {
                var dbCity = db.Cities.SingleOrDefault(g => g.Id == id);
                if (dbCity != null)
                {
                    dbCity.Locations.Clear();
                    db.Cities.Remove(dbCity);
                    db.SaveChanges();
                }
            }
        }

        public City GetById(Guid id)
        {
            using (var db = GetDbContext())
            {
                var dbCity = db.Cities
                    .Include(c => c.Locations)
                    .SingleOrDefault(g => g.Id == id);

                return MapToCity(dbCity);
            }
        }

        public List<City> GetAll()
        {
            using (var db = GetDbContext())
            {
                var dbCities = db.Cities
                    .Include(c => c.Locations)
                    .OrderBy(g => g.Name).ToList();

                return dbCities.Select(MapToCity).ToList();
            }
        }

        public List<City> Find(string search)
        {
            using (var db = GetDbContext())
            {
                var dbCities = db.Cities
                    .Include(c => c.Locations)
                    .Where(c => c.Name.ToLower().Contains(search.ToLower())).ToList();

                return dbCities
                    .Select(MapToCity)
                    .OrderBy(g => g.Name)
                    .ToList();
            }
        }

        public static City MapToCity(Entities.CityEntity dbCity)
        {
            if (dbCity == null) return null;

            return new City
            {
                Id = dbCity.Id,
                Name = dbCity.Name,
                CreatedBy = dbCity.CreatedBy,
                CreatedOn = dbCity.CreatedOn,
                UpdatedBy = dbCity.UpdatedBy,
                UpdatedOn = dbCity.UpdatedOn,
                Locations = dbCity.Locations.Select(MapToLocation).ToList()
            };
        }

        public static Location MapToLocation(Entities.LocationEntity dbLocation)
        {
            if (dbLocation == null) return null;

            return new Location
            {
                Id = dbLocation.Id,
                Name = dbLocation.Name,
                CreatedBy = dbLocation.CreatedBy,
                CreatedOn = dbLocation.CreatedOn,
                UpdatedBy = dbLocation.UpdatedBy,
                UpdatedOn = dbLocation.UpdatedOn,
            };
        }

    }
}