using Kara.Framework.Common;
using Kara.Framework.Common.Entity;
using Kara.Framework.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kara.Framework.Data
{
    public interface IBlobRepository<TEntity> where TEntity : class
    {
        MemoryStream FindById(TEntity entity, string containerRef, string reference);
        Task SaveAsync(TEntity entity, string containerRef, string reference, MimeTypes contentType, byte[] byteArray);
        bool Save(TEntity entity, string containerRef, string reference, MimeTypes contentType, byte[] byteArray);        
        Task DeleteAsync(TEntity entity, string containerRef, string reference);

    }

}
