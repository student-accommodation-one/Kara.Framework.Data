using Kara.Framework.Common;
using Kara.Framework.Common.Entity;
using Kara.Framework.Common.Enums;
using Kara.Framework.Common.Managers;
using Kara.Framework.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using LinqKit;
namespace Kara.Framework.Data
{
    public class EntityRepository<TEntity> : IEntityRepository<TEntity> where TEntity : class
    {

        private DbContext _context;
        private DbSet<TEntity> _dbSet;
        protected DbContext DbContext { get { return _context; } }
        protected DbSet<TEntity> DbSet { get { return _dbSet; } }

        public EntityRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public virtual TEntity FindById(object id, bool fromCache = false, bool refreshCache = false)
        {           
            if (fromCache)
            {
                TEntity result = null;
                var methodInfo = System.Reflection.MethodInfo.GetCurrentMethod();
                var cacheKey = string.Format("{0}.{1}", this.ToString(), methodInfo.Name);
                result = CacheManager.Get<TEntity>(cacheKey, id.ToString());
                if (result == null || refreshCache)
                    result = _dbSet.Find(id);
                if (result != null) CacheManager.Set<TEntity>(cacheKey,  result, null, id.ToString());
                return result;
            }
            return _dbSet.Find(id);
        }

        public virtual TEntity Get(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] childRelations)
        {
            IQueryable<TEntity> querySet = _dbSet;
            if (childRelations != null && childRelations.Length > 0)
            {
                foreach (var relation in childRelations)
                {
                    querySet = querySet.Include(relation);
                }
            }
            if (filter != null)
                return querySet.Where(filter).FirstOrDefault();
            else
                return querySet.FirstOrDefault();
        }

