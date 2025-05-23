using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ShippingAddressRepository : IShippingAddressRepository
    {
        private readonly FtownContext _context;
        public ShippingAddressRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<ShippingAddress?> GetByIdAsync(int addressId)
        {
            return await _context.ShippingAddresses
                                 .FirstOrDefaultAsync(a => a.AddressId == addressId);
        }

        public async Task<ShippingAddress?> GetDefaultShippingAddressAsync(int accountId)
        {
            return await _context.ShippingAddresses
                                 .Where(a => a.AccountId == accountId && a.IsDefault == true)
                                 .FirstOrDefaultAsync();
        }
        public async Task<List<ShippingAddress>> GetShippingAddressesByAccountIdAsync(int accountId)
        {
            var addresses = await _context.ShippingAddresses
                                          .Where(addr => addr.AccountId == accountId)
                                          .AsNoTracking()               // Không theo dõi để truy vấn tối ưu hơn
                                          .ToListAsync();
            // Nếu không có kết quả, addresses sẽ là một danh sách rỗng (không phải null).
            return addresses;
        }
        public async Task CreateAsync(ShippingAddress address)
        {
            _context.ShippingAddresses.Add(address);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ShippingAddress address)
        {
            _context.ShippingAddresses.Update(address);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ShippingAddress address)
        {
            _context.ShippingAddresses.Remove(address);
            await _context.SaveChangesAsync();
        }
        public async Task<ShippingAddress?> GetDefaultAddressByAccountIdAsync(int accountId)
        {
            return await _context.ShippingAddresses
                .FirstOrDefaultAsync(x => x.AccountId == accountId && x.IsDefault == true);
        }
    }
}
