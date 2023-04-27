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
using System.Linq;
using System.Xml.Linq;

namespace Schedule_Function_App
{
    public static class GetUser
    {
        private static Random random = new Random();

        [FunctionName("GetUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            User data = JsonConvert.DeserializeObject<User>(body);

            User user = await GetUserInfo(data, log);

            if(user.User_Id == null)
            {
                string r1 = await GetRandomString();
                string r2 = await GetRandomString();

                string userId = (r1 + data.FirstName + r2);

                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "INSERT INTO Users (User_Id, First_Name, Last_Name, User_Email) " +
                                    "VALUES (@User_Id, @First_Name, @Last_Name, @User_Email);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameter.
                        cmd.Parameters.AddWithValue("@User_Id", userId);
                        cmd.Parameters.AddWithValue("@First_Name", data.FirstName);
                        cmd.Parameters.AddWithValue("@Last_Name", data.LastName);
                        cmd.Parameters.AddWithValue("@User_Email", data.User_Email);

                        // Execute the command and log the # rows affected.
                        var rows = await cmd.ExecuteNonQueryAsync();
                        log.LogInformation($"{rows} rows were updated");
                    }
                }

                user = await GetUserInfo(data, log);
                return new OkObjectResult(user);
            }
            else
                return new OkObjectResult(user);
        }
        public static async Task<User> GetUserInfo(User data, ILogger log)
        {
            User user = new User();

            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var query = "Select User_Id, First_Name, Last_Name " +
                                "From Users " +
                                    $"Where User_Email = @User_Email;";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Add parameter.
                    cmd.Parameters.AddWithValue("@User_Email", data.User_Email);

                    // Execute the command and log the # rows affected.
                    var rows = await cmd.ExecuteNonQueryAsync();
                    log.LogInformation($"{rows} rows were updated");

                    var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        user = new User()
                        {
                            User_Id = reader["User_Id"].ToString(),
                            LastName = reader["Last_Name"].ToString(),
                            FirstName = reader["First_Name"].ToString()
                        };
                    }
                }
            }
            return user;
        }

        public static async Task<string> GetRandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 3)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
