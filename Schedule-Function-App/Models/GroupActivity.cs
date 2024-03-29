﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Function_App.Models
{
    public class GroupActivity
    {
        public int Activity_Id { get; set; }
        public int Group_Id { get; set; }
        public string Activity_Name { get; set; }
        public string? Activity_Description { get; set; }
        public bool Limit { get; set; }
        public int? Minimum_Members { get; set;}
    }

    public class UpdatedActivity
    {
        public int Activity_Id { get; set; }
        public int Group_Id { get; set; }
        public string User_Id { get; set; }
        public string Activity_Name { get; set; }
        public string? Activity_Description { get; set; }
        public bool Limit { get; set; }
        public int? Minimum_Members { get; set; }
    }

    public class NewGroupActivity
    {
        public string User_Id { get; set; }
        public int Group_Id { get; set; }
        public string Activity_Name { get; set; }
        public string? Activity_Description { get; set; }
        public bool Limit { get; set; }
        public int? Minimum_Members { get; set; }
    }

    public class RemovedActivity
    {
        public string User_Id { get; set; }
        public int Group_Id { get; set; }
        public int Activity_Id { get; set; }
    }
}
