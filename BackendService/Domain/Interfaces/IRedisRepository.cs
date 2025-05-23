using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IRedisRepository
    {
        Task<List<string>> GetKeysByPatternAsync(string pattern);
        Task RemoveKeysAsync(List<string> keys);
    }
}
