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
using System.Linq.Expressions;
using CCM.Core.Entities;
using CCM.Core.Interfaces;
using CCM.Core.Interfaces.Repositories;
using CCM.Data.Entities;
using CCM.Data.Entities.Base;

namespace CCM.Data.Repositories
{
    //public abstract class EntityRepositoryBase<T,TU> : BaseRepository, IRepository<T> 
    //    where T : CoreEntityBaseWithId
    //    where TU : EntityBase
    //{
    //    protected EntityRepositoryBase(IMemoryCacheLoader memoryCacheLoader) : base(memoryCacheLoader)
    //    {
    //    }

    //    public void Delete(Guid id)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public List<T> GetAll()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public T GetById(Guid id)
    //    {
    //        using (var db = GetDbContext())
    //        {
    //            Expression<Func<CityEntity, ICollection<LocationEntity>>> expression1 = c => c.Locations;
    //            Expression<Func<EntityBase, ICollection<LocationEntity>>> expression = expression1;

    //            EntityBase dbCity = db.Set<TU>()
    //                .Include(expression)
    //                .SingleOrDefault(g => g.Id == id);

    //            return MapToCoreObject(dbCity);
    //        }
    //    }

    //    protected abstract T MapToCoreObject(EntityBase dbObject);
       

    //    public void Save(T item)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}