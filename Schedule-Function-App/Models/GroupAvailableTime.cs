using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Function_App.Models
{
    public class GroupAvailableTime
    {
        public int? Activity_Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class LimitedGroupAvailableTime
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string EventType { get; set; }
        public bool IsReadonly { get; set; }
        public string? StartTimeString { get; set; }
        public string? EndTimeString { get; set; }
        public int? Meeting_Id { get; set; }
        public int? Group_Id { get; set; }
        public int? Activity_Id { get; set; }
        public string? Activity_Name { get; set; }
    }
}
