using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using eTimeTrack.Helpers;

namespace eTimeTrack.Models
{
    public class TimesheetPeriod : ITrackableModel, IUserModified, IMergeable
    {
        [Key]
        [Display(Name = "Timesheet Period")]
        public int TimesheetPeriodID { get; set; }
        [Display(Name = "Week Number")]
        public int WeekNo { get; set; }
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        [Display(Name = "Period Closed")]
        public bool IsClosed { get; set; }
        [Display(Name = "Standard Days")]
        public int StandardDays { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        [JsonIgnore]
        [ForeignKey("LastModifiedBy")]
        public virtual Employee LastModifiedByUser { get; set; }
        [JsonIgnore]
        public virtual ICollection<EmployeeTimesheet> EmployeeTimesheet { get; set; }

        [JsonIgnore]
        public virtual ICollection<ReconciliationEntry> ReconciliationEntries { get; set; }

        [JsonIgnore]
        [NotMapped]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public Dictionary<string, bool> MergedFields { get; set; }

        public TimesheetPeriod()
        {
            IsClosed = true;
            StandardDays = 5;
            LastModifiedDate = DateTime.UtcNow;
            if (UserHelpers.GetCurrentUserId() != UserHelpers.Invalid)
            {
                LastModifiedBy = UserHelpers.GetCurrentUserId();
            }
        }

        public string GetId()
        {
            return TimesheetPeriodID.ToString();
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