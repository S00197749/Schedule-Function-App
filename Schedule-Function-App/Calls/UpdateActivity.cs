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
using Schedule_Function_App.Models;
using System.Reflection.Metadata;

namespace Schedule_Function_App
{
    public static class UpdateActivity
    {
        [FunctionName("UpdateActivity")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            UpdatedActivity groupActivity = JsonConvert.DeserializeObject<UpdatedActivity>(body);

            if (await Verify.IsAdmin(groupActivity.User_Id, groupActivity.Group_Id))
            {
                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "UPDATE GroupActivities " +
                            "SET Activity_Name = @Activity_Name, Activity_Desc = @Activity_Desc , Limit = @Limit , Min_Members = @Min_Members " +
                                "WHERE Activity_Id = @Activity_Id AND Group_Id = @Group_Id;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Activity_Id", groupActivity.Activity_Id);
                        cmd.Parameters.AddWithValue("@Group_Id", groupActivity.Group_Id);
                        cmd.Parameters.AddWithValue("@Activity_Name", groupActivity.Activity_Name);
                        cmd.Parameters.AddWithValue("@Activity_Desc", groupActivity.Activity_Description);
                        cmd.Parameters.AddWithValue("@Limit", groupActivity.Limit);
                        cmd.Parameters.AddWithValue("@Min_Members", groupActivity.Minimum_Members);

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
                string responseMessage = "You must be an Admin of this group to create this request.";

                return new BadRequestObjectResult(responseMessage);
            }

        }
    }
}
