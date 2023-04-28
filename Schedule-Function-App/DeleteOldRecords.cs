using System;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Schedule_Function_App
{
    public class DeleteOldRecords
    {
        [FunctionName("DeleteOldRecords")]
        public void Run([TimerTrigger("0 0 12 * * */7")]TimerInfo myTimer, ILogger log)
        {
            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var query = "DELETE FROM UserSchedule " +
                                "WHERE EndTime < GETDATE() " +
                                    "DELETE FROM GroupMeetings " +
                                        "WHERE EndTime < GETDATE() " +
                                            "DELETE FROM GroupInvites " +
                                                "WHERE Expired = 'true';";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Execute the command and log the # rows affected.
                    var rows = cmd.ExecuteNonQueryAsync();
                    log.LogInformation($"{rows} rows were updated");
                }
            }
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
