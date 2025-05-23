using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
    }
}
