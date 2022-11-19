using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class EmployeeTimesheet : ITrackableModel, IUserModified, IMergeable
    {
        [Key]
        public int TimesheetID { get; set; }
        public int EmployeeID { get; set; }
        [Display(Name = "Timesheet Period")]
        public int TimesheetPeriodID { get; set; }
        [Display(Name = "Approved By")]
        public int? ApprovedByID { get; set; }
        [Display(Name = "Date Approved")]
        public DateTime? DateApproved { get; set; }
        public bool UseDayTimeEntry { get; set; }
        public DateTime? Day1StartTime { get; set; }
        public DateTime? Day1EndTime { get; set; }
        public DateTime? Day2StartTime { get; set; }
        public DateTime? Day2EndTime { get; set; }
        public DateTime? Day3StartTime { get; set; }
        public DateTime? Day3EndTime { get; set; }
        public DateTime? Day4StartTime { get; set; }
        public DateTime? Day4EndTime { get; set; }
        public DateTime? Day5StartTime { get; set; }
        public DateTime? Day5EndTime { get; set; }
        public DateTime? Day6StartTime { get; set; }
        public DateTime? Day6EndTime { get; set; }
        public DateTime? Day7StartTime { get; set; }
        public DateTime? Day7EndTime { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        [JsonIgnore]
        public virtual Employee Employee { get; set; }
        [JsonIgnore]
        public virtual TimesheetPeriod TimesheetPeriod { get; set; }
        [JsonIgnore]
        public virtual Employee ApprovedBy { get; set; }
        [JsonIgnore]
        public virtual ICollection<ReconciliationEntry> ReconciliationEntries { get; set; }

        [JsonIgnore]
        [ForeignKey("LastModifiedBy")]
        public virtual Employee LastModifiedByUser { get; set; }
        [JsonIgnore]
        public virtual ICollection<EmployeeTimesheetItem> TimesheetItems { get; set; }

        [JsonIgnore]
        [NotMapped]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public Dictionary<string, bool> MergedFields { get; set; }

        public EmployeeTimesheet()
        {
            TimesheetItems = new List<EmployeeTimesheetItem>();
        }

        public string GetId()
        {
            return TimesheetID.ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void SetLastModifiedUserAndTime(int userId)
        {
            LastModifiedBy = userId;
            LastModifiedDate = DateTime.UtcNow;
        }
    }
}