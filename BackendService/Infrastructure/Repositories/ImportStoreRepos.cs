using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class ImportStoreRepos : IImportStoreRepos
    {
        private readonly FtownContext _context;
        public ImportStoreRepos(FtownContext context)
        {
            _context = context;
        }

        public void Add(ImportStoreDetail importStore)
        {
            _context.ImportStoreDetails.Add(importStore);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
        public async Task UpdateRangeAsync(IEnumerable<ImportStoreDetail> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            // Đánh dấu các entity cần cập nhật
            _context.ImportStoreDetails.UpdateRange(entities);

            // Lưu thay đổi
            await _context.SaveChangesAsync();
        }
        public async Task AddRangeAsync(IEnumerable<ImportStoreDetail> details)
       => await _context.ImportStoreDetails.AddRangeAsync(details);

       

        public async Task UpdateAsync(ImportStoreDetail entity)
        {
            // Nếu entity vừa được tracked (ví dụ bạn lấy từ DB trước đó), chỉ cần:
            _context.ImportStoreDetails.Update(entity);
            // Nếu bạn muốn chỉ đánh dấu là modified mà không overwrite toàn bộ:
            // _context.Entry(entity).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        
    }

}
