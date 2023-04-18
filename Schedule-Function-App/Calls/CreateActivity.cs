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
using Schedule_Function_App;

namespace Schedule_Function_App
{
    public static class CreateActivity
    {
        [FunctionName("CreateActivity")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            NewGroupActivity activity = JsonConvert.DeserializeObject<NewGroupActivity>(body);

            if(await Verify.IsAdmin(activity.User_Id, activity.Group_Id))
            {
                if (activity.Group_Id != null && activity.Activity_Name != null)
                {
                    var str = Environment.GetEnvironmentVariable("sqldb_connection");
                    using (SqlConnection conn = new SqlConnection(str))
                    {
                        conn.Open();
                        var query = "INSERT INTO GroupActivities (Group_Id, Activity_Name, Activity_Desc, Limit, Min_Members) " +
                                "VALUES (@Group_Id, @Activity_Name, @Activity_Desc , @Limit , @Min_Members);";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Group_Id", activity.Group_Id);
                            cmd.Parameters.AddWithValue("@Activity_Name", activity.Activity_Name);
                            cmd.Parameters.AddWithValue("@Activity_Desc", activity.Activity_Description);
                            cmd.Parameters.AddWithValue("@Limit", activity.Limit);
                            cmd.Parameters.AddWithValue("@Min_Members", activity.Minimum_Members);

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
