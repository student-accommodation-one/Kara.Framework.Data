using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kara.Framework.Data
{
    public class BlobUnitOfWork : IBlobUnitOfWork, IDisposable
    {
        private CloudStorageAccount _cloudStorageAccount;
        public CloudStorageAccount CloudStorageAccount { get { return _cloudStorageAccount; } }

        private bool _disposed;

        public BlobUnitOfWork(string connectionString)
        {
            _cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
        }

        public BlobUnitOfWork()
        {
            _cloudStorageAccount = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }  

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                {
                    //Do nothing
                }
                   

            _disposed = true;
        }
    } 
}
