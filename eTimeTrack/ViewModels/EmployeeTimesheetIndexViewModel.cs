using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class EmployeeTimesheetIndexViewModel
    {
        public Employee Employee { get; set; }
        public List<OpenEmployeeTimesheet> EmployeeTimesheets  { get; set; }
    }

    public class OpenEmployeeTimesheet
    {
        public EmployeeTimesheet EmployeeTimesheet { get; set; }
        public bool Open { get; set; }
    }
}