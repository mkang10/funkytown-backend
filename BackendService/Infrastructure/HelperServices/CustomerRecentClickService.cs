using Application.Interfaces;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.HelperServices
{
    public class CustomerRecentClickService : ICustomerRecentClickService
    {
        private readonly IRedisCacheService _redisCacheService;
        private const int ExpireMinutes = 10; // TTL 10 phút cho mỗi session

        public CustomerRecentClickService(IRedisCacheService redisCacheService)
        {
            _redisCacheService = redisCacheService;
        }

        public async Task IncreaseStyleClickAsync(int customerDetailId, int styleId)
        {
            var cacheKey = $"customer:{customerDetailId}:recentClicks";
            var clicks = await _redisCacheService.GetCacheAsync<Dictionary<int, int>>(cacheKey) ?? new Dictionary<int, int>();

            if (clicks.ContainsKey(styleId))
                clicks[styleId]++;
            else
                clicks[styleId] = 1;

            await _redisCacheService.SetCacheAsync(cacheKey, clicks, TimeSpan.FromMinutes(ExpireMinutes));
        }

        public async Task<Dictionary<int, int>> GetRecentClicksAsync(int customerDetailId)
        {
            var cacheKey = $"customer:{customerDetailId}:recentClicks";
            return await _redisCacheService.GetCacheAsync<Dictionary<int, int>>(cacheKey) ?? new Dictionary<int, int>();
        }
    }
}
