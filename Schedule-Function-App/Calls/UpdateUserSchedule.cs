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
    public static class UpdateUserSchedule
    {
        [FunctionName("UpdateUserSchedule")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            UpdatedUserSchedule availableTime = JsonConvert.DeserializeObject<UpdatedUserSchedule>(body);

            var str = Environment.GetEnvironmentVariable("sqldb_connection");

            if (availableTime.UpdateRecurring == true && availableTime.Recurring_Id != null)
            {
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "UPDATE UserSchedule " +
                    "SET StartTime = @StartTime, EndTime = @EndTime " +
                        "WHERE Recurring_Id = @Recurring_Id AND User_Id = @User_Id;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Timeslot_Id", availableTime.Timeslot_Id);
                        cmd.Parameters.AddWithValue("@User_Id", availableTime.User_Id);
                        cmd.Parameters.AddWithValue("@Recurring_Id", availableTime.Recurring_Id);
                        cmd.Parameters.AddWithValue("@StartTime", availableTime.StartTime);
                        cmd.Parameters.AddWithValue("@EndTime", availableTime.EndTime);
                        cmd.Parameters.AddWithValue("@IsRecurring", availableTime.IsRecurring);

                        // Execute the command and log the # rows affected.
                        var rows = await cmd.ExecuteNonQueryAsync();
                        log.LogInformation($"{rows} rows were updated");
                    }
                }
            }
            else if (availableTime.UpdateRecurring == false)
            {
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "UPDATE UserSchedule " +
                    "SET StartTime = @StartTime, EndTime = @EndTime " +
                        "WHERE Timeslot_Id = @Timeslot_Id AND User_Id = @User_Id;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Timeslot_Id", availableTime.Timeslot_Id);
                        cmd.Parameters.AddWithValue("@User_Id", availableTime.User_Id);
                        cmd.Parameters.AddWithValue("@Recurring_Id", (availableTime.Recurring_Id != null) ? availableTime.Recurring_Id : DBNull.Value);
                        cmd.Parameters.AddWithValue("@StartTime", availableTime.StartTime);
                        cmd.Parameters.AddWithValue("@EndTime", availableTime.EndTime);
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
