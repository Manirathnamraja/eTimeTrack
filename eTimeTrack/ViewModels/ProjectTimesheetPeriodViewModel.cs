using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class ProjectTimesheetPeriodViewModel
    {
        public TimesheetPeriod TimesheetPeriod { get; set; }
        public bool ContainsExisting { get; set; }
    }
}