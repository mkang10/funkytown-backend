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
    public class DispatchDetailRepos : IDispatchDetailRepository
    {
        private readonly FtownContext _context;

        public DispatchDetailRepos(FtownContext context)
        {
            _context = context;
        }
        public async Task AddRangeAndSaveAsync(IEnumerable<DispatchDetail> entities)
        {
            await _context.Set<DispatchDetail>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
    }
}
