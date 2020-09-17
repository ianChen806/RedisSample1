using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace ConsoleApp1.Infra
{
    public class RedisClient
    {
        private readonly ConnectionMultiplexer _connection;

        public RedisClient()
        {
            var options = new ConfigurationOptions()
            {
                EndPoints = {"localhost"}
            };
            _connection = ConnectionMultiplexer.Connect(options);
        }

        public async Task<bool> Lock(string key, TimeSpan expiry)
        {
            var lockKey = $"Lock_{key}";
            var number = 0;
            do
            {
                try
                {
                    var database = _connection.GetDatabase();
                    if (await database.LockTakeAsync(lockKey, Environment.MachineName, expiry))
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    await Task.Delay(200);
                    number++;
                }
            } while (number < 10);

            return false;
        }

        public async Task SetString(string key, RedisValue value)
        {
            var database = _connection.GetDatabase();
            await database.StringSetAsync(key, value);
        }

        public async Task<RedisValue> GetString(string key)
        {
            var database = _connection.GetDatabase();

            return await database.StringGetAsync(key);
        }

        public async Task<bool> LockRelease(string key)
        {
            var lockKey = $"Lock_{key}";
            var database = _connection.GetDatabase();

            return await database.LockReleaseAsync(lockKey, Environment.MachineName);
        }

        public async Task<long> StringDecrement(string key)
        {
            var database = _connection.GetDatabase();

            return await database.StringDecrementAsync(key);
        }
    }
}