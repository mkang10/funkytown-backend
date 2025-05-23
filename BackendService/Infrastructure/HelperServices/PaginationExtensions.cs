using Application.Interfaces;
using Domain.Common_Model;
using Infrastructure.HelperServices.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HelperServices
{
    public static class PaginationExtensions
    {
        public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
            this IQueryable<T> query, int pageNumber, int pageSize)
        {
            var helper = new PaginationHelper(); // Gọi class helper trực tiếp
            return await helper.PaginateAsync(query, pageNumber, pageSize);
        }
    }
    public class PaginationHelper : IPaginationHelper
    {
        public async Task<PaginatedResult<T>> PaginateAsync<T>(
            IQueryable<T> query, int pageNumber, int pageSize)
        {
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<T>(items, totalCount, pageNumber, pageSize);
        }
        public PaginatedResult<T> PaginateInMemory<T>(IEnumerable<T> items, int pageNumber, int pageSize)
        {
            var totalCount = items.Count();
            var pagedItems = items.Skip((pageNumber - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList();

            return new PaginatedResult<T>(pagedItems, totalCount, pageNumber, pageSize);
        }

    }
}
