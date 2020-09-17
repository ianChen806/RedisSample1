using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp1.Infra;
using ConsoleApp1.Service;

namespace ConsoleApp1
{
    internal class Program
    {
        private static readonly RedisClient _client = new RedisClient();
        private static readonly string _eventCountKey = "Event_Count";
        private static readonly TicketService _service = new TicketService(_client);

        private static async Task Init()
        {
            await _client.SetString(_eventCountKey, 100);

            var value = await _client.GetString(_eventCountKey);
            Console.WriteLine(value);
        }

        private static async Task Main(string[] args)
        {
            await Init();

            await Run();
        }

        private static async Task Run()
        {
            var result = new ConcurrentStack<bool>();
            var tasks = new List<Task>();
            for (var index = 0; index < 105; index++)
            {
                var number = index;
                tasks.Add(Task.Run(async () =>
                {
                    result.Push(await _service.GetTicket(_eventCountKey));
                    Console.WriteLine($"{number}");
                }));
            }
            await Task.WhenAll(tasks);

            Console.WriteLine($"success count: {result.Count(r => r)}");
        }
    }
}