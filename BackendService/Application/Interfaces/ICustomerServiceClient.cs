using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICustomerServiceClient
    {
        Task<List<CartItem>?> GetCartAsync(int accountId);
        Task<bool> ClearCartAfterOrderAsync(int accountId, List<int> selectedProductVariantIds);
    }
}
