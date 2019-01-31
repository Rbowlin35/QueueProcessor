using Microsoft.Owin.Hosting;
using QueueProcessor.Core;
using QueueProcessor.DAL;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QueueProcessor.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = log4net.LogManager.GetLogger("Calculator.Host");
            logger.Info($"Starting calculator host");

            bool run = true;
            if (args.Contains("-drop"))
            {
                QueueItem.DropQueueTable();
                run = false;
            }

            if (args.Contains("-create"))
            {
                QueueItem.CreateQueueTable();
                run = true;
            }

            if (run)
            {
                QueueItem.ResetItems();
                var configs = ConfigurationLookup.Create();

                using (WebApp.Start<Startup>(url: Configurations.WebApiBase))
                {

                    var runner = new QueueProcessorCore();
                    var t = Task.Factory.StartNew(() =>
                    {
                        runner.Run(configs, Configurations.NumberThreads);
                    });

                    Console.ReadKey();
                    runner.Stop();

                    Task.WaitAll(t);
                }
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

    }
}
