using Dapper;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QueueProcessor.DAL
{
    public class QueueItem
    {
        internal static string TableName { get { return $"tbl{Configurations.QueueName}Queue"; } }

        internal static string ProcedureName { get { return $"{Configurations.QueueName}DeQueue"; } }

        public int Id { get; set; }

        public string Parms { internal get; set; }
        public JObject JParams
        {
            get
            {
                return JObject.Parse(Parms);
            }
        }

        public static void DropQueueTable()
        {
            try
            {
                using (IDbConnection db = new SqlConnection(Configurations.ConnectionString))
                {
                    db.Execute($"IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('{ProcedureName}')) exec('DROP PROCEDURE [dbo].[{ProcedureName}]')");
                    db.Execute($"IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = '{TableName}') exec('DROP TABLE [dbo].[{TableName}]')");
                    log4net.LogManager.GetLogger("QueueItem").Debug($"table dropped");
                }
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("QueueItem").Error($"Error dropping table or procedure: {ex}");
            }
        }

        public static void CreateQueueTable()
        {
            RunSQL("QueueProcessor.DAL.tblQueue.sql");
            RunSQL("QueueProcessor.DAL.Dequeue.sql", true);
        }

        private static void RunSQL(string name, bool proc = false)
        {
            try
            {
                var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
                using (var sr = new StreamReader(s))
                using (IDbConnection db = new SqlConnection(Configurations.ConnectionString))
                {
                    if (proc)
                        db.Execute($"IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('{ProcedureName}')) exec('CREATE PROCEDURE [dbo].[{ProcedureName}] AS BEGIN SET NOCOUNT ON; END')");

                    var sql = sr.ReadToEnd().Replace("<TABLE_NAME>", TableName).Replace("<PROC_NAME>", ProcedureName).Replace("<QUEUE_NAME>", Configurations.QueueName);
                    db.Execute(sql);
                    log4net.LogManager.GetLogger("QueueItem").Debug($"{(proc? "procedure" : "table")} created");
                }
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("QueueItem").Error($"Error creating table or procedure: {ex}");
            }
        }

        public static void ResetItems()
        {
            using (IDbConnection db = new SqlConnection(Configurations.ConnectionString))
            {
                string query = $"Update [{TableName}] set [Status] = 'Ready', [ExtendedStatus] = '' where MachineName = '{Environment.MachineName}' and [Status] NOT in ('Finished', 'Canceled')";
                db.Execute(query);
            }

        }

        public static QueueItem DeQueue()
        {
            using (IDbConnection db = new SqlConnection(Configurations.ConnectionString))
            {
                return db.Query<QueueItem>(ProcedureName, new { machine = Environment.MachineName },
                    commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();

            }
        }

        public void SetStatus(string status, string extendedStatus = "", ILog logger = null)
        {
            if (logger == null)
                logger = log4net.LogManager.GetLogger("QueueItem SetStatus");

            using (IDbConnection db = new SqlConnection(Configurations.ConnectionString))
            {
                logger.Debug($"Setting status on queued id {Id}: {status}");
                if (this.Id > 0)
                {
                    string query = $"Update [{TableName}] set [Status] = '{status}', [ExtendedStatus] = '{extendedStatus.Replace("'", "''")}', [StatusTime] = '{DateTime.Now}'";
                    if (status == "Finished")
                        query += $", [CompleteTime] = '{DateTime.Now}'";
                    query += " where CalcQueueId = {Id}";
                    db.Execute(query);
                }
            }
        }
    }
}
