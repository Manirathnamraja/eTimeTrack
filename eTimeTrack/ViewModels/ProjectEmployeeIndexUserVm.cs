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
        public IEnumerable<SelectListItem> ProjectUserTypes { get; set; }
        public string ProjectRole { get; set; }
    }
}