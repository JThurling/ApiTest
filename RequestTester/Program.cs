using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RequestTester
{
    class Program
    {
        private static readonly object _lock = new object();

        static void Main(string[] args)
        {
            Console.WriteLine("Connecting to Host...");
            var id = ConnectionToHost();
            

            if (id == 200)
            {
                Console.WriteLine("Connected!");
                
                Console.WriteLine("Please enter the amount of requests to send");
                var anount = Int32.TryParse(Console.ReadLine(), out int value);
                Console.WriteLine("Do you want this sent concurrently? Y/N");
                var key = Console.ReadLine();

                Console.WriteLine("Getting Ready to send....");
                Stopwatch stopwatch = Stopwatch.StartNew();
                stopwatch.Start();
                Console.WriteLine("Starting Test....");

                SetupTest(value, key);
            
                Console.WriteLine("Success!");
                stopwatch.Stop();
                Console.WriteLine($"Time for test: {stopwatch.Elapsed}");
            }
            else
            {
                Console.WriteLine("Connection Failed");
            }
            
        }

        private static int ConnectionToHost()
        {
            using (var client = new HttpClient())
            {
                var get = client.GetAsync("https://localhost:5001/ping").Result;
                return (int) get.StatusCode;
            }        
        }

        private static Task SetupTest(int amount, string key)
        {
            Console.WriteLine($"Testing...");
            if (key == "y")
            {
                for (int i = 0; i < amount; i++)
                {
                    Thread t = new Thread(GetPing);
                    t.Start();
                }
            }
            else
            {
                for (int i = 0; i < amount; i++)
                {
                    GetPing();
                }  
            }
            
            return Task.CompletedTask;
        }

        public static void GetPing()
        {
            lock (_lock)
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-velta-client-guid", Guid.NewGuid().ToString());
                    client.DefaultRequestHeaders.Add("x-velta-client-sid", Guid.NewGuid().ToString());
                    client.DefaultRequestHeaders.Add("x-velta-request-uid", Guid.NewGuid().ToString());
                    var get = client.GetAsync("https://localhost:5001/ping");
                    
                    Console.WriteLine(get.Result.Content.ReadAsStringAsync().Result);
                }
            }
        }
    }
}
