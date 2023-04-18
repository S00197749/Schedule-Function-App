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

namespace Schedule_Function_App
{
    public static class RemoveActivity
    {
        [FunctionName("RemoveActivity")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            RemovedActivity activity = JsonConvert.DeserializeObject<RemovedActivity>(body);

            if (await Verify.IsAdmin(activity.User_Id, activity.Group_Id))
            {
                if (activity.User_Id != null && activity.Activity_Id != null)
                {
                    var str = Environment.GetEnvironmentVariable("sqldb_connection");
                    using (SqlConnection conn = new SqlConnection(str))
                    {
                        conn.Open();
                        var query = "DELETE FROM GroupActivities " +
                                "WHERE Activity_Id = @Activity_Id;";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Activity_Id", activity.Activity_Id);

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
            else
            {
                string responseMessage = "You must be an Admin of this group to create this request.";

                return new BadRequestObjectResult(responseMessage);
            }

        }
    }
}
