using log4net;
using QueueProcessor.DAL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueProcessor.Core
{
    public class QueueProcessorCore
    {
        internal ConfigurationLookup _configs;
        internal ILog _logger;

        internal bool _stopRequested { get; set; }

        public QueueProcessorCore(ILog logger = null)
        {
            _logger = logger ?? log4net.LogManager.GetLogger("CalculatorCore"); ;
        }

        public void Run(ConfigurationLookup configs, int numberOfThreads)
        {
            _configs = configs;
            var threads = new List<Task>();

            foreach (var i in Enumerable.Range(0, numberOfThreads))
            {
                _logger.Debug($"Starting thread number {i+1}");
                threads.Add(Task.Factory.StartNew(() => RunTask()));
            }
            Task.WaitAll(threads.ToArray());
        }

        public void Stop()
        {
            _stopRequested = true;
        }

        private void RunTask()
        {
            while (!_stopRequested)
            {
                QueueItem item = null;
                try
                {
                    _logger.Debug("Dequeueing item");
                    item = QueueItem.DeQueue();
                    if (item != null)
                    {
                        _logger.Info($"processing queue Id {item.Id} with parms: {item.JParams.ToString()}");
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        Orchestrator.Process(item, _logger);
                        _logger.Info($"Finished processing queue Id {item.Id} in {sw.ElapsedMilliseconds} milliseconds");
                        item.SetStatus("Finished");
                    }
                    else
                    {
                        _logger.Debug($"Nothing to dequeue, sleeping");
                        Thread.Sleep(Configurations.SleepTimout);
                    }
                }
                catch(Exception ex)
                {
                    _logger.Error($"Error processing: {ex}");
                    item?.SetStatus("Error", ex.Message);
                }
            }
        }

    }
}
