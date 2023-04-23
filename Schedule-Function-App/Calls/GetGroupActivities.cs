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
    public static class GetGroupActivities
    {
        [FunctionName("GetGroupActivities")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            int User_Id = int.Parse(req.Query["u"]);
            int Group_Id = int.Parse(req.Query["g"]);

            List<GroupActivity> activityList = new List<GroupActivity>();

            if (await Verify.IsMember(User_Id, Group_Id))
            {
                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "Select * " +
                                    "From GroupActivities " +
                                         $"Where Group_Id = @Group_Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameter.
                        cmd.Parameters.AddWithValue("@Group_Id", Group_Id);

                        // Execute the command and log the # rows affected.
                        var rows = await cmd.ExecuteNonQueryAsync();
                        log.LogInformation($"{rows} rows were updated");

                        var reader = await cmd.ExecuteReaderAsync();
                        while (reader.Read())
                        {
                            GroupActivity activity = new GroupActivity()
                            {
                                Activity_Id = (int)reader["Activity_Id"],
                                Group_Id = (int)reader["Group_Id"],
                                Activity_Name = reader["Activity_Name"].ToString(),
                                Activity_Description = reader["Activity_Desc"].ToString(),
                                Limit = (bool)reader["Limit"],
                                Minimum_Members = (reader["Min_Members"].GetType().IsValueType) ? (int)reader["Min_Members"] : null
                            };
                            activityList.Add(activity);
                        }
                    }
                }
                if (activityList.Count > 0)
                {
                    return new OkObjectResult(activityList);
                }
                return new NotFoundObjectResult("No Activities");
            }
            else
            {
                string responseMessage = "You must be a member of this group to create this request.";

                return new BadRequestObjectResult(responseMessage);
            }

        }
    }
}
