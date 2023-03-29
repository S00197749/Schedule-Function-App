using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Function_App.Models
{
    public class GroupMembers
    {
        public int Member_Id { get; set; }
        public int Group_Id { get; set; }
        public int User_Id { get; set; }
        public int Role_Id { get; set; }
        public string User_Name { get; set; }
    }
}
