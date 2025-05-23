using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class RedisHandler
    {
        private readonly IRedisRepository _redisRepository;

        public RedisHandler(IRedisRepository redisRepository)
        {
            _redisRepository = redisRepository;
        }

        public async Task<string> ClearInstanceCacheAsync(string instanceName)
        {
            var keysToDelete = await _redisRepository.GetKeysByPatternAsync(instanceName + "*");

            if (keysToDelete.Count > 0)
            {
                await _redisRepository.RemoveKeysAsync(keysToDelete);
                return $"All cache with InstanceName '{instanceName}' has been cleared.";
            }

            return "No matching cache found for InstanceName.";
        }
    }
}
