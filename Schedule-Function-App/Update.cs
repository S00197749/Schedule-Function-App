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

namespace Schedule_Function_App
{
    public static class Update
    {
        [FunctionName("Update")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string status = req.Query["status"];

            if(status != null) { 
                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var query = "UPDATE SalesLT.SalesOrderHeader " +
                            $"SET [Status] = {status}  WHERE ShipDate < GetDate();";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
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
                string responseMessage = "This HTTP triggered function executed successfully. Pass a Username and Password in the query string or in the request body for a response.";

                return new OkObjectResult(responseMessage);
            }
        }
    }
}
