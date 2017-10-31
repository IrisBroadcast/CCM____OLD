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
using CCM.Core.Entities;
using CCM.Core.Entities.Specific;
using CCM.Core.Helpers;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Repositories;
using CCM.Data.Entities;
using LazyCache;

namespace CCM.Data.Repositories
{
    public class CallHistoryRepository : BaseRepository, ICallHistoryRepository
    {
        #region CallHistory

        public CallHistoryRepository(IAppCache cache) : base(cache)
        {
        }

        public bool Save(CallHistory callHistory)
        {
            using (var db = GetDbContext())
            {
                CallHistoryEntity dbCallHistory = null;

                if (callHistory.CallHistoryId != Guid.Empty)
                {
                    dbCallHistory = db.CallHistories.SingleOrDefault(c => c.Id == callHistory.CallHistoryId);
                }

                if (dbCallHistory == null)
                {
                    dbCallHistory = new CallHistoryEntity { Id = Guid.NewGuid() };
                    callHistory.CallHistoryId = dbCallHistory.Id;
                    db.CallHistories.Add(dbCallHistory);
                }

                dbCallHistory.CallId = callHistory.CallId;
                dbCallHistory.DlgHashEnt = callHistory.DlgHashEnt;
                dbCallHistory.DlgHashId = callHistory.DlgHashId;
                dbCallHistory.Ended = callHistory.Ended;
                dbCallHistory.FromCodecTypeColor = callHistory.FromCodecTypeColor;
                dbCallHistory.FromCodecTypeId = callHistory.FromCodecTypeId;
                dbCallHistory.FromCodecTypeName = callHistory.FromCodecTypeName;
                dbCallHistory.FromComment = callHistory.FromComment;
                dbCallHistory.FromDisplayName = callHistory.FromDisplayName;
                dbCallHistory.FromId = callHistory.FromId;
                dbCallHistory.FromLocationId = callHistory.FromLocationId;
                dbCallHistory.FromLocationComment = callHistory.FromLocationComment;
                dbCallHistory.FromLocationName = callHistory.FromLocationName;
                dbCallHistory.FromLocationShortName = callHistory.FromLocationShortName;
                dbCallHistory.FromOwnerId = callHistory.FromOwnerId;
                dbCallHistory.FromOwnerName = callHistory.FromOwnerName;
                dbCallHistory.FromRegionId = callHistory.FromRegionId;
                dbCallHistory.FromRegionName = callHistory.FromRegionName;
                dbCallHistory.FromSip = callHistory.FromSip;
                dbCallHistory.FromTag = callHistory.FromTag;
                dbCallHistory.FromUserAgentHead = callHistory.FromUserAgentHead;
                dbCallHistory.FromUsername = callHistory.FromUsername;
                dbCallHistory.SipCallId = callHistory.SipCallId;
                dbCallHistory.Started = callHistory.Started;
                dbCallHistory.ToCodecTypeColor = callHistory.ToCodecTypeColor;
                dbCallHistory.ToCodecTypeId = callHistory.ToCodecTypeId;
                dbCallHistory.ToCodecTypeName = callHistory.ToCodecTypeName;
                dbCallHistory.ToComment = callHistory.ToComment;
                dbCallHistory.ToDisplayName = callHistory.ToDisplayName;
                dbCallHistory.ToId = callHistory.ToId;
                dbCallHistory.ToLocationId = callHistory.ToLocationId;
                dbCallHistory.ToLocationComment = callHistory.ToLocationComment;
                dbCallHistory.ToLocationName = callHistory.ToLocationName;
                dbCallHistory.ToLocationShortName = callHistory.ToLocationShortName;
                dbCallHistory.ToOwnerId = callHistory.ToOwnerId;
                dbCallHistory.ToOwnerName = callHistory.ToOwnerName;
                dbCallHistory.ToRegionId = callHistory.ToRegionId;
                dbCallHistory.ToRegionName = callHistory.ToRegionName;
                dbCallHistory.ToSip = callHistory.ToSip;
                dbCallHistory.ToTag = callHistory.ToTag;
                dbCallHistory.ToUserAgentHead = callHistory.ToUserAgentHead;
                dbCallHistory.ToUsername = callHistory.ToUsername;
                dbCallHistory.IsPhoneCall = callHistory.IsPhoneCall;

                db.SaveChanges();
                return true;
            }
        }

        public CallHistory GetById(Guid id)
        {
            using (var db = GetDbContext())
            {
                var dbCallHistory = db.CallHistories.AsNoTracking().SingleOrDefault(c => c.Id == id);
                return dbCallHistory == null ? null : MapCallHistory(dbCallHistory);
            }
        }

