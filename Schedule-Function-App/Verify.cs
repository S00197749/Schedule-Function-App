using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Schedule_Function_App.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Function_App
{
    public class Verify
    {
        public static async Task<bool> IsAdmin(string userId, int groupId)
        {
            int Role_Id = 0;
            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var query = "Select Role_Id " +
                                "From GroupMembers " +
                                     $"Where User_Id = @User_Id AND Group_Id = @Group_Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Add parameter.
                    cmd.Parameters.AddWithValue("@User_Id", userId);
                    cmd.Parameters.AddWithValue("@Group_Id", groupId);

                    // Execute the command and log the # rows affected.
                    var rows = await cmd.ExecuteNonQueryAsync();

                    var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        Role_Id = (int)reader["Role_Id"];
                    }
                }
            }

            if (Role_Id == 1)
                return true;
            else
                return false;
        }

        public static async Task<bool> IsMember(string userId, int groupId)
        {
            int Member_Id = 0;
            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var query = "Select Member_Id " +
                                "From GroupMembers " +
                                     $"Where User_Id = @User_Id AND Group_Id = @Group_Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Add parameter.
                    cmd.Parameters.AddWithValue("@User_Id", userId);
                    cmd.Parameters.AddWithValue("@Group_Id", groupId);

                    // Execute the command and log the # rows affected.
                    var rows = await cmd.ExecuteNonQueryAsync();

                    var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        Member_Id = (int)reader["Member_Id"];
                    }
                }
            }

            if (Member_Id != 0)
                return true;
            else
                return false;
        }
    }
}
