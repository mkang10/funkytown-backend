using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class TransferDetailRepository : ITransferDetailRepository
    {
        private readonly FtownContext _context;
        public TransferDetailRepository(FtownContext context) => _context = context;

        public async Task AddAsync(TransferDetail detail)
            => await _context.TransferDetails.AddAsync(detail);

        public async Task<int> GetCommittedAsync(int warehouseId, int variantId)
            => await _context.TransferDetails
                .Where(td => td.VariantId == variantId && td.TransferOrder.Status != "Done")
                .SumAsync(td => td.Quantity);

        public async Task AddRangeAndSaveAsync(IEnumerable<TransferDetail> entities)
        {
            await _context.Set<TransferDetail>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
    }
}
