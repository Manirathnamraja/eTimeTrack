using System;
using System.Collections.Generic;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class ReconciliationSummaryDetailsViewModel
    {
        public TimesheetPeriod TimesheetPeriod { get; set; }
        public decimal? TotalEttHours { get; set; }
        public decimal? TotalOtherHours { get; set; }
        public int TotalEmployees { get; set; }
        public string ProjectName { get; set; }
        public string CompanyName { get; set; }
        public int? CompanyId { get; set; }
        public List<ReconciliationTypeHourSummary> ReconciliationHours { get; set; }       
    }

    public class ReconciliationTypeHourSummary
    {
        public ReconciliationType ReconciliationType { get; set; }
        public decimal? EttHours { get; set; }
        public decimal? OtherHours { get; set; }
        public int Employees { get; set; }
    }
}