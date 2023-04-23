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
    public static class CreateGroup
    {
        [FunctionName("CreateGroup")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            NewGroup group = JsonConvert.DeserializeObject<NewGroup>(body);

            if (group.Group_Name != null || group.User_Id != null) { 
                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "INSERT INTO Groups (Group_Name, Group_Desc) " +
                            "VALUES (@Group_Name, @Group_Desc) " +
                                "INSERT INTO GroupMembers (Group_Id, User_Id, Role_Id) " +
                                    "VALUES (SCOPE_IDENTITY(), @User_Id, @Role_Id);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@User_Id", group.User_Id);
                        cmd.Parameters.AddWithValue("@Role_Id", 1);
                        cmd.Parameters.AddWithValue("@Group_Name", group.Group_Name);
                        cmd.Parameters.AddWithValue("@Group_Desc", group.Group_Description);

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
                string responseMessage = "This HTTP triggered function executed successfully. Pass Group info in the query string or in the request body for a response.";

                return new OkObjectResult(responseMessage);
            }
        }
    }
}