        public CallHistory GetCallHistoryByCallId(Guid callId)
        {
            using (var db = GetDbContext())
            {
                var dbCallHistory = db.CallHistories.SingleOrDefault(c => c.CallId == callId);
                return dbCallHistory == null ? null : MapCallHistory(dbCallHistory);
            }
        }

        public IList<CallHistory> GetCallHistories(DateTime startTime, DateTime endTime)
        {
            return GetFiltered(c => c.Started < endTime && c.Ended >= startTime);
        }

        public IList<CallHistory> GetCallHistoriesForRegion(DateTime startDate, DateTime endDate, Guid regionId)
        {
            return regionId == Guid.Empty ? 
                GetFiltered(c => c.Started < endDate && c.Ended >= startDate) : 
                GetFiltered(c => c.Started < endDate && c.Ended >= startDate && (c.FromRegionId == regionId || c.ToRegionId == regionId));
        }

        public IList<CallHistory> GetCallHistoriesForRegisteredSip(DateTime startDate, DateTime endDate, string sipId)
        {
            return GetFiltered(c => c.Started < endDate && c.Ended >= startDate && (c.FromSip == sipId || c.ToSip == sipId));
        }

        public IList<CallHistory> GetCallHistoriesForCodecType(DateTime startDate, DateTime endDate, Guid codecTypeId)
        {
            return codecTypeId == Guid.Empty
                ? GetFiltered(c => c.Started < endDate && c.Ended >= startDate)
                : GetFiltered(c => c.Started < endDate && c.Ended >= startDate && (c.FromCodecTypeId == codecTypeId || c.ToCodecTypeId == codecTypeId));
        }

        public IList<CallHistory> GetCallHistoriesForLocation(DateTime startDate, DateTime endDate, Guid locationId)
        {
            return locationId == Guid.Empty
                ? new List<CallHistory>()
                : GetFiltered(c => c.Started < endDate && c.Ended >= startDate && (c.FromLocationId == locationId || c.ToLocationId == locationId));
        }

        private IList<CallHistory> GetFiltered(Expression<Func<CallHistoryEntity, bool>> filterExpression)
        {
            using (var db = GetDbContext())
            {
                var dbCallHistories = db.CallHistories
                    .AsNoTracking()
                    .Where(filterExpression)
                    .ToList();
                return dbCallHistories.Select(MapCallHistory).ToList();
            }
        }

        private CallHistory MapCallHistory(CallHistoryEntity dbCallHistory)
        {
            return dbCallHistory == null ? null : new CallHistory()
            {
                CallHistoryId = dbCallHistory.Id,
                CallId = dbCallHistory.CallId,
                DlgHashEnt = dbCallHistory.DlgHashEnt,
                DlgHashId = dbCallHistory.DlgHashId,
                Ended = dbCallHistory.Ended,
                IsPhoneCall = dbCallHistory.IsPhoneCall,
                FromCodecTypeColor = dbCallHistory.FromCodecTypeColor,
                FromCodecTypeId = dbCallHistory.FromCodecTypeId,
                FromCodecTypeName = dbCallHistory.FromCodecTypeName,
                FromComment = dbCallHistory.FromComment,
                FromDisplayName = dbCallHistory.FromDisplayName,
                FromId = dbCallHistory.FromId,
                FromLocationId = dbCallHistory.FromLocationId,
                FromLocationComment = dbCallHistory.FromLocationComment,
                FromLocationName = dbCallHistory.FromLocationName,
                FromLocationShortName = dbCallHistory.FromLocationShortName,
                FromOwnerId = dbCallHistory.FromOwnerId,
                FromOwnerName = dbCallHistory.FromOwnerName,
                FromRegionId = dbCallHistory.FromRegionId,
                FromRegionName = dbCallHistory.FromRegionName,
                FromSip = dbCallHistory.FromSip,
                FromTag = dbCallHistory.FromTag,
                FromUserAgentHead = dbCallHistory.FromUserAgentHead,
                FromUsername = dbCallHistory.FromUsername,
                SipCallId = dbCallHistory.SipCallId,
                Started = dbCallHistory.Started,
                ToCodecTypeColor = dbCallHistory.ToCodecTypeColor,
                ToCodecTypeId = dbCallHistory.ToCodecTypeId,
                ToCodecTypeName = dbCallHistory.ToCodecTypeName,
                ToComment = dbCallHistory.ToComment,
                ToDisplayName = dbCallHistory.ToDisplayName,
                ToId = dbCallHistory.ToId,
                ToLocationId = dbCallHistory.ToLocationId,
                ToLocationComment = dbCallHistory.ToLocationComment,
                ToLocationName = dbCallHistory.ToLocationName,
                ToLocationShortName = dbCallHistory.ToLocationShortName,
                ToOwnerId = dbCallHistory.ToOwnerId,
                ToOwnerName = dbCallHistory.ToOwnerName,
                ToRegionId = dbCallHistory.ToRegionId,
                ToRegionName = dbCallHistory.ToRegionName,
                ToSip = dbCallHistory.ToSip,
                ToTag = dbCallHistory.ToTag,
                ToUserAgentHead = dbCallHistory.ToUserAgentHead,
                ToUsername = dbCallHistory.ToUsername
            };
        }

