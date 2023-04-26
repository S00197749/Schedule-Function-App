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
    public static class UpdateInvite
    {
        [FunctionName("UpdateInvite")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,"post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            UpdatedGroupInvite invite = JsonConvert.DeserializeObject<UpdatedGroupInvite>(body);

            var str = Environment.GetEnvironmentVariable("sqldb_connection");

            if(invite.Accepted == true)
            {
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "Declare @GroupId int " +
                            "SELECT @GroupId = Groups.Group_Id " +
                                "FROM Groups INNER JOIN GroupInvites ON GroupInvites.Group_Id = Groups.Group_Id " +
                                    "AND GroupInvites.Invite_Id = @Invite_Id AND GroupInvites.Invite_Code = @Invite_Code AND GroupInvites.Expired = @NotExpired " +
                                        "INSERT INTO GroupMembers (Group_Id, User_Id, Role_Id) " +
                                            "Values (@GroupId, @User_Id, @Role_Id) " +
                                                "UPDATE GroupInvites " +
                                                    "Set Expired = @Expired " +
                                                        "Where Group_Id = @GroupId AND Invite_Id = @Invite_Id AND Invite_Code = @Invite_Code;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Invite_Id", invite.Invite_Id);
                        cmd.Parameters.AddWithValue("@Invite_Code", invite.Invite_Code);
                        cmd.Parameters.AddWithValue("@User_Id", invite.User_Id);
                        cmd.Parameters.AddWithValue("@Role_Id", 2);
                        cmd.Parameters.AddWithValue("@NotExpired", "false");
                        cmd.Parameters.AddWithValue("@Expired", "true");

                        // Execute the command and log the # rows affected.
                        var rows = await cmd.ExecuteNonQueryAsync();
                        log.LogInformation($"{rows} rows were updated");
                    }
                }
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "UPDATE GroupInvites " +
                                    "Set Expired = @Expired " +
                                        "Where Invite_Id = @Invite_Id AND Invite_Code = @Invite_Code;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Invite_Id", invite.Invite_Id);
                        cmd.Parameters.AddWithValue("@Invite_Code", invite.Invite_Code);
                        cmd.Parameters.AddWithValue("@Expired", "true");

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
