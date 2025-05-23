using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITransferDetailRepository
    {
        Task AddAsync(TransferDetail detail);
        Task<int> GetCommittedAsync(int warehouseId, int variantId);

        Task AddRangeAndSaveAsync(IEnumerable<TransferDetail> entities);
    }
}
