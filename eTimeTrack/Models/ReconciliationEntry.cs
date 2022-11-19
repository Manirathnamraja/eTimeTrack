using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace eTimeTrack.Models
{
    public class ReconciliationEntry : IUserModified, ITrackableModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; } //probably import the employee number so have to convert to id
        public int TimesheetPeriodId { get; set; } //probably import the week ending so have to convert to id
        public int? EmployeeTimesheetId { get; set; }
        public decimal? Hours { get; set; }
        public int OriginalReconciliationUploadId { get; set; }
        public int CurrentReconciliationUploadId { get; set; }
        public bool HoursEqual { get; set; }
        public bool Complete { get; set; }
        public ReconciliationDiscrepencyStatus? Status { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string ReconciliationComment { get; set; }
        public int? ReconciliationTypeId { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string EmployeeComment { get; set; }
        public bool Deleted { get; set; }

        [JsonIgnore]
        public virtual Employee Employee { get; set; }
        [JsonIgnore]
        public virtual TimesheetPeriod TimesheetPeriod { get; set; }
        [JsonIgnore]
        public virtual ReconciliationType ReconciliationType { get; set; }
        [JsonIgnore]
        [ForeignKey("EmployeeTimesheetId")]
        public virtual EmployeeTimesheet EmployeeTimesheet { get; set; }

        [JsonIgnore]
        [ForeignKey("OriginalReconciliationUploadId")]
        public virtual ReconciliationUpload OriginalReconciliationUpload { get; set; }
        [JsonIgnore]
        [ForeignKey("CurrentReconciliationUploadId")]
        public virtual ReconciliationUpload CurrentReconciliationUpload { get; set; }

        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }


        public string GetId()
        {
            return Id.ToString();
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

    public enum ReconciliationDiscrepencyStatus
    {
        ToBeActioned = 1,
        Actioned = 2,
        PostActionChangeToBeActioned = 3
    }
}