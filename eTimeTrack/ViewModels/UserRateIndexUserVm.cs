using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class UserRateIndexUserVm
    {
        public List<UserRate> UserRates { get; set; }
        public List<EmployeeProject> EmployeeProjects { get; set; }
    }
}