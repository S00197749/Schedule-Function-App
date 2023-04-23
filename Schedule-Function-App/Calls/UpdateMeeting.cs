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
    public static class UpdateMeeting
    {
        [FunctionName("UpdateMeeting")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            UpdatedGroupMeeting groupMeeting = JsonConvert.DeserializeObject<UpdatedGroupMeeting>(body);

            if (await Verify.IsAdmin(groupMeeting.User_Id, groupMeeting.Group_Id))
            {
                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "UPDATE GroupMeetings " +
                            "SET Activity_Id = @Activity_Id, StartTime = @StartTime, EndTime = @EndTime " +
                                "WHERE Meeting_Id = @Meeting_Id;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Activity_Id", groupMeeting.Activity_Id);
                        cmd.Parameters.AddWithValue("@StartTime", groupMeeting.StartTime);
                        cmd.Parameters.AddWithValue("@EndTime", groupMeeting.EndTime);
                        cmd.Parameters.AddWithValue("@Meeting_Id", groupMeeting.Meeting_Id);

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
