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
    public static class RemoveGroup
    {
        [FunctionName("RemoveGroup")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            RemovedGroup group = JsonConvert.DeserializeObject<RemovedGroup>(body);

            if (await Verify.IsAdmin(group.User_Id, group.Group_Id))
            {
                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "DELETE FROM GroupMembers " +
                            "WHERE Group_Id = @Group_Id " +
                                "DELETE FROM GroupInvites   " +
                                    "WHERE Group_Id = @Group_Id " +
                                "DELETE FROM GroupMeetings " +
                                    "WHERE Group_Id = @Group_Id " +
                                "DELETE FROM GroupActivities " +
                                    "WHERE Group_Id = @Group_Id " +
                                "DELETE FROM Groups " +
                                    "WHERE Group_Id = @Group_Id;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Group_Id", group.Group_Id);

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
