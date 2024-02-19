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
        public string TimeAdministratorContact { get; set; }
        public string WBSCodes { get; set; }
        public string CapWeeklyHours { get; set; }
        public string MatchHours  { get; set; }
        public string OvertimeApproval  { get; set; }
        public string NTTimeCodeNote { get; set; }
        public string OT1TimeCodeNote { get; set; }
        public string OT2TimeCodeNote { get; set; }
        public string OT3TimeCodeNote { get; set; }
        public string OT4TimeCodeNote { get; set; }
        public string OT5TimeCodeNote { get; set; }
        public string OT6TimeCodeNote { get; set; }
        public string OT7TimeCodeNote { get; set; }
        public string TimesheetComments { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}