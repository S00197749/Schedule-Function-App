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
    public static class RemoveUserSchedule
    {
        [FunctionName("RemoveUserSchedule")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            RemovedUserSchedule availableTime = JsonConvert.DeserializeObject<RemovedUserSchedule>(body);

            var str = Environment.GetEnvironmentVariable("sqldb_connection");

            if(availableTime.RemoveRecurring == true && availableTime.Recurring_Id != null)
            {
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "DELETE FROM UserSchedule " +
                            "WHERE Recurring_Id = @Recurring_Id AND User_Id = @User_Id;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Recurring_Id", availableTime.Recurring_Id);
                        cmd.Parameters.AddWithValue("@User_Id", availableTime.User_Id);

                        // Execute the command and log the # rows affected.
                        var rows = await cmd.ExecuteNonQueryAsync();
                        log.LogInformation($"{rows} rows were updated");
                    }
                }
            }
            else if(availableTime.RemoveRecurring == false)
            {
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "DELETE FROM UserSchedule " +
                            "WHERE Timeslot_Id = @Timeslot_Id AND User_Id = @User_Id;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Timeslot_Id", availableTime.Timeslot_Id);
                        cmd.Parameters.AddWithValue("@User_Id", availableTime.User_Id);

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
