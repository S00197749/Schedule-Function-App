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
            int User_Id = int.Parse(req.Query["u"]);
            int Group_Id = int.Parse(req.Query["g"]);

            List<GroupMeeting> groupMeetings = new List<GroupMeeting>();
            List<LimitedUserSchedule> userSchedules = new List<LimitedUserSchedule>();
            List<GroupAvailableTime> groupSchedules = new List<GroupAvailableTime>();

            if (await Verify.IsMember(User_Id, Group_Id))
            {
                groupMeetings = await GetGroupMeetings(Group_Id, log);
                userSchedules = await GetUserSchedules(Group_Id, log);

                List<LimitedGroupAvailableTime> allAvailableTimes = new List<LimitedGroupAvailableTime>();

                foreach(var groupMeeting in groupMeetings)
                {
                    allAvailableTimes.Add(new LimitedGroupAvailableTime
                    {
                        Meeting_Id = groupMeeting.Meeting_Id,
                        Group_Id = groupMeeting.Group_Id,
                        Activity_Id = groupMeeting.Activity_Id,
                        Activity_Name = groupMeeting.Activity_Name,
                        StartTime = groupMeeting.StartTime,
                        EndTime = groupMeeting.EndTime,
                        EventType = groupMeeting.Activity_Name,
                        IsReadonly = true,
                        StartTimeString = groupMeeting.StartTime.ToString("dd/MM/yyyy HH:mm"),
                        EndTimeString = groupMeeting.EndTime.ToString("HH:mm")
                    });
                }

                foreach (var userSchedule in userSchedules)
                {
                    allAvailableTimes.Add(new LimitedGroupAvailableTime
                    {
                        StartTime = userSchedule.StartTime,
                        EndTime = userSchedule.EndTime,
                        EventType = "Available",
                        IsReadonly = true,
                        StartTimeString = userSchedule.StartTime.ToString("dd/MM/yyyy HH:mm"),
                        EndTimeString = userSchedule.EndTime.ToString("HH:mm")
                    });
                }


                if (allAvailableTimes.Count > 0)
                {
                    return new OkObjectResult(allAvailableTimes);
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
                var query = "Select GroupMeetings.Meeting_Id, GroupMeetings.Group_Id, GroupMeetings.StartTime, GroupMeetings.EndTime, GroupMeetings.Activity_Id, GroupActivities.Activity_Name " +
                                "From GroupMeetings INNER JOIN GroupActivities ON GroupMeetings.Activity_Id = GroupActivities.Activity_Id " +
                                     $"AND GroupMeetings.Group_Id = @Group_Id";

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
                            Activity_Name = reader["Activity_Name"].ToString(),
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
                var query = "Select UserSchedule.StartTime, UserSchedule.EndTime, GroupMembers.Member_Id " +
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
