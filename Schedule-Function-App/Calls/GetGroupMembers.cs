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
    public static class GetGroupMembers
    {
        [FunctionName("GetGroupMembers")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            List<GroupMembers> memberList = new List<GroupMembers>();
            int group_id = int.Parse(req.Query["group_id"]);

            if (group_id != null)
            {
                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "Select GroupMembers.Member_Id, GroupMembers.Group_Id, GroupMembers.User_Id, GroupMembers.Role_Id, Users.User_Name " +
                                    "From GroupMembers INNER JOIN Users ON GroupMembers.User_Id = Users.User_Id " +
                                         $"Where Group_Id = @Group_Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameter.
                        cmd.Parameters.AddWithValue("@Group_Id", group_id);

                        // Execute the command and log the # rows affected.
                        var rows = await cmd.ExecuteNonQueryAsync();
                        log.LogInformation($"{rows} rows were updated");

                        var reader = await cmd.ExecuteReaderAsync();
                        while (reader.Read())
                        {
                            GroupMembers member = new GroupMembers()
                            {
                                Member_Id = (int)reader["Member_Id"],
                                Group_Id = (int)reader["Group_Id"],
                                User_Id = (int)reader["User_Id"],
                                Role_Id = (int)reader["Role_Id"],
                                User_Name = reader["User_Name"].ToString()
                            };
                            memberList.Add(member);
                        }
                    }
                }
                if (memberList.Count > 0)
                {
                    return new OkObjectResult(memberList);
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
