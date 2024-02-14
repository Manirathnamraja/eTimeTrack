using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eTimeTrack.ViewModels
{
    public class RefreshreconcillationViewModel
    {
        public int Id { get; set; }
        public int? EmployeeId { get; set; } 
        public int? TimesheetPeriodId { get; set; } 
        public int? EmployeeTimesheetId { get; set; }
        public decimal? Hours { get; set; }
        public int? OriginalReconciliationUploadId { get; set; }
        public int? CurrentReconciliationUploadId { get; set; }
        public bool? HoursEqual { get; set; }
        public bool? Complete { get; set; }
        public int? Status { get; set; }
        public string ReconciliationComment { get; set; }
        public int? ReconciliationTypeId { get; set; }
        public string EmployeeComment { get; set; }
        public bool? Deleted { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public int? EmployeeTimesheetId_SHOULDBE { get; set; }   

    }
}