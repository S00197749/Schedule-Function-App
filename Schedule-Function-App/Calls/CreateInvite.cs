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
using System.Diagnostics;
using System.Linq;

namespace Schedule_Function_App
{
    public static class CreateInvite
    {
        private static Random random = new Random();

        [FunctionName("CreateInvite")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            NewGroupInvite invite = JsonConvert.DeserializeObject<NewGroupInvite>(body);

            if(await Verify.IsAdmin(invite.User_Id, invite.Group_Id))
            {
                string user_Id = await FindUserIdByEmail(invite.Email, log);
                string inviteCode = await GetRandomString();

                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "INSERT INTO GroupInvites (Group_Id, User_Id, Invite_Code, Expired) " +
                            "VALUES (@Group_Id, @User_Id, @Invite_Code, @Expired);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Group_Id", invite.Group_Id);
                        cmd.Parameters.AddWithValue("@User_Id", user_Id);
                        cmd.Parameters.AddWithValue("@Invite_Code", inviteCode);
                        cmd.Parameters.AddWithValue("@Expired", "false");

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

        public static async Task<string> FindUserIdByEmail(string email, ILogger log)
        {
            string user_Id = string.Empty;
            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var query = "Select User_Id " +
                        "From Users " +
                            "Where User_Email = @User_Email";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@User_Email", email);

                    // Execute the command and log the # rows affected.
                    var rows = await cmd.ExecuteNonQueryAsync();
                    log.LogInformation($"{rows} rows were updated");

                    var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        user_Id = reader["User_Id"].ToString();
                    }
                }
            }

            return user_Id;
        }

        public static async Task<string> GetRandomString()
        {           
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
