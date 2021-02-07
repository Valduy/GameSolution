using System;
using System.Threading.Tasks;
using Matches;

namespace FakeListenSession
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var session = new ListenSessionMatch(2, 120000, int.Parse(args[0]));
            Console.WriteLine("Fake listen session started!");

            await Task.Run(async () =>
            {
                var task = session.WorkAsync();

                while (!task.IsCompleted)
                {
                    Console.WriteLine("In progress...");
                    await Task.Delay(1000);
                }
            });
            
            Console.WriteLine("Complete");
        }
    }
}
