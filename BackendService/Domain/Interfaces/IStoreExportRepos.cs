using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
  
        public interface IStoreExportRepos
        {
            void Add(StoreExportStoreDetail storeExport);
            Task SaveChangesAsync();

        Task AddRangeAndSaveAsync(IEnumerable<StoreExportStoreDetail> entities);
    }

   
   
}
