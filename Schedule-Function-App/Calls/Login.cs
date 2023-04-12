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
using System.Xml.Linq;
using System.Data;
using System.Collections.Generic;
using Schedule_Function_App.Models;
using System.Diagnostics;

namespace Schedule_Function_App
{
    public static class Login
    {
        [FunctionName("Login")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            List<User> userList = new List<User>();
            string username = req.Query["username"];
            string password = req.Query["password"];

            if (username != null && password != null)
            {
                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "Select * From dbo.Users " +
                            $"Where [User_Name] = ('" + username + "') And [User_Password] = ('" + password + "');";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Execute the command and log the # rows affected.
                        var rows = await cmd.ExecuteNonQueryAsync();
                        log.LogInformation($"{rows} rows were updated");

                        var reader = await cmd.ExecuteReaderAsync();
                        while (reader.Read())
                        {
                            User user = new User()
                            {
                                User_Id = (int)reader["user_Id"],
                                User_Name = reader["user_Name"].ToString(),
                                User_Email = reader["user_Email"].ToString(),
                                User_Password = reader["user_Password"].ToString()
                            };
                            userList.Add(user);
                        }
                    }
                }
                if(userList.Count > 0)
                {
                    return new OkObjectResult(userList);
                }
                return new OkObjectResult("No User");
            }
            else
            {
                string responseMessage =  "This HTTP triggered function executed successfully. Pass a Username and Password in the query string or in the request body for a response.";

                return new OkObjectResult(responseMessage);
            }
        }
    }
}
