using Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;

        public RedisRepository(IDistributedCache cache, IConnectionMultiplexer redis)
        {
            _cache = cache;
            _redis = redis;
        }

        public async Task<List<string>> GetKeysByPatternAsync(string pattern)
        {
            var keys = new List<string>();
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            await foreach (var key in server.KeysAsync(pattern: pattern + "*"))
            {
                keys.Add(key);
            }
            return keys;
        }

        public async Task RemoveKeysAsync(List<string> keys)
        {
            if (keys == null || !keys.Any()) return; // Không có keys thì không làm gì cả

            var redisDb = _redis.GetDatabase(); // Lấy database Redis
            foreach (var key in keys)
            {
                await redisDb.KeyDeleteAsync(key);
            }
        }
    }
}
