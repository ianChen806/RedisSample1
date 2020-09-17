using System;
using System.Threading.Tasks;
using ConsoleApp1.Infra;

namespace ConsoleApp1.Service
{
    public class TicketService
    {
        private readonly RedisClient _client;

        public TicketService(RedisClient redisClient)
        {
            _client = redisClient;
        }

        public async Task<bool> GetTicket(string key)
        {
            if (await TicketCount(key) > 0 && await _client.Lock(key, TimeSpan.FromMilliseconds(100)))
            {
                try
                {
                    var lastCount = await _client.StringDecrement(key);

                    return lastCount >= 0;
                }
                finally
                {
                    await _client.LockRelease(key);
                }
            }

            return false;
        }

        private async Task<int> TicketCount(string key)
        {
            return (int)await _client.GetString(key);
        }
    }
}