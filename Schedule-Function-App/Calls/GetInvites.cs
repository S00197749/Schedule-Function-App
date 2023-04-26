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
    public static class GetInvites
    {
        [FunctionName("GetInvites")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            int User_Id = int.Parse(req.Query["u"]);

            List<GroupInvite> invites = new List<GroupInvite>();

            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var query = "Select GroupInvites.Invite_Id, GroupInvites.Invite_Code, GroupInvites.Expired, Groups.Group_Name " +
                                    "From GroupInvites INNER JOIN Groups ON GroupInvites.Group_Id = Groups.Group_Id " +
                                         $"And User_Id = @User_Id AND Expired = @Expired;";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Add parameter.
                    cmd.Parameters.AddWithValue("@User_Id", User_Id);
                    cmd.Parameters.AddWithValue("@Expired", "false");

                    // Execute the command and log the # rows affected.
                    var rows = await cmd.ExecuteNonQueryAsync();
                    log.LogInformation($"{rows} rows were updated");

                    var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        GroupInvite invite = new GroupInvite()
                        {
                            Invite_Id = (int)reader["Invite_Id"],
                            Invite_Code = reader["Invite_Code"].ToString(),
                            Group_Name = reader["Group_Name"].ToString(),
                            Expired = (bool)reader["Expired"]
                        };
                        invites.Add(invite);
                    }
                }
            }
            if (invites.Count > 0)
            {
                return new OkObjectResult(invites);
            }
            return new NotFoundObjectResult("No Groups");
        }
    }
}
