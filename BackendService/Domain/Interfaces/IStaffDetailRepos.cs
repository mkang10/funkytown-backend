using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IStaffDetailRepository
    {
        Task<StaffDetail?> GetByIdAsync(int staffDetailId);
        Task<StaffDetail?> GetByAccountIdAsync(int accountId);
        Task<IEnumerable<StaffNameDto>> GetAllStaffNamesAsync(int warehouseId);
        Task<int> GetAccountIdByStaffIdAsync(int staffId);


    }
}