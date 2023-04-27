using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Function_App.Models
{
    public class Group
    {
        public int Group_Id { get; set; }
        public string Group_Name { get; set; }
        public string? Group_Description { get; set; }
        public string? Group_Image { get; set;}
        public GroupActivity[] Activities { get; set; }
        public GroupMember[] Members { get; set; }

    }
    public class NewGroup
    {
        public string User_Id { get; set; }
        public string Group_Name { get; set; }
        public string? Group_Description { get; set; }
        public string? Group_Image { get; set; }
    }
    public class UpdatedGroup
    {
        public string User_Id { get; set; }
        public int Group_Id { get; set; }
        public string Group_Name { get; set; }
        public string? Group_Description { get; set; }
        public string? Group_Image { get; set; }
    }

    public class RemovedGroup
    {
        public string User_Id { get; set; }
        public int Group_Id { get; set; }
    }
}
