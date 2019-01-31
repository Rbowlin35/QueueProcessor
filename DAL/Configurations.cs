using System.Configuration;

namespace QueueProcessor.DAL
{
    public class Configurations
    {
        public static string ConnectionString { get { return $"{ConfigurationManager.AppSettings["ConnectionString"]}"; } }

        public static string WebApiBase { get { return $"http://localhost:{ConfigurationManager.AppSettings["WebAPIPort"]}"; } }

        public static string ConfigSelect { get { return $"{ConfigurationManager.AppSettings["ConfigSelect"]}"; } }

        public static string QueueName { get { return $"{ConfigurationManager.AppSettings["QueueName"]}"; } }

        public static int NumberThreads
        {
            get
            {
                return int.TryParse(ConfigurationManager.AppSettings["NumberThreads"], out int val) ? val : 3;
            }
        }

        public static int SleepTimout
        {
            get
            {
                return int.TryParse(ConfigurationManager.AppSettings["SleepTimout"], out int val) ? val : 5000;
            }
        }

    }
}