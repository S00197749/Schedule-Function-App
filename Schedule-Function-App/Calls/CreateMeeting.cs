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
    public static class CreateMeeting
    {
        [FunctionName("CreateMeeting")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            NewGroupMeeting meeting = JsonConvert.DeserializeObject<NewGroupMeeting>(body);

            if(await Verify.IsAdmin(meeting.User_Id, meeting.Group_Id))
            {
                if (meeting.Group_Id != null || meeting.StartTime != null || meeting.EndTime != null)
                {
                    var str = Environment.GetEnvironmentVariable("sqldb_connection");
                    using (SqlConnection conn = new SqlConnection(str))
                    {
                        conn.Open();
                        var query = "INSERT INTO GroupMeetings (Group_Id, Activity_Id, StartTime, EndTime) " +
                                "VALUES (@Group_Id, @Activity_Id, @StartTime , @EndTime);";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Group_Id", meeting.Group_Id);
                            cmd.Parameters.AddWithValue("@Activity_Id", meeting.Activity_Id);
                            cmd.Parameters.AddWithValue("@StartTime", meeting.StartTime);
                            cmd.Parameters.AddWithValue("@EndTime", meeting.EndTime);

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