        public virtual List<TEntity> Find(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] childRelations)
        {
            IQueryable<TEntity> querySet = _dbSet;
            if (childRelations != null && childRelations.Length > 0)
            {
                foreach (var relation in childRelations)
                {
                    querySet = querySet.Include(relation);
                }
            }
            if (filter != null)
                return querySet.Where(filter).ToList();
            else
                return querySet.ToList(); 
        }

        public virtual int Count(Expression<Func<TEntity, bool>> filter)
        {
            IQueryable<TEntity> querySet = _dbSet;
            if (filter != null)
                return querySet.Count(filter);
            else
                return querySet.Count();
        }
        public virtual void Add(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void Update(TEntity entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete(object id)
        {
            var entity = _dbSet.Find(id);
            Delete(entity);
        }

        public virtual void Save(TEntity entity, Func<TEntity, int> primaryKeyProperty)
        {
            if (primaryKeyProperty.Invoke(entity) == 0)
                Add(entity);
            else
                Update(entity);
        }

        public virtual void Save(TEntity entity, Func<TEntity, long> primaryKeyProperty)
        {
            if (primaryKeyProperty.Invoke(entity) == 0)
                Add(entity);
            else
                Update(entity);
        }

        public virtual void Save(TEntity entity, Func<TEntity, Guid> primaryKeyProperty)
        {
            if (primaryKeyProperty.Invoke(entity) == Guid.Empty)
            {
                var property = typeof(TEntity).GetProperty(GetKeyNames()[0]);
                property.SetValue(entity, Guid.NewGuid());
                Add(entity);
            }
            else
                Update(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            _dbSet.Attach(entity);
            _dbSet.Remove(entity);
        }
        public virtual IQueryable<TEntity> GetQueryable()
        {
            IQueryable<TEntity> querySet = _dbSet;
            return querySet;
        }
        public IQueryable<TEntity> GetExpendableQueryable()
        {
            IQueryable<TEntity> querySet = _dbSet.AsExpandable<TEntity>();
            return querySet;
        }
        public List<TEntity> GetAllFromCache(bool refreshCache = false)
        {
            var methodInfo = System.Reflection.MethodInfo.GetCurrentMethod();
            var cacheKey = string.Format("{0}.{1}", this.ToString(), methodInfo.Name);

            var result = CacheManager.Get<List<TEntity>>(cacheKey);
            if (refreshCache || result == null)
            {
                result = _dbSet.ToList();
                CacheManager.Set<List<TEntity>>(cacheKey, result, null);
            }

            return result;
        }

        public Dictionary<int, TEntity> GetAllFromCacheAsDictionary(bool refreshCache = false)
        {
            var methodInfo = System.Reflection.MethodInfo.GetCurrentMethod();
            var cacheKey = string.Format("{0}.{1}", this.ToString(), methodInfo.Name);

            var result = CacheManager.Get<Dictionary<int, TEntity>>(cacheKey);
            if (refreshCache || result == null)
            {
                var primaryKeySelector = GetPrimaryKeySelector();
                result = _dbSet.ToDictionary<TEntity, int>(primaryKeySelector);
                CacheManager.Set<Dictionary<int, TEntity>>(cacheKey, result, null);
            }

            return result;
        }

        private Func<TEntity, int> GetPrimaryKeySelector()
        {
            string[] keys = GetKeyNames();
            var parameter = Expression.Parameter(typeof(TEntity));
            var memberExpression = Expression.Property(parameter, keys[0]);
            var conversion = Expression.Convert(memberExpression, typeof(int));
            var primaryKeySelector = Expression<TEntity>.Lambda<Func<TEntity, int>>(conversion, parameter).Compile();
            return primaryKeySelector;
        }

        public ActionResultCustom<TEntity> GetByPage(Expression<Func<TEntity, bool>> baseFilter, EntityDataRequest entityDataRequest, params Expression<Func<TEntity, object>>[] childRelations)
        {
            try
            {

                Func<TEntity, object> sortExpression = null;

                IQueryable<TEntity> querySet = _dbSet;

                if (childRelations != null && childRelations.Length > 0)
                {
                    foreach (var relation in childRelations)
                    {
                        querySet = querySet.Include(relation);
                    }                        
                }

                if (string.IsNullOrWhiteSpace(entityDataRequest.SortField))
                {
                   var keyNames = GetKeyNames();
                   entityDataRequest.SortField = keyNames[0];
                   entityDataRequest.SortDirection = SortDirections.DESC;
                }
               
                var parameter = Expression.Parameter(typeof(TEntity));
                var memberExpression = Expression.Property(parameter, entityDataRequest.SortField);
                Expression conversion = Expression.Convert(memberExpression, typeof(object));
                sortExpression = Expression<TEntity>.Lambda<Func<TEntity, object>>(conversion, parameter).Compile();

                var filterExpression = baseFilter;

                if (filterExpression != null)
                {
                    filterExpression = filterExpression.Expand();
               
                    var requestFilter = entityDataRequest.Filters.GetFilterExpression<TEntity>(baseFilter.Parameters[0]);
                    if (requestFilter != null)
                        filterExpression = PredicateHelper.And<TEntity>(filterExpression, requestFilter);
                }
                else filterExpression = entityDataRequest.Filters.GetFilterExpression<TEntity>(null);

                var result = new ActionResultCustom<TEntity>();
                if (filterExpression != null)
                    result.TotalRows = querySet.Where(filterExpression).Count();
                else
                    result.TotalRows = querySet.Count();

                if (result.TotalRows > 0)
                {
                    List<TEntity> queryResult;

                    if (filterExpression != null)
                    {
                        if (entityDataRequest.SortDirection == SortDirections.ASC)
                            queryResult = querySet.Where(filterExpression).OrderBy(sortExpression).Skip(entityDataRequest.Skip.Value).Take(entityDataRequest.PageSize.Value).ToList();
                        else
                            queryResult = querySet.Where(filterExpression).OrderByDescending(sortExpression).Skip(entityDataRequest.Skip.Value).Take(entityDataRequest.PageSize.Value).ToList();
                    }
                    else
                    {
                        if (entityDataRequest.SortDirection == SortDirections.ASC)
                            queryResult = querySet.OrderBy(sortExpression).Skip(entityDataRequest.Skip.Value).Take(entityDataRequest.PageSize.Value).ToList();
                        else
                            queryResult = querySet.OrderByDescending(sortExpression).Skip(entityDataRequest.Skip.Value).Take(entityDataRequest.PageSize.Value).ToList();
                    }
                    if (queryResult != null)
                        result.Results.AddRange(queryResult);

                }
                result.IsSuccess = true;
                return result;

            }
            catch (Exception ex)
            {
                //TODO: Logging repository exception
                throw;
            }
        }

        public string[] GetKeyNames()
        {
            var objectSet = ((IObjectContextAdapter)_context).ObjectContext.CreateObjectSet<TEntity>();
            string[] keyNames = objectSet.EntitySet.ElementType.KeyMembers.Select(k => k.Name).ToArray();
            return keyNames;
        }

    }
}
