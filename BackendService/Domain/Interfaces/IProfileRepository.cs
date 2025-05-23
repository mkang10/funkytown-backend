using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IProfileRepository
    {
        Task<Account?> GetAccountByIdAsync(int accountId);
        Task<CustomerDetail?> GetCustomerDetailByAccountIdAsync(int accountId);
        Task UpdateAccountAsync(Account account);
        Task UpdateCustomerDetailAsync(CustomerDetail customerDetail);
        Task<(Account?, CustomerDetail?)> GetCustomerProfileByAccountIdAsync(int accountId);
        Task<List<CustomerStyle>> GetStylesByCustomerDetailIdAsync(int customerDetailId);

        Task<CustomerStyle?> GetCustomerStyleAsync(int customerDetailId, int styleId);

        Task InsertAsync(CustomerStyle entity);

        Task UpdateAsync(CustomerStyle entity);
        Task<Style?> GetStyleByNameAsync(string styleName);
        Task<List<Style>> GetPreferredStylesByCustomerDetailIdAsync(int customerDetailId);
        Task UpdatePreferredStylesAsync(int customerDetailId, List<int> styleIds);
        Task<Style?> GetStyleByIdAsync(int styleId);
        Task<List<Style>> GetAllStylesAsync();

    }
}
