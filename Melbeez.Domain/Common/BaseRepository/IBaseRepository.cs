using Melbeez.Domain.Common.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Melbeez.Domain.Common.BaseRepository
{
    public interface IBaseRepository<TEntity, TKey> where TEntity : class, IAuditableEntity<TKey>
    {
        Task<TEntity> AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);
        /// <summary>
        /// It soft deletion the entity with <see cref="IEntity{TKey}.Id"/> by marking <see cref="IEntity{TKey}.IsDeleted"/> property to <see cref="bool.TrueString"/>
        /// </summary>
        /// <param name="id"></param>
        void Delete(TKey id);
        Task DeleteAsync(TKey id);
        /// <summary>
        /// It soft deletion the entity by marking <see cref="IEntity{TKey}.IsDeleted"/> property to <see cref="bool.TrueString"/>
        /// </summary>
        /// <param name="entity"></param>
        void Delete(TEntity entity);
        /// <summary>
        /// It soft deletion the enities by marking <see cref="IEntity{TKey}.IsDeleted"/> property to <see cref="bool.TrueString"/>
        /// </summary>
        /// <param name="entities"></param>
        void DeleteRange(IEnumerable<TEntity> entities);
        /// <summary>
        /// It removes the entity permanently, should be used wisely.
        /// 
        /// Review the use case and use <see cref="IRepository{TEntity, TKey}.Delete(TEntity)" /> method.
        /// </summary>
        /// <param name="entity"></param>
        void Remove(TEntity entity);

        /// <summary>
        /// It removes the entity permanently, should be used wisely.
        /// 
        /// Review the use case and use <see cref="IRepository{TEntity, TKey}.DeleteRange(TEntity)" /> method.
        /// </summary>
        /// <param name="entity"></param>
        void RemoveRange(IEnumerable<TEntity> entities);
        IQueryable<TEntity> GetQueryable();
        IQueryable<TEntity> GetQueryable(Expression<Func<TEntity, bool>> predicate);
        IEnumerable<TEntity> GetList();
        IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetListAsync();
        Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate);
        Task<long> GetLongCountAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> GetAsync(TKey id);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate);
        //TODO
        //Task<JqDataTableResponse<TEntity>> GetDataTableResponse(IQueryable<TEntity> query, JqDataTableRequest model);
        Task<bool> AnyAsync();
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
    }
}