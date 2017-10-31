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

namespace CCM.Data.Repositories
{
    public class RegionRepository : BaseRepository, IRegionRepository
    {
        public RegionRepository(IAppCache cache) : base(cache)
        {
        }

        public void Save(Region region)
        {
            using (var db = GetDbContext())
            {
                Entities.RegionEntity dbRegion;
                var timeStamp = DateTime.UtcNow;

                if (region.Id != Guid.Empty)
                {
                    dbRegion = db.Regions.SingleOrDefault(g => g.Id == region.Id);

                    if (dbRegion == null)
                    {
                        throw new Exception("Region could not be found");
                    }

                    dbRegion.Locations.Clear();
                }
                else
                {
                    dbRegion = new Entities.RegionEntity
                    {
                        Id = Guid.NewGuid(),
                        CreatedBy = region.CreatedBy,
                        CreatedOn = timeStamp,
                        Locations = new Collection<Entities.LocationEntity>()
                    };

                    region.Id = dbRegion.Id;
                    region.CreatedOn = dbRegion.CreatedOn;
                    db.Regions.Add(dbRegion);
                }


                dbRegion.Name = region.Name;
                dbRegion.UpdatedBy = region.UpdatedBy;
                dbRegion.UpdatedOn = timeStamp;
                region.UpdatedOn = dbRegion.UpdatedOn;

                // Add relations
                foreach (var location in region.Locations)
                {
                    var dbLocation = db.Locations.SingleOrDefault(l => l.Id == location.Id);
                    if (dbLocation != null)
                    {
                        dbRegion.Locations.Add(dbLocation);
                    }
                }

                db.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            using (var db = GetDbContext())
            {
                var dbRegion = db.Regions.SingleOrDefault(g => g.Id == id);
                if (dbRegion != null)
                {
                    dbRegion.Locations.Clear();

                    db.Regions.Remove(dbRegion);
                    db.SaveChanges();
                }
            }
        }

        public Region GetById(Guid id)
        {
            using (var db = GetDbContext())
            {
                var dbRegion = db.Regions
                    .Include(r => r.Locations)
                    .SingleOrDefault(g => g.Id == id);
                return MapToRegion(dbRegion, true);
            }
        }
        
        public List<Region> GetAll()
        {
            using (var db = GetDbContext())
            {
                var dbRegions = db.Regions
                    .Include(r => r.Locations)
                    .ToList();
                return dbRegions.Select(o => MapToRegion(o, true)).OrderBy(g => g.Name).ToList();
            }
        }

        public List<Region> FindRegions(string search)
        {
            using (var db = GetDbContext())
            {
                var dbRegions = db.Regions
                    .Include(r => r.Locations)
                    .Where(r => r.Name.ToLower().Contains(search))
                    .ToList();
                return dbRegions.Select(o => MapToRegion(o, true)).OrderBy(g => g.Name).ToList();
            }
        }

        private Region MapToRegion(Entities.RegionEntity dbRegion, bool includeLocations)
        {
            if (dbRegion == null) return null;

            return new Region
            {
                Id = dbRegion.Id,
                Name = dbRegion.Name,
                Locations = includeLocations ? dbRegion.Locations.Select(MapToLocation).ToList() : new List<Location>(),
                CreatedBy = dbRegion.CreatedBy,
                CreatedOn = dbRegion.CreatedOn,
                UpdatedBy = dbRegion.UpdatedBy,
                UpdatedOn = dbRegion.UpdatedOn
            };
        }

        public static Location MapToLocation(Entities.LocationEntity dbLocation)
        {
            if (dbLocation == null) return null;

            return new Location
            {
                Cidr = dbLocation.Cidr,
                Id = dbLocation.Id,
                Name = dbLocation.Name,
                Net = dbLocation.Net_Address_v4,
                CreatedBy = dbLocation.CreatedBy,
                CreatedOn = dbLocation.CreatedOn,
                UpdatedBy = dbLocation.UpdatedBy,
                UpdatedOn = dbLocation.UpdatedOn,
            };
        }
    }
}