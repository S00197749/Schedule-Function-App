using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Function_App.Models
{
    public class GroupMember
    {
        public int Member_Id { get; set; }
        public int Group_Id { get; set; }
        public string User_Id { get; set; }
        public int Role_Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
    public class RemovedMember
    {
        public string User_Id { get; set; }
        public int Member_Id { get; set; }
        public int Group_Id { get; set; }
    }
}
