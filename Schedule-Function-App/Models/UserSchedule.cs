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
        public int? Group_Id { get; set; }
        public int User_Id { get; set; }
        public int? Recurring_Id { get; set; }
        public DateTime Time_Available { get; set; }
    }
}
