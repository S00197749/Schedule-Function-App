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

            var str = Environment.GetEnvironmentVariable("sqldb_connection");

            if (availableTime.IsRecurring == true)
            {
                //Need to create functionality to set date to repeat every week for 26 weeks
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "INSERT INTO UserSchedule (User_Id, IsRecurring, Recurring_Id, StartTime, EndTime) " +
                            "VALUES (@User_Id, @IsRecurring, @Recurring_Id, @StartTime , @EndTime);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@User_Id", availableTime.User_Id);
                        cmd.Parameters.AddWithValue("@StartTime", availableTime.StartTime);
                        cmd.Parameters.AddWithValue("@EndTime", availableTime.EndTime);
                        cmd.Parameters.AddWithValue("@Recurring_Id", DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsRecurring", availableTime.IsRecurring);

                        // Execute the command and log the # rows affected.
                        var rows = await cmd.ExecuteNonQueryAsync();
                        log.LogInformation($"{rows} rows were updated");
                    }
                }
            }
            else if (availableTime.IsRecurring == false)
            {
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "INSERT INTO UserSchedule (User_Id, IsRecurring, Recurring_Id, StartTime, EndTime) " +
                            "VALUES (@User_Id, @IsRecurring, @Recurring_Id, @StartTime , @EndTime);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@User_Id", availableTime.User_Id);
                        cmd.Parameters.AddWithValue("@StartTime", availableTime.StartTime);
                        cmd.Parameters.AddWithValue("@EndTime", availableTime.EndTime);
                        cmd.Parameters.AddWithValue("@Recurring_Id", DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsRecurring", availableTime.IsRecurring);

                        // Execute the command and log the # rows affected.
                        var rows = await cmd.ExecuteNonQueryAsync();
                        log.LogInformation($"{rows} rows were updated");
                    }
                }
            }
            
            string responseMessage = $"This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);

        }
    }
}
