using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class ProjectEmployeeIndexUserVm
    {
        public List<EmployeeProject> EmployeeProjects { get; set; }
        public int? ProjectUserTypeIdFilter { get; set; }
        public int? ProjectDisciplineIdFilter { get; set; }
        public int? OfficeIdFilter { get; set; }
        public IEnumerable<SelectListItem> ProjectUserTypes { get; set; }
        public IEnumerable<SelectListItem> ProjectDisciplines { get; set; }
        public IEnumerable<SelectListItem> Offices { get; set; }
        public string ProjectRole { get; set; }
    }
}