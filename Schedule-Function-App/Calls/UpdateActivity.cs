using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace Schedule_Function_App
{
    public static class UpdateActivity
    {
        [FunctionName("UpdateActivity")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            int activity_id = int.Parse(req.Query["activity_id"]);
            int group_id = int.Parse(req.Query["group_id"]);
            string activity_name = req.Query["activity_name"];
            string activity_desc = req.Query["activity_desc"];
            bool limit = bool.Parse(req.Query["limit"]);
            int? min_members = int.Parse(req.Query["min_members"]);

            if (activity_id != null && activity_name != null) { 
                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "UPDATE GroupActivities " +
                            "SET Activity_Name = @Activity_Name, Activity_Desc = @Activity_Desc , Limit = @Limit , Min_Members = @Min_Members " +
                                "WHERE Activity_Id = @Activity_Id AND Group_Id = @Group_Id;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Activity_Id", activity_id);
                        cmd.Parameters.AddWithValue("@Group_Id", group_id);
                        cmd.Parameters.AddWithValue("@Activity_Name", activity_name);
                        cmd.Parameters.AddWithValue("@Activity_Desc", activity_desc);
                        cmd.Parameters.AddWithValue("@Limit", limit);
                        cmd.Parameters.AddWithValue("@Min_Members", min_members);

                        // Execute the command and log the # rows affected.
                        var rows = await cmd.ExecuteNonQueryAsync();
                        log.LogInformation($"{rows} rows were updated");
                    }
                }

                string responseMessage = $"This HTTP triggered function executed successfully.";

                return new OkObjectResult(responseMessage);
            }
            else
            {
                string responseMessage = "This HTTP triggered function executed successfully. Pass Group info in the query string or in the request body for a response.";

                return new OkObjectResult(responseMessage);
            }
        }
    }
}
