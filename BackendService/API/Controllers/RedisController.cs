using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisController : ControllerBase
    {
        private readonly RedisHandler _redisHandler;

        public RedisController(RedisHandler redisHandler)
        {
            _redisHandler = redisHandler;
        }

        [HttpDelete("clear-instance")]
        public async Task<IActionResult> ClearInstanceCache()
        {
            string instanceName = "ProductInstance"; // 👈 Đặt InstanceName của bạn tại đây

            var resultMessage = await _redisHandler.ClearInstanceCacheAsync(instanceName);
            return Ok(new { Message = resultMessage });
        }
    }
}
