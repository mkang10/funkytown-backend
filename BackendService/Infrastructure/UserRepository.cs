using Application.Interfaces;
using Domain.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Domain.Interfaces;

namespace Infrastructure.Repositories
{
    public class AccountRepos : IAccountRepos
    {
        private readonly FtownContext _context;

        public AccountRepos(FtownContext context)
        {
            _context = context;
        }

        public async Task<Account> GetUserByUsernameAsync(string fullname)
        {
            return await _context.Accounts.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.FullName == fullname);
        }

        public async Task<Account> GetUserByEmail(string email)
        {
            return await _context.Accounts.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(Account acc)
        {
            await _context.Accounts.AddAsync(acc);
            await _context.SaveChangesAsync();
        }

        public async Task AddStaffAsync(StaffDetail staff)
        {
            await _context.StaffDetails.AddAsync(staff);
            await _context.SaveChangesAsync();
        }

        public async Task AddShopmanagerAsync(ShopManagerDetail shopManager)
        {
            await _context.ShopManagerDetails.AddAsync(shopManager);
            await _context.SaveChangesAsync();
        }
        public async Task AddCustomerAsync(CustomerDetail cus)
        {
            await _context.CustomerDetails.AddAsync(cus);
            await _context.SaveChangesAsync();
        }

        public async Task<object?> GetRoleDetailsAsync(Account account)
        {
            switch (account.RoleId)
            {
                case 1: // Ví dụ: RoleId = 1 là Customer
                    return await _context.CustomerDetails
                        .Where(c => c.AccountId == account.AccountId)
                        .Select(c => new
                        {
                            c.CustomerDetailId,
                            c.LoyaltyPoints,
                            c.MembershipLevel,
                            c.DateOfBirth,
                            c.Gender,
                            c.CustomerType,
                            c.PreferredPaymentMethod
                        }).FirstOrDefaultAsync();

                case 2: // Ví dụ: RoleId = 2 là Shop Manager
                    return await _context.ShopManagerDetails
                        .Where(m => m.AccountId == account.AccountId)
                        .Select(m => new
                        {
                            m.ShopManagerDetailId,
                            StoreId = _context.Warehouses
            .Where(w => w.ShopManagerId == m.ShopManagerDetailId)
            .Select(w => w.WarehouseId)
            .FirstOrDefault(),
                            m.ManagedDate,
                            m.YearsOfExperience,
                            m.ManagerCertifications,
                            m.OfficeContact
                        }).FirstOrDefaultAsync();

                case 3: // Ví dụ: RoleId = 3 là Staff
                    return await _context.StaffDetails
                        .Where(s => s.AccountId == account.AccountId)
                        .Select(s => new
                        {
                            s.StaffDetailId,
                            s.JoinDate,


                        }).FirstOrDefaultAsync();

                default:
                    return null;
            }
        }


    }
}
