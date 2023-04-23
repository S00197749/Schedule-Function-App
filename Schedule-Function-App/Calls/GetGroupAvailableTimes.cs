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
using System.Diagnostics;

namespace Schedule_Function_App
{
    public static class GetGroupAvailableTimes
    {
        [FunctionName("GetGroupAvailableTimes")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            int User_Id = JsonConvert.DeserializeObject<int>(body);
            int Group_Id = JsonConvert.DeserializeObject<int>(body);

            List<GroupMeeting> groupMeetings = new List<GroupMeeting>();
            List<LimitedUserSchedule> userSchedules = new List<LimitedUserSchedule>();
            List<GroupAvailableTime> groupSchedules = new List<GroupAvailableTime>();

            if (await Verify.IsMember(User_Id, Group_Id))
            {
                groupMeetings = await GetGroupMeetings(Group_Id, log);
                userSchedules = await GetUserSchedules(Group_Id, log);

                List<LimitedGroupAvailableTime> allNonAvailableTimes = new List<LimitedGroupAvailableTime>();

                foreach(var groupMeeting in groupMeetings)
                {
                    allNonAvailableTimes.Add(new LimitedGroupAvailableTime
                    {
                        StartTime = groupMeeting.StartTime,
                        EndTime = groupMeeting.EndTime,
                    });
                }

                foreach (var userSchedule in userSchedules)
                {
                    allNonAvailableTimes.Add(new LimitedGroupAvailableTime
                    {
                        StartTime = userSchedule.StartTime,
                        EndTime = userSchedule.EndTime,
                    });
                }


                if (allNonAvailableTimes.Count > 0)
                {
                    return new OkObjectResult(allNonAvailableTimes);
                }
                return new OkObjectResult("No Schedule");
            }
            else
            {
                string responseMessage = "You must be a member of this group to create this request."; ;

                return new BadRequestObjectResult(responseMessage);
            }
        }

        public static async Task<List<GroupMeeting>> GetGroupMeetings(int Group_Id, ILogger log)
        {
            List<GroupMeeting> groupMeetings = new List<GroupMeeting>();

            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var query = "Select GroupMeetings.Meeting_Id, GroupMeetings.Group_Id, GroupMeetings.StartTime, GroupMeetings.EndTime, GroupMeetings.Activity " +
                                "From GroupMeetings INNER JOIN Users ON GroupMembers.User_Id = Users.User_Id " +
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
                        GroupMeeting meeting = new GroupMeeting()
                        {
                            Meeting_Id = (int)reader["Meeting_Id"],
                            Activity_Id = (int)reader["Activity_Id"],
                            Group_Id = (int)reader["Group_Id"],
                            StartTime = (DateTime)reader["StartTime"],
                            EndTime = (DateTime)reader["EndTime"]
                        };
                        groupMeetings.Add(meeting);
                    }
                }
            }
            return groupMeetings;
        }
        public static async Task<List<LimitedUserSchedule>> GetUserSchedules(int Group_Id, ILogger log)
        {
            List<LimitedUserSchedule> userSchedules = new List<LimitedUserSchedule>();

            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var query = "Select UserSchedule.User_Id, UserSchedule.StartTime, UserSchedule.EndTime, GroupMembers.Member_Id " +
                                "From UserSchedule INNER JOIN GroupMembers ON UserSchedule.User_Id = GroupMembers.User_Id " +
                                     $"Where UserSchedule.EndTime >= GETDATE() AND GroupMembers.Group_Id = @Group_Id";

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
                        LimitedUserSchedule userSchedule = new LimitedUserSchedule()
                        {
                            User_Id = (int)reader["User_Id"],
                            Member_Id = (int)reader["Member_Id"],
                            StartTime = (DateTime)reader["StartTime"],
                            EndTime = (DateTime)reader["EndTime"]
                        };
                        userSchedules.Add(userSchedule);
                    }
                }
            }
            return userSchedules;
        }
    }
}
