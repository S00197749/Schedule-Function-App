using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Schedule_Function_App.Models;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Schedule_Function_App
{
    public static class GetUserSchedule
    {
        [FunctionName("GetUserSchedule")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string User_Id = req.Query["u"].ToString();

            List<UserSchedule> userSchedules = new List<UserSchedule>();

            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var query = "Select Timeslot_Id, IsRecurring, Recurring_Id, StartTime, EndTime " +
                                "From UserSchedule " +
                                     $"Where User_Id = @User_Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Add parameter.
                    cmd.Parameters.AddWithValue("@User_Id", User_Id);

                    // Execute the command and log the # rows affected.
                    var rows = await cmd.ExecuteNonQueryAsync();
                    log.LogInformation($"{rows} rows were updated");

                    var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        UserSchedule userSchedule = new UserSchedule()
                        {
                            Timeslot_Id = (int)reader["Timeslot_Id"],
                            IsRecurring = (bool)reader["IsRecurring"],
                            Recurring_Id = (reader["Recurring_Id"].GetType().IsValueType) ? (int)reader["Recurring_Id"] : null,
                            StartTime = (DateTime)reader["StartTime"],
                            EndTime = (DateTime)reader["EndTime"],
                            IsReadonly = true,
                            Title = "Available"                           
                        };
                        userSchedule.StartTimeString = userSchedule.StartTime.ToString("dd/MM/yyyy HH:mm");
                        userSchedule.EndTimeString = userSchedule.EndTime.ToString("HH:mm");

                        userSchedules.Add(userSchedule);
                    }
                }
            }
            return new OkObjectResult(userSchedules);
        }
    }
}
