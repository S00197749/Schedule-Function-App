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
        public int Group_Id { get; set;}
        public int User_Id { get; set;}
        public bool Accepted { get; set; }
    }
}
