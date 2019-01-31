using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QueueProcessor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace QueueProcessor.Host
{
    public class ProcessorController : ApiController
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
        }
        public string Get()
        {
            var result = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => typeof(ApiController).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
            .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
            .GroupBy(x => x.DeclaringType.Name)
            .Select(x => new { Controller = x.Key, Actions = x.Select(s => s.Name).ToList() })
            .ToList();

            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        public string Calculate(string parms)
        {
            var logger = log4net.LogManager.GetLogger("ProcessorController");
            return Orchestrator.BuildAndProcess(parms, logger);
        }
    }
}
