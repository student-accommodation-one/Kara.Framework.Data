using Kara.Framework.Common;
using Kara.Framework.Common.Entity;
using Kara.Framework.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kara.Framework.Data
{
    public interface IEntityRepository<TEntity> where TEntity : class
    {
        TEntity FindById(object id, bool fromCache = false, bool refreshCache = false);
        TEntity Get(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] childRelations);
        List<TEntity> Find(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] childRelations);
        void Add(TEntity entity);
        void Update(TEntity entity);
        void Save(TEntity entity, Func<TEntity, int> primaryKeyProperty);
        void Save(TEntity entity, Func<TEntity, long> primaryKeyProperty);
        void Save(TEntity entity, Func<TEntity, Guid> primaryKeyProperty);
        void Delete(object id);
        void Delete(TEntity entity);
        IQueryable<TEntity> GetQueryable();
        IQueryable<TEntity> GetExpendableQueryable();
        int Count(Expression<Func<TEntity, bool>> filter);
        List<TEntity> GetAllFromCache(bool refreshCache = false);
        ActionResultCustom<TEntity> GetByPage(Expression<Func<TEntity, bool>> baseFilter, EntityDataRequest entityDataRequest, params Expression<Func<TEntity, object>>[] childRelations);
        string[] GetKeyNames();

    }

}
