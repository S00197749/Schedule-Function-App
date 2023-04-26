using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Function_App.Models
{
    public class GroupInvite
    {
        public int Invite_Id { get; set; }
        public int? Group_Id { get; set;}
        public int User_Id { get; set;}
        public string Invite_Code { get; set; }
        public string? Group_Name { get; set; }
        public bool Expired { get; set; }
    }
    public class NewGroupInvite
    {
        public int User_Id { get; set; }
        public int Group_Id { get; set; }
        public string Email { get; set; }
    }
    public class UpdatedGroupInvite
    {
        public int Invite_Id { get; set; }
        public int User_Id { get; set; }
        public string Invite_Code { get; set; }
        public bool Accepted { get; set; }
    }
}
