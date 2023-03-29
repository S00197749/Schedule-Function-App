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
    public static class GetGroups
    {
        [FunctionName("GetGroups")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            List<Group> groupList = new List<Group>();
            int user_id = int.Parse(req.Query["user_id"]);

            if (user_id != null)
            {
                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "Select Groups.Group_Id, Groups.Group_Name, Groups.Group_Desc, Groups.Group_Image "+
                                    "From Groups INNER JOIN GroupMembers ON Groups.Group_Id = GroupMembers.Group_Id " +
                                         $"AND User_Id = @User_id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameter.
                        cmd.Parameters.AddWithValue("@User_id", user_id);

                        // Execute the command and log the # rows affected.
                        var rows = await cmd.ExecuteNonQueryAsync();
                        log.LogInformation($"{rows} rows were updated");

                        var reader = await cmd.ExecuteReaderAsync();
                        while (reader.Read())
                        {
                            Group group = new Group()
                            {
                                Group_Id = (int)reader["Group_Id"],
                                Group_Name = reader["Group_Name"].ToString(),
                                Group_Description = reader["Group_Desc"].ToString(),
                                Group_Image = reader["Group_Image"].ToString()
                            };
                            groupList.Add(group);
                        }
                    }
                }
                if (groupList.Count > 0)
                {
                    return new OkObjectResult(groupList);
                }
                return new OkObjectResult("No Groups");
            }
            else
            {
                string responseMessage = "This HTTP triggered function executed successfully. Pass a User_Id query string or in the request body for a response.";

                return new OkObjectResult(responseMessage);
            }
        }
    }
}