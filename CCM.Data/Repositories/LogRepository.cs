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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CCM.Core.Entities;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Repositories;
using CCM.Data.Entities;
using LazyCache;

namespace CCM.Data.Repositories
{
    public class LogRepository : BaseRepository, ILogRepository
    {
        public LogRepository(IAppCache cache) : base(cache)
        {
        }

        public async Task<List<Log>> GetLastAsync(int nrOfRows, string application, DateTime? startTime, DateTime? endTime, int minLevel, string search, Guid activityId)
        {
            using (var db = GetDbContext())
            {
                List<LogEntity> dbLogRows;

                if (activityId != Guid.Empty)
                {
                    dbLogRows = await db.Logs
                        .Where(l => l.ActivityId == activityId)
                        .OrderByDescending(l => l.Id)
                        .ToListAsync();
                }
                else
                {
                    IQueryable<LogEntity> dbLogQuery = db.Logs.Where(l => l.LevelValue >= minLevel);

                    if (!string.IsNullOrEmpty(application))
                    {
                        dbLogQuery = dbLogQuery.Where(r => r.Application == application);
                    }

                    if (startTime != null)
                    {
                        dbLogQuery = dbLogQuery.Where(r => r.Date >= startTime.Value);
                    }

                    if (endTime != null)
                    {
                        dbLogQuery = dbLogQuery.Where(r => r.Date <= endTime.Value);
                    }

                    if (!string.IsNullOrEmpty(search))
                    {
                        dbLogQuery = dbLogQuery.Where(r => r.Message.Contains(search));
                    }

                    dbLogRows = await dbLogQuery
                        .OrderByDescending(l => l.Id)
                        .Take(nrOfRows)
                        .ToListAsync();
                }

                return dbLogRows.Select(r => new Log
                {
                    Id = r.Id,
                    Date = r.Date,
                    Level = r.Level,
                    Callsite = r.Callsite,
                    Message = r.Message,
                    Exception = r.Exception,
                    Application = r.Application,
                    ActivityId = r.ActivityId
                }).ToList();
            }
        }

        public async Task<IList<LogInfo>> GetLogInfoAsync()
        {
            using (var db = GetDbContext())
            {
                var logInfo = await db.Logs
                    .GroupBy(l => l.Application)
                    .Select(g => new LogInfo() { Application = g.Key, Count = g.Count(), MinDate = g.Min(x => x.Date) })
                    .ToListAsync();

                return logInfo;
            }
        }

        public void DeleteOldest(int nrOfRowsToDelete = 100)
        {
            using (var db = GetDbContext())
            {
                var sql = string.Format("DELETE from Log where Id IN (SELECT TOP {0} Id from Log ORDER BY Date);",
                    nrOfRowsToDelete);
                db.Database.ExecuteSqlCommand(sql);
            }
        }
    }
}