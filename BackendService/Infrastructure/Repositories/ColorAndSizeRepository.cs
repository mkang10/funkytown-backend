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

namespace Infrastructure
{
    public class ColorAndSizeRepository : ISizeAndColorRepository
    {
        private readonly FtownContext _context;

        public ColorAndSizeRepository(FtownContext context)
        {
            _context = context;
        }

        public async Task<Color> CreateColor(Color data)
        {
            _context.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<Size> CreateSize(Size data)
        {
            _context.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<bool> DeleteColor(Color data)
        {
            _context.Remove(data);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSize(Size data)
        {
            _context.Remove(data);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Pagination<Color>> GetAllColor(PaginationParameter paginationParameter)
        {
            var itemCount = await _context.Colors.CountAsync();
            var items = await _context.Colors
                                    .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                    .Take(paginationParameter.PageSize)
                                    .AsNoTracking()
                                    .ToListAsync();
            var result = new Pagination<Color>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        public async Task<Pagination<Size>> GetAllSize(PaginationParameter paginationParameter)
        {
            var itemCount = await _context.Sizes.CountAsync();
            var items = await _context.Sizes
                                    .Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                    .Take(paginationParameter.PageSize)
                                    .AsNoTracking()
                                    .ToListAsync();
            var result = new Pagination<Size>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        public async Task<List<Color>> GetByCode(string id)
        {
            string keyword = id.ToUpper(); 
            return await _context.Colors
                                 .Where(o => o.ColorCode.ToUpper().Contains(keyword))
                                 .ToListAsync();
        }

        public async Task<Color> GetByCodeColor(string id)
        {
            string keyword = id.ToLower();    
            return await _context.Colors.SingleOrDefaultAsync(o => o.ColorCode.ToLower() == keyword);
        }

        public async Task<Size> GetByName(string name)
        {
            string keyword = name.ToUpper();
            return await _context.Sizes.SingleOrDefaultAsync(o => o.SizeName.ToUpper() == keyword);
        }

        public async Task<List<Size>> GetBySizeName(string name)
        {

            string keyword = name.ToUpper();
            return await _context.Sizes
                                 .Where(o => o.SizeName.ToUpper().Contains(keyword))
                                 .ToListAsync();
        }

        public async Task<Color> UpdateColor(Color data)
        {
            _context.Update(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<Size> UpdateSize(Size data)
        {
            _context.Update(data);
            await _context.SaveChangesAsync();
            return data;
        }
    }
}
