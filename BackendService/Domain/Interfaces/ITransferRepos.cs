using Domain.DTO.Response;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITransferRepos
    {
      
        void Add(Transfer transfer);
        Task SaveChangesAsync();
        // Thêm các phương thức khác nếu cần (ví dụ: GetById, Update, Delete)
        Task<PaginatedResponseDTO<TransferDto>> GetAllWithPagingAsync(
           int page,
           int pageSize,
           string? filter,
           CancellationToken cancellationToken = default
       );
        Task<Transfer?> GetByIdWithDetailsAsync(int transferId);
        public Task<Transfer> GetJSONTransferOrderById(int id);

        Task AddAsync(Transfer transfer);


          Task UpdateAsync(Transfer entity);



          Task AddRangeAsync(IEnumerable<TransferDetail> entities);
       
    }


}
