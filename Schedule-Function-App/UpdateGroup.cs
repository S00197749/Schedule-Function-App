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
    public static class UpdateGroup
    {
        [FunctionName("UpdateGroup")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            int group_id = int.Parse(req.Query["group_id"]);
            string group_name = req.Query["group_name"];
            string group_desc = req.Query["group_desc"];

            if (group_id != null && group_name != null && group_desc != null) { 
                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "UPDATE Groups " +
                            "SET Group_Name = @Group_Name, Group_Desc = @Group_Desc " +
                                "WHERE Group_Id = @Group_Id;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Group_Id", group_id);
                        cmd.Parameters.AddWithValue("@Group_Name", group_name);
                        cmd.Parameters.AddWithValue("@Group_Desc", group_desc);

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
