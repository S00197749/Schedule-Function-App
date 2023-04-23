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
    public static class CreateUserSchedule
    {
        [FunctionName("CreateUserSchedule")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            NewUserSchedule availableTime = JsonConvert.DeserializeObject<NewUserSchedule>(body);

            if (availableTime.Group_Id != null || availableTime.StartTime != null || availableTime.EndTime != null)
            {
                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "INSERT INTO UserSchedule (Group_Id, User_Id, Activity_Id, Recurring_Id, StartTime, EndTime) " +
                            "VALUES (@Group_Id, @User_Id, @Activity_Id, @Recurring_Id, @StartTime , @EndTime);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Group_Id", availableTime.Group_Id);
                        cmd.Parameters.AddWithValue("@User_Id", availableTime.User_Id);
                        cmd.Parameters.AddWithValue("@Activity_Id", availableTime.Activity_Id);
                        cmd.Parameters.AddWithValue("@Recurring_Id", availableTime.Recurring_Id);
                        cmd.Parameters.AddWithValue("@StartTime", availableTime.StartTime);
                        cmd.Parameters.AddWithValue("@EndTime", availableTime.EndTime);

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
