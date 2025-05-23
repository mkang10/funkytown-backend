using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IWarehouseStaffRepos
    {
        Task<List<WarehouseStaff>> GetByWarehouseAndRoleAsync(int warehouseId, string role);
        Task<List<WarehouseStaff>> GetByWarehouseAndRoleAsyncNormal(int warehouseId, string role);
    }
}
