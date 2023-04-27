using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Function_App.Models
{
    public class UserSchedule
    {
        public int Timeslot_Id { get; set; }
        public string? User_Id { get; set; }
        public bool IsRecurring { get; set; }
        public bool? IsReadonly { get; set; }
        public int? Recurring_Id { get; set; }
        public string? Title { get; set; }
        public string? StartTimeString { get; set; }
        public string? EndTimeString { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
    public class UpdatedUserSchedule
    {
        public int Timeslot_Id { get; set; }
        public string User_Id { get; set; }
        public bool IsRecurring { get; set; }
        public bool UpdateRecurring { get; set; }
        public int? Recurring_Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
    public class NewUserSchedule
    {
        public string User_Id { get; set; }
        public bool IsRecurring { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
    public class RemovedUserSchedule
    {
        public int Timeslot_Id { get; set; }
        public string User_Id { get; set; }
        public bool RemoveRecurring { get; set; }
        public int? Recurring_Id { get; set; }
    }
    public class LimitedUserSchedule
    {
        public int Member_Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
