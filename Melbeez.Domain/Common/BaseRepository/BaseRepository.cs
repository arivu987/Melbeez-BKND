using Melbeez.Domain.Common.BaseEntity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Melbeez.Domain.Common.BaseRepository
{
    public class BaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey> where TEntity : AuditableEntity<TKey>
    {
        protected readonly DbContext dbContext;
        public BaseRepository(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<TEntity> AddAsync(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Entity");
            }

            await dbContext.Set<TEntity>().AddAsync(entity);
            return entity;
        }
        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null || !entities.Any())
            {
                return;
            }

            foreach (var entity in entities)
            {
                await AddAsync(entity);
            }
        }
        public void Delete(TKey id)
        {
            TEntity entity = dbContext.Set<TEntity>().FirstOrDefault(x => x.Id.Equals(id));
            if (entity == null)
            {
                return;
            }

            Delete(entity);
        }
        public async Task DeleteAsync(TKey id)
        {
            TEntity entity = await dbContext.Set<TEntity>().FirstOrDefaultAsync(x => x.Id.Equals(id));
            if (entity == null)
            {
                return;
            }
            Delete(entity);
        }
        public void Delete(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Entity");
            }

            entity.IsDeleted = true;
            dbContext.Entry(entity).State = EntityState.Modified;
        }
        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                Delete(entity);
            }
        }
        public void Update(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Entity");
            }

            dbContext.Entry(entity).State = EntityState.Modified;
        }
        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                Update(entity);
            }
        }
        public void Remove(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Entity");
            }

            dbContext.Set<TEntity>().Remove(entity);
        }
        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            dbContext.Set<TEntity>().RemoveRange(entities);
        }
        public IQueryable<TEntity> GetQueryable()
        {
            return dbContext.Set<TEntity>().AsQueryable<TEntity>();
        }
        public IQueryable<TEntity> GetQueryable(Expression<Func<TEntity, bool>> predicate)
        {
            return dbContext.Set<TEntity>().Where(predicate);
        }
        public IEnumerable<TEntity> GetList()
        {
            return dbContext.Set<TEntity>().AsEnumerable<TEntity>();
        }
        public IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate)
        {
            return dbContext.Set<TEntity>().Where(predicate);
        }
        public async Task<TEntity> GetAsync(TKey id)
        {
            return await dbContext.Set<TEntity>().FirstOrDefaultAsync(x => x.Id.Equals(id));
        }
        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await dbContext.Set<TEntity>().FirstOrDefaultAsync(predicate);
        }
        public async Task<IEnumerable<TEntity>> GetListAsync()
        {
            return await dbContext.Set<TEntity>().ToListAsync();
        }
        public async Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await dbContext.Set<TEntity>().Where(predicate).ToListAsync();
        }
        public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await dbContext.Set<TEntity>().Where(predicate).CountAsync();
        }
        public async Task<long> GetLongCountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await dbContext.Set<TEntity>().Where(predicate).LongCountAsync();
        }
        //public async Task<JqDataTableResponse<TEntity>> GetDataTableResponse(IQueryable<TEntity> query, JqDataTableRequest model)
        //{
        //    if (model == null)
        //    {
        //        return new JqDataTableResponse<TEntity>
        //        {
        //            RecordsTotal = 0,
        //            RecordsFiltered = 0,
        //            Data = Enumerable.Empty<TEntity>()
        //        };
        //    }

        //    var recordsCount = await query.CountAsync();
        //    IQueryable<TEntity> paginateQuery;
        //    if (model.Length > -1)
        //    {
        //        paginateQuery = query.OrderBy(model.GetSortExpression()).Skip(model.Start).Take(model.Length);
        //    }
        //    else
        //    {
        //        paginateQuery = query.OrderBy(model.GetSortExpression());
        //    }

        //    var pagedResult = new JqDataTableResponse<TEntity>
        //    {
        //        RecordsTotal = recordsCount,
        //        RecordsFiltered = recordsCount,
        //        Data = await paginateQuery
        //            .ToListAsync()
        //    };

        //    return pagedResult;
        //}
        public async Task<bool> AnyAsync()
        {
            return await dbContext.Set<TEntity>().AnyAsync();
        }
        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await dbContext.Set<TEntity>().AnyAsync(predicate);
        }
    }
}