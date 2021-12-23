using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kara.Framework.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private DbContext _context;

        public DbContext DbContext
        {
            get { return _context; }
        }

        private bool _disposed;

        public UnitOfWork(DbContext context)
        {
            _context = context;
        }

        public UnitOfWork()
        {
            _context = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                    _context.Dispose();

            _disposed = true;
        }
        
        public string GetRepositoryNameForType(Type type, string typeNamespace, string repositoryAssembly)
        {
            return typeNamespace + type.Name + "Repository," + repositoryAssembly;
        }
    }
 
}
