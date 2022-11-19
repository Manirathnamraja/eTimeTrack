using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace eTimeTrack.ViewModels
{
    public class ReconciliationEntryItemViewModel
    {
        public int Id { get; set; }
        [DisplayName("Employee Number")]
        public string EmployeeNo { get; set; }
        [DisplayName("Employee Names")]
        public string EmployeeNames { get; set; }
        [DisplayName("Company")]
        public string CompanyName { get; set; }
        [DisplayName("Hours Correct")]
        public bool HoursEqual { get; set; }
        [DisplayName("Home Office Hours")]
        public decimal? HoursExternal { get; set; }
        [DisplayName("eTimeTrack Hours")]
        public decimal? HoursETT { get; set; }
        [DisplayName("Timesheet Period")]
        public DateTime TimesheetPeriodEndDate { get; set; }
        [DisplayName("Reconciliation Type")]
        public int? ReconciliationTypeId { get; set; }
        public string Comments { get; set; }
        [DisplayName("Employee Comments")]
        public string EmployeeComments { get; set; }

        public int? EmployeeTimesheetId { get; set; }
        [DisplayName("Complete: Hide Entry")]
        public bool Complete { get; set; }
    }

    public class ReconciliationEntriesIndexViewModel
    {
        public int? ReconciliationTypeIdFilter { get; set; }
        public List<ReconciliationEntryItemViewModel> ReconciliationEntries { get; set; }
        public bool HideComplete { get; set; }
    }
}