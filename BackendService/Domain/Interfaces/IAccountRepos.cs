using Domain.Entities;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAccountRepos
    {
        Task<Account> GetUserByEmail(string email);
        Task<Account> GetUserByUsernameAsync(string fullname);
        Task AddUserAsync(Account user);

        Task AddStaffAsync(StaffDetail staff);

        Task AddShopmanagerAsync(ShopManagerDetail shopManager);

        Task AddCustomerAsync(CustomerDetail cus);

        Task<object?> GetRoleDetailsAsync(Account account);


    }


}
