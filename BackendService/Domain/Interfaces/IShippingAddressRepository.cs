using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IShippingAddressRepository
    {
        Task<ShippingAddress?> GetByIdAsync(int addressId);
        Task CreateAsync(ShippingAddress address);
        Task<ShippingAddress?> GetDefaultShippingAddressAsync(int accountId);
        Task<List<ShippingAddress>> GetShippingAddressesByAccountIdAsync(int accountId);
        Task UpdateAsync(ShippingAddress address);
        Task DeleteAsync(ShippingAddress address);
        Task<ShippingAddress?> GetDefaultAddressByAccountIdAsync(int accountId);
    }
}
