using Domain.Commons;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly FtownContext _context;

        public CategoryRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<Category> Create(Category data)
        {
            _context.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<bool> Delete(Category data)
        {
            _context.Remove(data);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Pagination<Category>> GetAllCategory(PaginationParameter paginationParameter)
        {
            var itemCount = await _context.Categories.CountAsync();
            var items = await _context.Categories
                                    .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                    .Take(paginationParameter.PageSize)
                                    .AsNoTracking()
                                    .ToListAsync();
            var result = new Pagination<Category>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        public async Task<List<Category>> GetByCategoryName(string name)
        {
            string keyword = name.ToUpper();
            return await _context.Categories
                                 .Where(o => o.Name.ToUpper().Contains(keyword))
                                 .ToListAsync();
        }

        public async Task<Category> GetById(int id)
        {
            return await _context.Categories
                                 .SingleOrDefaultAsync(o => o.CategoryId == id);
        }

        public async Task<Category> GetByName(string id)
        {
            string keyword = id.ToUpper();

            return await _context.Categories
                                             .SingleOrDefaultAsync(o => o.Name.ToUpper() == keyword);
        }

        public async Task<Category> Update(Category data)
        {
            _context.Update(data);
            await _context.SaveChangesAsync();
            return data;
        }
    }
}
