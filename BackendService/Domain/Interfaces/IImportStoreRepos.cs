using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IImportStoreRepos
    {
        void Add(ImportStoreDetail importStore);
        Task SaveChangesAsync();
        Task UpdateRangeAsync(IEnumerable<ImportStoreDetail> entities);
        Task AddRangeAsync(IEnumerable<ImportStoreDetail> details);
       
        Task UpdateAsync(ImportStoreDetail entity);


    }
}
