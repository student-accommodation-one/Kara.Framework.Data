using Kara.Framework.Common.Enums;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kara.Framework.Data
{
    public class BlobRepository<TEntity> : IBlobRepository<TEntity> where TEntity : class
    {
        private IBlobUnitOfWork _unitOfWork;
        public BlobRepository(IBlobUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public MemoryStream FindById(TEntity entity, string containerRef, string reference)
        {
            var blockBlob = GetBlockBlobReference(containerRef, reference, false);
            var memoryStream = new MemoryStream();
            memoryStream.Position = 0;
            blockBlob.DownloadToStream(memoryStream);
            return memoryStream;
        }

        public Task SaveAsync(TEntity entity, string containerRef, string reference, MimeTypes contentType, byte[] byteArray)
        {
            var blockBlob = GetBlockBlobReference(containerRef, reference, true);
            blockBlob.Properties.ContentType = contentType.ToValue();
            blockBlob.Properties.CacheControl = "private, max-age=10800";
            blockBlob.ServiceClient.DefaultRequestOptions.ParallelOperationThreadCount = 20;
            var token = new CancellationToken();
            return blockBlob.UploadFromByteArrayAsync(byteArray, 0, byteArray.Length, token);
        }
        public bool Save(TEntity entity, string containerRef, string reference, MimeTypes contentType, byte[] byteArray)
        {
            var blockBlob = GetBlockBlobReference(containerRef, reference, true);
            blockBlob.Properties.ContentType = contentType.ToValue();
            blockBlob.Properties.CacheControl = "private, max-age=10800";
            blockBlob.ServiceClient.DefaultRequestOptions.ParallelOperationThreadCount = 20;
            blockBlob.UploadFromByteArray(byteArray, 0, byteArray.Length);
            return true;
        }

        private CloudBlockBlob GetBlockBlobReference(string containerRef, string reference, bool createIfNotExist)
        {
            var client = _unitOfWork.CloudStorageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerRef.ToLower());
            bool createResult;
            if (createIfNotExist)
                createResult = container.CreateIfNotExists();
            return container.GetBlockBlobReference(reference.ToLower());
        }

        public Task DeleteAsync(TEntity entity, string containerRef, string reference)
        {
            var blockBlob = GetBlockBlobReference(containerRef, reference, false);
            return blockBlob.DeleteIfExistsAsync();
        }

    }
}
