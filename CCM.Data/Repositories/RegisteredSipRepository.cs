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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using CCM.Core.Entities;
using CCM.Core.Entities.Specific;
using CCM.Core.Enums;
using CCM.Core.Helpers;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Managers;
using CCM.Core.Interfaces.Repositories;
using CCM.Core.Kamailio;
using CCM.Data.Entities;
using CCM.Data.Helpers;
using LazyCache;
using NLog;

namespace CCM.Data.Repositories
{
    public class RegisteredSipRepository : BaseRepository, IRegisteredSipRepository
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IMetaRepository _metaRepository;
        public ILocationManager LocationManager { get; set; }
        public ISettingsManager SettingsManager { get; set; }

        public RegisteredSipRepository(ISettingsManager settingsManager, ILocationManager locationManager, IMetaRepository metaRepository, IAppCache cache)
            : base(cache)
        {
            _metaRepository = metaRepository;
            LocationManager = locationManager;
            SettingsManager = settingsManager;
        }

        public bool DeleteRegisteredSip(Guid guid)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var regSip = db.RegisteredSips.SingleOrDefault(r => r.Id == guid);
                    if (regSip != null)
                    {
                        db.RegisteredSips.Remove(regSip);
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when deleting RegisteredSip with id {0}", guid);
                return false;
            }
        }

        public KamailioMessageHandlerResult UpdateRegisteredSip(RegisteredSip registeredSip)
        {
            // Returnerar värde som indikerar om 
            // 1. Kodaren lagts till
            // 2. Kodaren fanns sedan tidigare men har på relevant sätt uppdaterad information
            // 3. Kodaren fanns sedan tidigare men registreringen i identisk. = NothingChanged

            if (registeredSip == null)
            {
                return KamailioMessageHandlerResult.NothingChanged;
            }

            try
            {
                using (var db = GetDbContext())
                {
                    var dbSip = db.RegisteredSips.SingleOrDefault(rs => rs.SIP == registeredSip.SIP);

                    if (dbSip == null)
                    {
                        if (registeredSip.Expires == 0)
                        {
                            // Avregistrering av ej registrerad kodare. Gör ingenting.
                            return KamailioMessageHandlerResult.NothingChanged;
                        }

                        dbSip = new Entities.RegisteredSipEntity { Id = Guid.NewGuid(), Updated = DateTime.UtcNow };
                        db.RegisteredSips.Add(dbSip);
                        //registeredSip.Id = dbSip.Id;
                    }
                    //else
                    //{
                    //registeredSip.Id = dbSip.Id;
                    //}

                    // Matcha och mappa
                    var user = db.Users.FirstOrDefault(u => u.UserName == registeredSip.Username);
                    var userId = user != null ? user.Id : (Guid?)null;

                    var userAgentId = GetUserAgentId(db, registeredSip.UserAgentHead);
                    var locationId = LocationManager.GetLocationIdByIp(registeredSip.IP);

                    registeredSip.Updated = registeredSip.Expires == 0
                        ? DateTime.UtcNow.AddSeconds(-SettingsManager.MaxRegistrationAge) // Expire immediately
                        : DateTime.UtcNow;

                    dbSip.UserAgentId = userAgentId;
                    dbSip.Location_LocationId = locationId != Guid.Empty ? locationId : (Guid?)null;
                    dbSip.User_UserId = userId;

                    dbSip.SIP = registeredSip.SIP;
                    dbSip.UserAgentHead = registeredSip.UserAgentHead;
                    dbSip.Username = registeredSip.Username;
                    dbSip.DisplayName = registeredSip.DisplayName;
                    dbSip.IP = registeredSip.IP;
                    dbSip.Port = registeredSip.Port;
                    dbSip.ServerTimeStamp = registeredSip.ServerTimeStamp;
                    dbSip.Updated = registeredSip.Updated;
                    dbSip.Expires = registeredSip.Expires;

                    var changeStatus = GetChangeStatus(db, dbSip);
                    db.SaveChanges();

                    return new KamailioMessageHandlerResult { ChangedObjectId = dbSip.Id, ChangeStatus = changeStatus };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error while updating registered sip {0}", registeredSip.SIP);
                return KamailioMessageHandlerResult.NothingChanged;
            }
        }

        public KamailioMessageHandlerResult DeleteRegisteredSip(string sipAddress)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return KamailioMessageHandlerResult.NothingChanged;
            }

            sipAddress = sipAddress.ToLower();

            try
            {
                using (var db = GetDbContext())
                {
                    // Registrations should be unique based on sipAddress, but to be safe we don't assume this.
                    var dbSips = db.RegisteredSips.Where(rs => rs.SIP == sipAddress).OrderByDescending(rs => rs.Updated);

                    if (!dbSips.Any())
                    {
                        return new KamailioMessageHandlerResult { ChangeStatus = KamailioMessageChangeStatus.NothingChanged, SipAddress = sipAddress };
                    }

                    db.RegisteredSips.RemoveRange(dbSips);
                    db.SaveChanges();
                    return new KamailioMessageHandlerResult { ChangeStatus = KamailioMessageChangeStatus.CodecRemoved, ChangedObjectId = dbSips.First().Id, SipAddress = sipAddress };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error while unregistering codec with sip address {0}", sipAddress);
                return KamailioMessageHandlerResult.NothingChanged;
            }
        }

        private KamailioMessageChangeStatus GetChangeStatus(CcmDbContext cxt, Entities.RegisteredSipEntity dbSip)
        {
            var entry = cxt.Entry(dbSip);

            if (entry.State == EntityState.Added || CodecWasExpired(entry))
            {
                return KamailioMessageChangeStatus.CodecAdded;
            }

            if (entry.State == EntityState.Modified && dbSip.Expires == 0)
            {
                return KamailioMessageChangeStatus.CodecRemoved;
            }

            if (entry.State == EntityState.Modified && HasRelevantChange(entry))
            {
                return KamailioMessageChangeStatus.CodecUpdated;
            }

            return KamailioMessageChangeStatus.NothingChanged;
        }

        /// <summary>
        /// Returnerar true om kodare i databasen indikerar att den var utgången.
        /// </summary>
        private bool CodecWasExpired(DbEntityEntry<Entities.RegisteredSipEntity> entry)
        {
            var maxRegistrationAge = SettingsManager.MaxRegistrationAge;
            var expireTime = DateTime.UtcNow.AddSeconds(-maxRegistrationAge);
            // Using lambda for safer refactoring
            return entry.OriginalValues.GetValue<DateTime>(nameof(entry.Entity.Updated)) < expireTime;
        }

        private bool HasRelevantChange(DbEntityEntry<Entities.RegisteredSipEntity> entry)
        {
            var changedProperties = GetChangedProperties(entry);
            // Remove not relevant properties
            changedProperties.Remove(nameof(entry.Entity.Updated));
            changedProperties.Remove(nameof(entry.Entity.ServerTimeStamp));
            changedProperties.Remove(nameof(entry.Entity.Expires));

            return changedProperties.Any();
        }

        private IList<string> GetChangedProperties(DbEntityEntry entry)
        {
            if (entry.State != EntityState.Modified)
                return new List<string>();

            return entry.OriginalValues.PropertyNames
                .Where(propertyName => !Equals(entry.OriginalValues[propertyName], entry.CurrentValues[propertyName]))
                .ToList();
        }

        public RegisteredSip Single(Expression<Func<Entities.RegisteredSipEntity, bool>> expression)
        {
            using (var db = GetDbContext())
            {
                Entities.RegisteredSipEntity dbRegisteredSip = db.RegisteredSips.SingleOrDefault(expression);

                return dbRegisteredSip == null
                    ? null
                    : new RegisteredSip
                    {
                        Id = dbRegisteredSip.Id,
                        DisplayName = dbRegisteredSip.DisplayName,
                        Expires = dbRegisteredSip.Expires,
                        IP = dbRegisteredSip.IP,
                        Port = dbRegisteredSip.Port,
                        SIP = dbRegisteredSip.SIP,
                        ServerTimeStamp = dbRegisteredSip.ServerTimeStamp,
                        Updated = dbRegisteredSip.Updated,
                        UserAgentHead = dbRegisteredSip.UserAgentHead,
                        Username = dbRegisteredSip.Username,
                    };
            }
        }

        public List<RegisteredSipDto> GetCachedRegisteredSips()
        {
            using (new TimeMeasurer("GetRegisteredSips from database."))
            {
                DateTime maxAge = DateTime.UtcNow.AddSeconds(-SettingsManager.MaxRegistrationAge);

                List<MetaType> metaList = _metaRepository.GetAll();
                using (var db = GetDbContext())
                {
                    if (log.IsTraceEnabled)
                    {
                        db.Database.Log = s => { log.Trace("EF Log GetCachedRegisteredSips: {0}", s); };
                    }

                    IQueryable<Entities.RegisteredSipEntity> query = db.RegisteredSips
                            .Include(rs => rs.Location)
                            .Include(rs => rs.Location.Region)
                            .Include(rs => rs.Location.City)
                            .Include(rs => rs.Location.ProfileGroup)
                            .Include(rs => rs.Location.ProfileGroup.OrderedProfiles)
                            .Include(rs => rs.User)
                            .Include(rs => rs.User.Owner)
                            .Include(rs => rs.User.CodecType)
                            .Include(rs => rs.UserAgent)
                            .Include(rs => rs.UserAgent.OrderedProfiles)
                            .Where(r => r.Updated >= maxAge); // && r.User.UserType == UserType.SIP);

                    var dbResult = query.ToList();
                    var list = dbResult.Select(sip => MapToRegisteredSipDto(sip, metaList)).ToList();
                    AddCallInfoToCachedRegisteredSip(db, list);

                    return list;
                }
            }
        }

        private void AddCallInfoToCachedRegisteredSip(CcmDbContext db, IEnumerable<RegisteredSipDto> registeredSips)
        {
            var sipDomain = SettingsManager.SipDomain;

            var dbCalls = db.Calls
                .Include(c => c.FromSip)
                .Include(c => c.FromSip.Location)
                .Include(c => c.FromSip.UserAgent)
                .Include(c => c.ToSip)
                .Include(c => c.ToSip.Location)
                .Include(c => c.ToSip.UserAgent)
                .Where(call => !call.Closed)
                .ToList();

            foreach (var item in registeredSips)
            {
                var sip = item;
                var call = dbCalls.FirstOrDefault(c => c.ToUsername == sip.Sip || c.FromUsername == sip.Sip);
                if (call == null)
                {
                    sip.InCall = false;
                    continue;
                }
                sip.InCall = true;
                sip.IsPhoneCall = sip.IsPhoneCall;
                sip.CallStartedAt = call.Started;

                if (call.FromUsername == sip.Sip)
                {
                    sip.InCallWithId = GuidHelper.GuidString(call.ToId);
                    sip.InCallWithName =
                        CallDisplayNameHelper.GetDisplayName(call.ToSip, call.ToDisplayName, call.ToUsername,
                            sipDomain);
                    sip.InCallWithSip = call.ToUsername;
                    sip.InCallWithLocation = call.ToSip != null && call.ToSip.Location != null
                        ? call.ToSip.Location.Name
                        : string.Empty;
                    sip.InCallWithHasGpo = call.ToSip != null && call.ToSip.UserAgent != null &&
                                           !string.IsNullOrEmpty(call.ToSip.UserAgent.GpoNames);
                    sip.IsCallingPart = true;
                }
                else
                {
                    sip.InCallWithId = GuidHelper.GuidString(call.FromId);
                    sip.InCallWithName = CallDisplayNameHelper.GetDisplayName(call.FromSip, call.FromDisplayName,
                        call.FromUsername, sipDomain);
                    sip.InCallWithSip = call.FromUsername;
                    sip.InCallWithLocation = call.FromSip != null && call.FromSip.Location != null
                        ? call.FromSip.Location.Name
                        : string.Empty;
                    sip.InCallWithHasGpo = call.FromSip != null && call.FromSip.UserAgent != null &&
                                           !string.IsNullOrEmpty(call.FromSip.UserAgent.GpoNames);
                    sip.IsCallingPart = false;
                }
            }
        }

        private RegisteredSipDto MapToRegisteredSipDto(Entities.RegisteredSipEntity dbSip, List<MetaType> metaList)
        {
            if (dbSip == null)
                return null;

            var regSip = new RegisteredSipDto
            {
                Updated = dbSip.Updated,
                Id = dbSip.Id,
                Sip = dbSip.SIP,
                DisplayName = dbSip.DisplayName,
                UserDisplayName = dbSip.User != null ? dbSip.User.DisplayName : string.Empty,
                IpAddress = dbSip.IP,
                UserAgentHeader = dbSip.UserAgentHead,
                UserName = dbSip.Username,
                RegionName = dbSip.Location != null && dbSip.Location.Region != null ? dbSip.Location.Region.Name : string.Empty,
                CodecTypeName = dbSip.User != null && dbSip.User.CodecType != null ? dbSip.User.CodecType.Name : string.Empty,
                CodecTypeColor = dbSip.User != null && dbSip.User.CodecType != null ? dbSip.User.CodecType.Color : string.Empty,
                Comment = dbSip.User != null ? dbSip.User.Comment : string.Empty,
                Image = dbSip.UserAgent != null ? dbSip.UserAgent.Image : string.Empty,
                Api = dbSip.UserAgent != null ? dbSip.UserAgent.Api : string.Empty,
                LocationName = dbSip.Location != null ? dbSip.Location.Name : string.Empty,
                LocationShortName = dbSip.Location != null ? dbSip.Location.ShortName : string.Empty,

                // Filtering properties. Not currently used.
                //OwnerName = dbSip.User != null && dbSip.User.Owner != null ? dbSip.User.Owner.Name : string.Empty,
                //CityName = dbSip.Location != null && dbSip.Location.City != null ? dbSip.Location.City.Name : string.Empty,
                //UserAgentName = dbSip.UserAgent != null ? dbSip.UserAgent.Name : string.Empty,

                HasGpo = dbSip.UserAgent != null && !string.IsNullOrEmpty(dbSip.UserAgent.GpoNames),
            };

            var locationProfiles = dbSip.Location != null ? dbSip.Location.ProfileGroup.OrderedProfiles.OrderBy(op => op.SortIndex).Select(p => p.Profile.Name) : Enumerable.Empty<string>();
            var userAgentProfiles = dbSip.UserAgent != null ? dbSip.UserAgent.OrderedProfiles.OrderBy(op => op.SortIndex).Select(p => p.Profile.Name) : Enumerable.Empty<string>();
            // The sort order is most important as it decides the order of recommended profiles in the Discovery service.
            // Sorting is based on the sort order of the location.
            regSip.Profiles = locationProfiles.Intersect(userAgentProfiles).ToList();

            regSip.MetaData = GetMetaData(metaList, dbSip);
            return regSip;
        }

        private List<KeyValuePair<string, string>> GetMetaData(List<MetaType> metaList, Entities.RegisteredSipEntity sip)
        {
            metaList = metaList ?? new List<MetaType>();

            var userAgentMetaDataList = metaList
                .Select(meta => new KeyValuePair<string, string>(meta.Name, MetadataHelper.GetPropertyValue(sip, meta.FullPropertyName)))
                .Where(m => !string.IsNullOrEmpty(m.Value))
                .ToList();

            return userAgentMetaDataList;
        }

        private Guid? GetUserAgentId(CcmDbContext db, string userAgent)
        {
            userAgent = (userAgent ?? string.Empty).Trim();

            var allUserAgents = db.UserAgents
                .Where(ua => !string.IsNullOrEmpty(ua.Identifier))
                .Select(ua => new UserAgentInfo
                {
                    UserAgentId = ua.Id,
                    MatchType = ua.MatchType,
                    Identifier = ua.Identifier
                })
                .ToList();

            var dbUserAgent = allUserAgents.FirstOrDefault(u => u.MatchType == MatchType.BeginsWith && userAgent.StartsWith(u.Identifier));

            if (dbUserAgent != null)
            {
                return dbUserAgent.UserAgentId;
            }

            dbUserAgent = allUserAgents.FirstOrDefault(u => u.MatchType == MatchType.EndsWith && userAgent.EndsWith(u.Identifier));

            if (dbUserAgent != null)
            {
                return dbUserAgent.UserAgentId;
            }

            dbUserAgent = allUserAgents.FirstOrDefault(u => u.MatchType == MatchType.Contains && userAgent.Contains(u.Identifier));

            return dbUserAgent?.UserAgentId;
        }
    }

    internal class UserAgentInfo
    {
        public Guid UserAgentId { get; set; }
        public string Identifier { get; set; }
        public MatchType MatchType { get; set; }
    }
}