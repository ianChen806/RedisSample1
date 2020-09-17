using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ConsoleApp1.Infra;

namespace ConsoleApp1.Service
{
    public class TicketService
    {
        private readonly RedisClient _client;
        private readonly string _channelKey = "TicketChannel";
        private readonly string _key = "Event2_Count";
        private readonly ConcurrentBag<string> _users;

        public TicketService(RedisClient redisClient)
        {
            _client = redisClient;
            _users = new ConcurrentBag<string>();
        }

        public async Task<bool> GetTicket(int client)
        {
            if (await ClientCount() > 0 && await _client.Lock(_key, TimeSpan.FromMilliseconds(100)))
            {
                try
                {
                    var lastCount = await _client.StringDecrement(_key);
                    if (lastCount < 0)
                    {
                        return false;
                    }
                    await _client.Publish(_channelKey, client.ToString());

                    return true;
                }
                finally
                {
                    await _client.LockRelease(_key);
                }
            }

            return false;
        }

        private async Task<long> ClientCount()
        {
            return (long)await _client.GetString(_key);
        }

        public async Task Subscribe()
        {
            await _client.Subscribe(_channelKey, value =>
            {
                _users.Add(value);
                SendEmail("恭喜搶到！");
                Console.WriteLine($"user count: {_users.Count}");
            });
        }

        private void SendEmail(string message)
        {
        }
    }
}
