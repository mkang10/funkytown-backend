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
    public class WarehouseStaffRepository : IWarehouseStaffRepos
    {
        private readonly FtownContext _db;
        public WarehouseStaffRepository(FtownContext db) => _db = db;

        public async Task<List<WarehouseStaff>> GetByWarehouseAndRoleAsync(int warehouseId, string role)
        {
            return await _db.WarehouseStaffs
                            .Where(ws => ws.WarehouseId == warehouseId
                                      && ws.Role == role
                                      && ws.Warehouse.IsOwnerWarehouse == true)
                            .ToListAsync();
        }

        public async Task<List<WarehouseStaff>> GetByWarehouseAndRoleAsyncNormal(int warehouseId, string role)
        {
            return await _db.WarehouseStaffs
                            .Where(ws => ws.WarehouseId == warehouseId
                                      && ws.Role == role
                                      )
                            .ToListAsync();
        }
    }
}
