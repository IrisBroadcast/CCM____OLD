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
using System.Net;
using CCM.Core.Entities;
using CCM.Core.Interfaces.Repositories;
using CCM.Data.Entities;
using System.Data.Entity;
using System.Net.Sockets;
using CCM.Core.Entities.Specific;
using NLog;
using AutoMapper;
using CCM.Core.Extensions;
using LazyCache;

namespace CCM.Data.Repositories
{
    public class LocationRepository : BaseRepository, ILocationRepository
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public LocationRepository(IAppCache cache) : base(cache)
        {
        }

        public void Save(Location location)
        {
            using (var db = GetDbContext())
            {
                LocationEntity dbLocation;

                if (location.Id != Guid.Empty)
                {
                    dbLocation = db.Locations
                        .Include(l => l.ProfileGroup)
                        .SingleOrDefault(l => l.Id == location.Id);

                    if (dbLocation == null)
                    {
                        throw new NullReferenceException("Location");
                    }
                }
                else
                {
                    dbLocation = new LocationEntity
                    {
                        Id = Guid.NewGuid(),
                        CreatedBy = location.CreatedBy,
                        CreatedOn = DateTime.UtcNow
                    };
                    location.Id = dbLocation.Id;
                    location.CreatedOn = dbLocation.CreatedOn;
                    db.Locations.Add(dbLocation);
                }

                dbLocation.Name = location.Name;
                dbLocation.ShortName = location.ShortName;
                dbLocation.Comment = location.Comment;
                dbLocation.CarrierConnectionId = location.CarrierConnectionId;
                dbLocation.UpdatedBy = location.UpdatedBy;
                dbLocation.UpdatedOn = DateTime.UtcNow;

                location.UpdatedOn = dbLocation.UpdatedOn;

                // IP V4
                IPNetwork ipv4Network;
                if (IPNetwork.TryParse(location.Net, location.Cidr ?? 0, out ipv4Network))
                {
                    dbLocation.Net_Address_v4 = ipv4Network.Network.ToString();
                    dbLocation.Cidr = ipv4Network.Cidr;
                }
                else
                {
                    dbLocation.Net_Address_v4 = location.Net;
                    dbLocation.Cidr = location.Cidr;
                }

                // IP v6
                IPNetwork ipv6Network;
                if (IPNetwork.TryParse(location.Net_v6, location.Cidr_v6 ?? 0, out ipv6Network))
                {
                    dbLocation.Net_Address_v6 = ipv6Network.Network.ToString();
                    dbLocation.Cidr_v6 = ipv6Network.Cidr;
                }
                else
                {
                    dbLocation.Net_Address_v6 = location.Net_v6;
                    dbLocation.Cidr_v6 = location.Cidr_v6;
                }

                //Profile Group
                dbLocation.ProfileGroup = db.ProfileGroups.SingleOrDefault(r => r.Id == location.ProfileGroup.Id);

                // Region
                dbLocation.Region = location.Region != null && location.Region.Id != Guid.Empty
                    ? db.Regions.SingleOrDefault(r => r.Id == location.Region.Id)
                    : null;

                // City
                dbLocation.City = location.City != null && location.City.Id != Guid.Empty
                    ? db.Cities.SingleOrDefault(c => c.Id == location.City.Id)
                    : null;

                db.SaveChanges();
            }
        }

        public Location GetById(Guid id)
        {
            using (var db = GetDbContext())
            {
                var dbLocation = db.Locations
                    .Include(l => l.Region)
                    .Include(l => l.City)
                    .Include(l => l.ProfileGroup)
                    .SingleOrDefault(l => l.Id == id);
                return MapToLocation(dbLocation, true, true, true, true);
            }
        }

        public List<Location> GetAll()
        {
            using (var db = GetDbContext())
            {
                var dbLocations = db.Locations
                    .Include(l => l.City)
                    .Include(l => l.Region)
                    .Include(l => l.ProfileGroup)
                    .ToList();
                var locations = dbLocations
                    .Select(dbLocation => MapToLocation(dbLocation, true, true, true, true))
                    .OrderBy(l => l.Name).ToList();
                return locations;
            }
        }

        public void Delete(Guid id)
        {
            using (var db = GetDbContext())
            {
                var location = db.Locations.SingleOrDefault(l => l.Id == id);
                if (location != null)
                {
                    if (location.RegisteredSips != null)
                    {
                        location.RegisteredSips.Clear();
                    }

                    db.Locations.Remove(location);
                    db.SaveChanges();
                }
            }
        }

