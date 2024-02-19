using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace eTimeTrack.Models
{
    public class ProjectGuidanceNotes
    {
        [Key]
        public int GuidanceNoteId { get; set; }
        public int ProjectId { get; set; }
        [Display(Name = "Time Administrator contact")]
        public string TimeAdministratorContact { get; set; }
        [Display(Name = "Online notes used with WBS codes")]
        public string WBSCodes { get; set; }
        [Display(Name = "Cap weekly hours at")]
        public string CapWeeklyHours { get; set; }
        [Display(Name = "Match hours to home office timesheet?")]
        public string MatchHours  { get; set; }
        [Display(Name = "Overtime Approval requirements")]
        public string OvertimeApproval  { get; set; }
        [Display(Name = "NT Time Code Note")]
        public string NTTimeCodeNote { get; set; }
        [Display(Name = "OT1 Time Code Note")]
        public string OT1TimeCodeNote { get; set; }
        [Display(Name = "OT2 Time Code Note")]
        public string OT2TimeCodeNote { get; set; }
        [Display(Name = "OT3 Time Code Note")]
        public string OT3TimeCodeNote { get; set; }
        [Display(Name = "OT4 Time Code Note")]
        public string OT4TimeCodeNote { get; set; }
        [Display(Name = "OT5 Time Code Note")]
        public string OT5TimeCodeNote { get; set; }
        [Display(Name = "OT6 Time Code Note")]
        public string OT6TimeCodeNote { get; set; }
        [Display(Name = "OT7 Time Code Note")]
        public string OT7TimeCodeNote { get; set; }
        [Display(Name = "Timesheet Comments")]
        public string TimesheetComments { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}