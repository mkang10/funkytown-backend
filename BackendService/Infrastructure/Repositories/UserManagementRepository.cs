using Domain.Commons;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Repositories
{
    public class UserManagementRepository : IUserManagementRepository
    {
        private readonly FtownContext _context;

        public UserManagementRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<Role> CreateRole(Role role)
        {
            _context.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<ShopManagerDetail> CreateShopmanagerDetail(ShopManagerDetail user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<StaffDetail> CreateStaffDetail(StaffDetail user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<Account> CreateUser(Account user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteRole(Role role)
        {
            _context.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUser(Account user)
        {
            _context.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Role>> GetAllRole()
        {
            var data = await _context.Roles.ToListAsync();
            return data;
        }


        public async Task<Pagination<Account>> GetAllUser(PaginationParameter paginationParameter)
        {
            var itemCount = await _context.Accounts.CountAsync();
            var items = await _context.Accounts.Include(o => o.Role)
                                    .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                    .Take(paginationParameter.PageSize)
                                    .AsNoTracking()
                                    .ToListAsync();
            var result = new Pagination<Account>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        public async Task<Role> GetRoleById(int id)
        {
            var data = await _context.Roles.SingleOrDefaultAsync(x => x.RoleId.Equals(id));
            return data;
        }

        public async Task<ShopManagerDetail> GetShopManagerdetailById(int id)
        {
            var data = await _context.ShopManagerDetails.SingleOrDefaultAsync(x => x.AccountId.Equals(id));
            return data;
        }
        public async Task<StaffDetail> GetStaffDetailById(int id)
        {
            var data = await _context.StaffDetails.SingleOrDefaultAsync(x => x.AccountId.Equals(id));
            return data;
        }

        public async Task<Account> GetUserByGmail(string gmail)
        {
            var data = await _context.Accounts.Include(o => o.Role).SingleOrDefaultAsync(x => x.Email.Equals(gmail));
            return data;
        }

        public async Task<Account> GetUserById(int id)
        {
            var data = await _context.Accounts
                .Include(o => o.Role)
                .SingleOrDefaultAsync(x => x.AccountId.Equals(id));
            return data;
        }

        public async Task<Account> GetUserByName(string name)
        {
            var data = await _context.Accounts.SingleOrDefaultAsync(x => x.FullName.Equals(name));
            return data;
        }

        public async Task<Role> UpdateRole(Role role)
        {
            _context.Update(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<ShopManagerDetail> UpdateShopmanagerDetail(ShopManagerDetail user)
        {
            _context.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task<StaffDetail> UpdateStaffDetail(StaffDetail user)
        {
            _context.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<Account> UpdateUser(Account user)
        {
            _context.Update(user);
            await _context.SaveChangesAsync();
            return user;
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
