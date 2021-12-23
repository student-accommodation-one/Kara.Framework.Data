using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kara.Framework.Data
{
    public interface IBlobUnitOfWork
    {
        CloudStorageAccount CloudStorageAccount { get; }
        void Dispose();
        void Dispose(bool disposing);

    }
}
