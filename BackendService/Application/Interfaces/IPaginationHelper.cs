using Domain.Common_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPaginationHelper
    {
        Task<PaginatedResult<T>> PaginateAsync<T>(IQueryable<T> query, int pageNumber, int pageSize);
        PaginatedResult<T> PaginateInMemory<T>(IEnumerable<T> items, int pageNumber, int pageSize);
    }
}
