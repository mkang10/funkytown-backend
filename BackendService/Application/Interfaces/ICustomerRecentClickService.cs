using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICustomerRecentClickService
    {
        Task IncreaseStyleClickAsync(int customerDetailId, int styleId);
        Task<Dictionary<int, int>> GetRecentClicksAsync(int customerDetailId);
    }
}
