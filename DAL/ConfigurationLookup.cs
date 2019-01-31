using Dapper;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueProcessor.DAL
{
    public class ConfigurationLookup
    {
        internal static ConfigurationLookup _instance;
        private Dictionary<string, string> _configs;
        internal ILog _logger { get { return log4net.LogManager.GetLogger("Queue"); } }

        public static ConfigurationLookup Create()
        {
            if (_instance == null)
                _instance = new ConfigurationLookup();

            _instance.Load();
            return _instance;
        }

        public string GetValue(string name)
        {
            _logger.Debug($"Retrieving config item of {name}");
            return _configs.ContainsKey(name) ? _configs[name] : string.Empty;
        }

        internal void Load()
        {
            if (!string.IsNullOrWhiteSpace(Configurations.ConfigSelect))
            {
                using (IDbConnection db = new SqlConnection(Configurations.ConnectionString))
                {
                    _logger.Debug("Retrieving configuration items");
                    var results = db.Query<KeyValuePair<string, string>>(Configurations.ConfigSelect);
                    _configs = results.ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
    }
}
