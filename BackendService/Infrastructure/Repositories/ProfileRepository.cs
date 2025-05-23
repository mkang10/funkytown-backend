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
    public class ProfileRepository : IProfileRepository
    {
        private readonly FtownContext _context;

        public ProfileRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetAccountByIdAsync(int accountId)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
        }

        public async Task<CustomerDetail?> GetCustomerDetailByAccountIdAsync(int accountId)
        {
            return await _context.CustomerDetails.FirstOrDefaultAsync(cd => cd.AccountId == accountId);
        }

        public async Task UpdateAccountAsync(Account account)
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCustomerDetailAsync(CustomerDetail customerDetail)
        {
            _context.CustomerDetails.Update(customerDetail);
            await _context.SaveChangesAsync();
        }
        public async Task<(Account?, CustomerDetail?)> GetCustomerProfileByAccountIdAsync(int accountId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
            var customerDetail = await _context.CustomerDetails.FirstOrDefaultAsync(cd => cd.AccountId == accountId);
            return (account, customerDetail);
        }
        public async Task<List<CustomerStyle>> GetStylesByCustomerDetailIdAsync(int customerDetailId)
        {
            return await _context.CustomerStyles
                .Where(cs => cs.CustomerDetailId == customerDetailId)
                .Include(cs => cs.Style) // 🔥 Thêm Include Style
                .ToListAsync();
        }


        public async Task<CustomerStyle?> GetCustomerStyleAsync(int customerDetailId, int styleId)
        {
            return await _context.CustomerStyles
                .FirstOrDefaultAsync(cs => cs.CustomerDetailId == customerDetailId && cs.StyleId == styleId);
        }

        public async Task InsertAsync(CustomerStyle entity)
        {
            await _context.CustomerStyles.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CustomerStyle entity)
        {
            _context.CustomerStyles.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<Style?> GetStyleByNameAsync(string styleName)
        {
            return await _context.Styles
                .FirstOrDefaultAsync(s => s.StyleName == styleName);
        }
        public async Task<List<Style>> GetPreferredStylesByCustomerDetailIdAsync(int customerDetailId)
        {
            return await _context.CustomerStyles
                .Where(cs => cs.CustomerDetailId == customerDetailId && cs.IsFromPreference == true)
                .Select(cs => cs.Style)
                .ToListAsync();
        }
        public async Task<List<Style>> GetAllStylesAsync()
        {
            return await _context.Styles.ToListAsync();
        }
        public async Task UpdatePreferredStylesAsync(int customerDetailId, List<int> styleIds)
        {
            // B1. Lấy toàn bộ
            var currentStyles = await _context.CustomerStyles
                .Where(cs => cs.CustomerDetailId == customerDetailId)
                .ToListAsync();

            // B2. Ghi nhớ trạng thái ban đầu
            var originalPref = currentStyles
                .ToDictionary(cs => cs.StyleId, cs => cs.IsFromPreference);

            var newSet = styleIds.ToHashSet();

            // B3. Reset tất cả về false
            foreach (var cs in currentStyles)
                cs.IsFromPreference = false;

            // B4. Cập nhật hoặc thêm mới
            foreach (var id in styleIds)
            {
                var style = currentStyles.FirstOrDefault(cs => cs.StyleId == id);
                if (style != null)
                {
                    // Chỉ cộng 10 điểm nếu ban đầu chưa là preference
                    if (!originalPref.TryGetValue(id, out var wasPref) || !wasPref)
                        style.Point += 10;

                    style.IsFromPreference = true;
                    style.LastUpdatedDate = DateTime.UtcNow;
                }
                else
                {
                    // Thêm mới style với 10 điểm
                    await _context.CustomerStyles.AddAsync(new CustomerStyle
                    {
                        CustomerDetailId = customerDetailId,
                        StyleId = id,
                        Point = 10,
                        ClickCount = 0,
                        IsFromPreference = true,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow
                    });
                }
            }

            // B5. Trừ điểm cho những style vốn là preference nhưng giờ bị bỏ
            foreach (var removedId in originalPref
                .Where(kvp => kvp.Value && !newSet.Contains(kvp.Key))
                .Select(kvp => kvp.Key))
            {
                var style = currentStyles.First(cs => cs.StyleId == removedId);
                style.Point -= 25;
                style.LastUpdatedDate = DateTime.UtcNow;
            }

            // B6. Xóa style có Point <= 0
            _context.CustomerStyles.RemoveRange(
                currentStyles.Where(cs => cs.Point <= 0)
            );

            await _context.SaveChangesAsync();
        }


        public async Task<Style?> GetStyleByIdAsync(int styleId)
        {
            return await _context.Styles
                .FirstOrDefaultAsync(s => s.StyleId == styleId);
        }
    }
}
