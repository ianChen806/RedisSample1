using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RedisSample.Infra;
using RedisSample.Service;
using StackExchange.Redis;

namespace RedisSample.Controllers
{
    [ApiController]
    [Route("Redis/[action]")]
    public class RedisController
    {
        private readonly RedisClient _client;
        private readonly TicketService _service;

        public RedisController(RedisClient redisClient, TicketService ticketService)
        {
            _client = redisClient;
            _service = ticketService;
        }

        [HttpGet]
        public async Task<int> Get()
        {
            var result = new ConcurrentStack<bool>();
            var tasks = new List<Task>();
            for (var index = 0; index < 200; index++)
            {
                var task = Task.Run(async () =>
                {
                    var ticket = await _service.GetTicket();
                    result.Push(ticket);
                });
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);

            return result.Count(r => r);
        }

        [HttpGet]
        public async Task<Dictionary<string, string>> Init()
        {
            await _client.SetHash("Event", "Id", "123");
            await _client.SetHash("Event", "Count", 100);

            return await _client.GetHash("Event");
        }


        [HttpGet]
        public async Task<Dictionary<string, string>> Value()
        {
            return await _client.GetHash("Event");
        }

        [HttpGet]
        public string Test()
        {
            var ints = new List<int>() {1, 2, 3, 4, 5};
            var stack = new Stack<int>(ints);

            return string.Join(",", stack.ToList());
        }
    }
}