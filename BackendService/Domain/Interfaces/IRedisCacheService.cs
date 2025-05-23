using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IRedisCacheService
    {
        Task<T?> GetCacheAsync<T>(string key);
        Task SetCacheAsync<T>(string key, T value, TimeSpan expiration);
        Task RemoveCacheAsync(string key);
        Task RemoveByPatternAsync(string pattern);

    }
}
