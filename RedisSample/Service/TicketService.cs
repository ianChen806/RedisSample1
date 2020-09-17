using System;
using System.Threading.Tasks;
using RedisSample.Infra;

namespace RedisSample.Service
{
    public class TicketService
    {
        private readonly RedisClient _client;

        public TicketService(RedisClient redisClient)
        {
            _client = redisClient;
        }

        public async Task<bool> GetTicket()
        {
            var count = (int)await _client.GetHash("Event", "Count");
            if (count > 0 && await _client.Lock("Test", TimeSpan.FromMinutes(1)))
            {
                if ((int)await _client.GetHash("Event", "Count") < 1)
                {
                    return false;
                }
                var last = await _client.HashDecrement("Event", "Count");
                await _client.LockRelease("Test");

                return true;
            }

            return false;
        }
    }
}