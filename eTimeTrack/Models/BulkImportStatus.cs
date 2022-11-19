using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.Models
{
    public class BulkImportStatus
    {
        public string User { get; set; }
        public int Row { get; set; }
        public TimesheetPeriod Period { get; set; }
        public ImportStatus Status { get; set; }
        public string Task { get; set; }
        public string Variation { get; set; }
    }

    public enum ImportStatus
    {
        [Display(Name="Success: Added to new timesheet")]
        SuccessNewTimesheet,
        [Display(Name = "Success: Added to existing timesheet")]
        SuccessExistingTimesheet,
        [Display(Name = "Failure: Duplicate of existing item entry")]
        FailureDuplicateOfExisting,
        [Display(Name = "Failure: Non-existent timesheet period")]
        FailureNonexistentTimesheetPeriod,
        [Display(Name = "Failure: Non-existent project task")]
        FailureNonexistentTask,
        [Display(Name = "Failure: Non-existent project variation")]
        FailureNonexistentVariation,
        [Display(Name = "Failure: Non-existent user")]
        FailureNonexistentUser,
        [Display(Name = "Failure: Unknown error")]
        FailureUnknown,
        [Display(Name = "Failure: Cannot import negative value")]
        NegativeValue
    }
}