        public List<Location> FindLocations(string searchString)
        {
            using (var db = GetDbContext())
            {
                IQueryable<LocationEntity> dbLocationsQuery;

                // Sök på ip-adress om söksträngen kan tolkas som sådan
                IPAddress searchIpAddress;
                if (IPAddress.TryParse(searchString, out searchIpAddress) && !searchString.IsNumeric()
                ) // Sökning på bara siffror försöker vi inte tolka som IP-adress.
                {
                    if (searchIpAddress.AddressFamily == AddressFamily.InterNetwork) // V4
                    {
                        var locationEntities = db.Locations.Where(l => !string.IsNullOrEmpty(l.Net_Address_v4))
                            .ToList();

                        var networks = locationEntities
                            .Where(l => l.Cidr.HasValue)
                            .Select(l => new LocationNetwork(l.Id, l.Net_Address_v4, l.Cidr.Value))
                            .Where(n => n.Network != null)
                            .ToList();

                        var locationIds = networks
                            .Where(n => IPNetwork.Contains(n.Network, searchIpAddress))
                            .Select(n => n.Id);

                        dbLocationsQuery = db.Locations
                            .Include(l => l.ProfileGroup)
                            .Where(l => locationIds.Contains(l.Id))
                            .OrderBy(l => l.Cidr);

                    }
                    else if (searchIpAddress.AddressFamily == AddressFamily.InterNetworkV6) // V6
                    {
                        var networks = db.Locations
                            .Where(l => !string.IsNullOrEmpty(l.Net_Address_v6))
                            .Select(l => new LocationNetwork(l.Id, l.Net_Address_v6, l.Cidr_v6.Value))
                            .Where(n => n.Network != null)
                            .ToList();

                        var locationIds = networks
                            .Where(n => IPNetwork.Contains(n.Network, searchIpAddress))
                            .Select(n => n.Id);

                        dbLocationsQuery = db.Locations
                            .Where(l => locationIds.Contains(l.Id))
                            .OrderBy(l => l.Cidr_v6);
                    }
                    else
                    {
                        // Okänd addresstyp
                        dbLocationsQuery = Enumerable.Empty<LocationEntity>().AsQueryable();
                    }
                }
                else
                    // Annars, sök på namn
                {
                    dbLocationsQuery = db.Locations
                        .Where(l =>
                            l.CarrierConnectionId.ToLower().Contains(searchString.ToLower()) ||
                            l.Name.ToLower().Contains(searchString.ToLower()) ||
                            l.ShortName.ToLower().Contains(searchString.ToLower()))
                        .OrderBy(l => l.Name);
                }

                var dbLocations = dbLocationsQuery.ToList();
                return dbLocations.Select(dbLocation => MapToLocation(dbLocation, true, true, true, true)).ToList();
            }
        }

        public List<LocationNetwork> GetAllLocationNetworks()
        {
            using (var db = GetDbContext())
            {
                var locations = db.Locations.ToList();

                var networksV4 = locations
                    .Where(l => !string.IsNullOrEmpty(l.Net_Address_v4) && l.Cidr.HasValue)
                    .Select(l => new LocationNetwork(l.Id, l.Net_Address_v4, l.Cidr.Value))
                    .Where(l => l.Network != null)
                    .ToList();

                var networksV6 = locations
                    .Where(l => !string.IsNullOrEmpty(l.Net_Address_v6) && l.Cidr_v6.HasValue)
                    .Select(l => new LocationNetwork(l.Id, l.Net_Address_v6, l.Cidr_v6.Value))
                    .Where(l => l.Network != null)
                    .ToList();

                return networksV4.Concat(networksV6).ToList();
            }
        }

        private Location MapToLocation(LocationEntity dbLocation, bool includeProfiles, bool includeCity, bool includeRegion, bool includeProfileGroup)
        {
            if (dbLocation == null) return null;

            var location = new Location
            {
                Id = dbLocation.Id,
                Name = dbLocation.Name,
                ShortName = dbLocation.ShortName,
                Comment = dbLocation.Comment,
                CarrierConnectionId = dbLocation.CarrierConnectionId,
                Net = dbLocation.Net_Address_v4,
                Cidr = dbLocation.Cidr,
                Net_v6 = dbLocation.Net_Address_v6,
                Cidr_v6 = dbLocation.Cidr_v6,
                CreatedBy = dbLocation.CreatedBy,
                CreatedOn = dbLocation.CreatedOn,
                UpdatedBy = dbLocation.UpdatedBy,
                UpdatedOn = dbLocation.UpdatedOn,
                Region = includeRegion ? MapToRegion(dbLocation.Region) : null,
                City = includeCity ? MapToCity(dbLocation.City) : null,
                ProfileGroup = includeProfileGroup ? MapToProfileGroup(dbLocation.ProfileGroup) : null
            };

            return location;
        }

        private Region MapToRegion(RegionEntity dbRegion)
        {
            if (dbRegion == null) return null;

            return new Region
            {
                Id = dbRegion.Id,
                Name = dbRegion.Name,
                CreatedBy = dbRegion.CreatedBy,
                CreatedOn = dbRegion.CreatedOn,
                UpdatedBy = dbRegion.UpdatedBy,
                UpdatedOn = dbRegion.UpdatedOn
            };
        }

        private City MapToCity(CityEntity dbCity)
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
            };
        }

        private ProfileGroup MapToProfileGroup(ProfileGroupEntity ge)
        {
            return Mapper.Map<ProfileGroup>(ge);
        }

    }
}