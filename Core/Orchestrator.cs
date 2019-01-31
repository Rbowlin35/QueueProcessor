using log4net;
using Newtonsoft.Json;
using QueueProcessor.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueProcessor.Core
{
    public class Orchestrator
    {
        public static IEnumerable<string> Process(QueueItem item, ILog logger)
        {
            //Todo: add your code here
            return new List<string>();
        }

        public static string BuildAndProcess(string parms, ILog logger)
        {
            var q = new QueueItem() { Id = -1, Parms = parms };
            var results = Process(q, logger);
            return JsonConvert.SerializeObject(results);
        }

    }
}
