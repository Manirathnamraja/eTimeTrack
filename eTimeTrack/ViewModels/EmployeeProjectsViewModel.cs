using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class EmployeeProjectsViewModel
    {
        public int EmployeeProjectId { get; set; }
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }
        public int? ProjectUserTypeID { get; set; }
        public int? ProjectDisciplineID { get; set; }
        public int? OfficeID { get; set; }
        public string ProjectRole { get; set; }
        public ProjectUserType ProjectUserType { get; set; }
        public  ProjectDiscipline ProjectDiscipline { get; set; }
        public  Employee Employee { get; set; }
        public  Project Project { get; set; }
    }
}