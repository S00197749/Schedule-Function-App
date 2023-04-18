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
    }
    public class NewGroup
    {
        public int User_Id { get; set; }
        public string Group_Name { get; set; }
        public string? Group_Description { get; set; }
        public string? Group_Image { get; set; }
    }
    public class UpdatedGroup
    {
        public int User_Id { get; set; }
        public int Group_Id { get; set; }
        public string Group_Name { get; set; }
        public string? Group_Description { get; set; }
        public string? Group_Image { get; set; }
    }

    public class RemovedGroup
    {
        public int User_Id { get; set; }
        public int Group_Id { get; set; }
    }
}
