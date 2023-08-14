using eTimeTrack.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eTimeTrack.ViewModels
{
    public class ExportTimesheetDataViewModel
    {
        public int ProjectID { get; set; }
        public SelectList ProjectList { get; set; }

        public ProjectTask projectTask { get; set; }
        public ProjectVariation projectVariation { get; set; }
        public EmployeeTimesheetItem employeeTimesheetItem { get; set; } 
        public EmployeeTimesheet employeeTimesheet { get; set; }
        public TimesheetPeriod timesheetPeriod { get; set; }
        public Employee employee { get; set; }
        public ProjectTimeCodeConfig projectTimeCodeConfig { get; set; }
    }
}