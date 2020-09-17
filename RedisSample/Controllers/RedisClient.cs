using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RedisSample.Controllers
{
    [ApiController]
    [Route("Redis/[action]")]
    public class RedisClient
    {
        [HttpGet]
        public async Task<string> Get()
        {
            var redis = await ConnectionMultiplexer.ConnectAsync("localhost");
            var database = redis.GetDatabase();

            var value = await database.StringGetAsync("test-key");

            return value;
        }

        [HttpGet]
        public async Task<bool> Index()
        {
            var options = new ConfigurationOptions()
            {
                EndPoints = {"localhost"},
                ReconnectRetryPolicy = new ExponentialRetry(5000),
            };

            var redis = await ConnectionMultiplexer.ConnectAsync(options);
            var database = redis.GetDatabase();

            return await database.StringSetAsync("test-key", "test");
        }
    }
}