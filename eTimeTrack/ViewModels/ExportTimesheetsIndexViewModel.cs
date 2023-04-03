using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Xml.Linq;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class ExportTimesheetsIndexViewModel
    {
        [Display(Name = "Select Project")]
        public int ProjectID { get; set; }
        public SelectList ProjectList { get; set; }
        [Display(Name = "Select Timesheet Period")]
        public int TimesheetPeriodID { get; set; }
    }
}