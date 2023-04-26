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
            int User_Id = int.Parse(req.Query["u"]);
            int Group_Id = int.Parse(req.Query["g"]);

            List<GroupMember> memberList = new List<GroupMember>();

            if (await Verify.IsMember(User_Id, Group_Id))
            {
                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "Select GroupMembers.Member_Id, GroupMembers.Group_Id, GroupMembers.Role_Id, Users.User_Name " +
                                    "From GroupMembers INNER JOIN Users ON GroupMembers.User_Id = Users.User_Id " +
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
                            GroupMember member = new GroupMember()
                            {
                                Member_Id = (int)reader["Member_Id"],
                                Group_Id = (int)reader["Group_Id"],
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
                return new NotFoundObjectResult("No Members");
            }
            else
            {
                string responseMessage = "You must be a member of this group to create this request."; ;

                return new BadRequestObjectResult(responseMessage);
            }
        }
    }
}
