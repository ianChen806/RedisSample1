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
        private static readonly string _ticketsKey = "Event2_Count";
        private static readonly TicketService _service = new TicketService(_client);

        private static async Task Init()
        {
            await _client.SetString(_ticketsKey, 100);
            Console.WriteLine("set count: 100");
        }

        private static async Task Main(string[] args)
        {
            await Init();

            await Subscribe();

            await Run();
        }

        private static async Task Subscribe()
        {
            await _service.Subscribe();
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
                    result.Push(await _service.GetTicket(number));
                    Console.WriteLine($"add {number}");
                }));
            }
            await Task.WhenAll(tasks);

            Console.WriteLine($"success count: {result.Count(r => r)}");
        }
    }
}