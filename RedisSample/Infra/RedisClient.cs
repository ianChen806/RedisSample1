using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisSample.Infra
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
            var number = 0;
            do
            {
                try
                {
                    var database = _connection.GetDatabase();
                    if (await database.LockTakeAsync(key, Environment.MachineName, expiry))
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

        public async Task SetHash(string key, string field, RedisValue value)
        {
            var database = _connection.GetDatabase();
            await database.HashSetAsync(key, field, value);
        }

        public async Task<Dictionary<string, string>> GetHash(string key)
        {
            var database = _connection.GetDatabase();
            var entries = await database.HashGetAllAsync(key);

            return entries.ToDictionary(
                r => r.Name.ToString(),
                r => r.Value.ToString());
        }

        public async Task<RedisValue> GetHash(string key, string field)
        {
            var database = _connection.GetDatabase();
            var entries = await database.HashGetAsync(key, field);

            return entries;
        }

        public async Task<long> HashDecrement(string key, string field)
        {
            var database = _connection.GetDatabase();
            var result = await database.HashDecrementAsync(key, field);

            return result;
        }

        public async Task<bool> LockRelease(string key)
        {
            var database = _connection.GetDatabase();
            var result = await database.LockReleaseAsync(key, Environment.MachineName);

            return result;
        }
    }
}