        #endregion

        #region OldCall
        public IList<OldCall> GetOldCalls(int callCount, bool anonymize)
        {
            using (var db = GetDbContext())
            {
                var dbCalls = db.CallHistories.OrderByDescending(callHistory => callHistory.Ended).Take(callCount)
                    .ToList();
                return dbCalls.Select(dbCall => MapToOldCall(dbCall, anonymize)).ToList();
            }
        }

        public IList<OldCall> GetOldCallsFiltered(string region, string codecType, string sipAddress, string searchString, bool anonymize, bool onlyPhoneCalls, int callCount)
        {
            using (var db = GetDbContext())
            {
                var query = db.CallHistories.AsQueryable();

                if (!string.IsNullOrEmpty(region))
                {
                    query = query.Where(ch => ch.FromRegionName == region || ch.ToRegionName == region);
                }

                if (!string.IsNullOrEmpty(codecType))
                {
                    query = query.Where(ch => ch.FromCodecTypeName == codecType || ch.ToCodecTypeName == codecType);
                }

                if (!string.IsNullOrEmpty(sipAddress))
                {
                    query = query.Where(ch => ch.FromSip.Contains(sipAddress) || ch.ToSip.Contains(sipAddress));
                }

                if (onlyPhoneCalls)
                {
                    query = query.Where(ch => ch.IsPhoneCall);
                }

                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(ch =>
                        ch.FromDisplayName.Contains(searchString) ||
                        ch.ToDisplayName.Contains(searchString) ||
                        ch.FromSip.Contains(searchString) ||
                        ch.ToSip.Contains(searchString) ||
                        ch.FromLocationName.Contains(searchString) ||
                        ch.ToLocationName.Contains(searchString) ||
                        ch.FromLocationShortName.Contains(searchString) ||
                        ch.ToLocationShortName.Contains(searchString) ||
                        ch.FromUsername.Contains(searchString) ||
                        ch.ToUsername.Contains(searchString)
                    );
                }

                var dbCalls = query.OrderByDescending(callHistory => callHistory.Ended).Take(callCount).ToList();
                return dbCalls.Select(dbCall => MapToOldCall(dbCall, anonymize)).ToList();
            }
        }

        private OldCall MapToOldCall(CallHistoryEntity dbCall, bool anonymize)
        {
            return new OldCall
            {
                CallId = GuidString(dbCall.CallId),
                Started = dbCall.Started.ToLocalTime(),
                Ended = dbCall.Ended.ToLocalTime(),
                Duration = dbCall.Ended.Subtract(dbCall.Started).ToString(@"dd\d\ hh\:mm\:ss"),
                IsPhoneCall = dbCall.IsPhoneCall,
                FromId = GuidString(dbCall.FromId),
                FromSip = anonymize ? DisplayNameHelper.AnonymizePhonenumber(dbCall.FromUsername) : dbCall.FromUsername,
                FromCodecTypeColor = dbCall.FromCodecTypeColor,
                FromCodecTypeName = dbCall.FromCodecTypeName,
                FromComment = dbCall.FromComment,
                FromDisplayName = anonymize ? DisplayNameHelper.AnonymizeDisplayName(dbCall.FromDisplayName) : dbCall.FromDisplayName,
                FromLocationName = dbCall.FromLocationName,
                FromLocationShortName = dbCall.FromLocationShortName,
                FromRegionName = dbCall.FromRegionName,
                ToId = GuidString(dbCall.ToId),
                ToSip = anonymize ? DisplayNameHelper.AnonymizePhonenumber(dbCall.ToUsername) : dbCall.ToUsername,
                ToCodecTypeColor = dbCall.ToCodecTypeColor,
                ToCodecTypeName = dbCall.ToCodecTypeName,
                ToComment = dbCall.ToComment,
                ToDisplayName = anonymize ? DisplayNameHelper.AnonymizeDisplayName(dbCall.ToDisplayName) : dbCall.ToDisplayName,
                ToLocationName = dbCall.ToLocationName,
                ToLocationShortName = dbCall.ToLocationShortName,
                ToRegionName = dbCall.ToRegionName
            };
        }

        private string GuidString(Guid guid) { return guid == Guid.Empty ? string.Empty : guid.ToString(); }

        #endregion


    }
}