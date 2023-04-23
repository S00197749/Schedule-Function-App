using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Function_App.Models
{
    public class GroupMeeting
    {
        public int Meeting_Id { get; set; }
        public int Group_Id { get; set; }
        public int Activity_Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
    public class NewGroupMeeting
    {
        public int Group_Id { get; set; }
        public int User_Id { get; set; }
        public int Activity_Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
