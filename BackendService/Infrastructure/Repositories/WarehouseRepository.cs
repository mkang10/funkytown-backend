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
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly FtownContext _context;

        public WarehouseRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<List<Warehouse>> GetAllWarehousesAsync()
        {
            return await _context.Warehouses.ToListAsync();
        }

        public async Task<Warehouse> CreateAsync(Warehouse warehouse)
        {
            await _context.Warehouses.AddAsync(warehouse);
            await _context.SaveChangesAsync();
            return warehouse;
        }
        public async Task<Warehouse?> GetByIdAsync(int warehouseId)
       => await _context.Warehouses.FindAsync(warehouseId);

        public async Task UpdateAsync(Warehouse warehouse)
        {
            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task<Warehouse> GetOwnerWarehouseAsync()
        {
            return await _context.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.IsOwnerWarehouse == true);
        }

        public async Task<IEnumerable<Warehouse>> GetAllAsync()
            => await _context.Warehouses.ToListAsync();

        public async Task<Warehouse?> GetWarehouseByIdAsync(int warehouseId)
        {
            return await _context.Warehouses.FindAsync(warehouseId);
        }

        public async Task<Warehouse> CreateWarehouseAsync(Warehouse warehouse)
        {
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
            return warehouse;
        }

        public async Task UpdateWarehouseAsync(Warehouse warehouse)
        {
            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteWarehouseAsync(int warehouseId)
        {
            var warehouse = await _context.Warehouses.FindAsync(warehouseId);
            if (warehouse != null)
            {
                _context.Warehouses.Remove(warehouse);
                await _context.SaveChangesAsync();
            }
        }
        
    }
}
