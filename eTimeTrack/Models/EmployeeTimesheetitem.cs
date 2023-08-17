using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using eTimeTrack.Enums;

namespace eTimeTrack.Models
{
    public class EmployeeTimesheetItem : ITrackableModel, IUserModified, IMergeable
    {
        [Key]
        public int TimesheetItemID { get; set; }

        public int TimesheetID { get; set; }
        public int VariationID { get; set; }
        public int TaskID { get; set; }
        public int OTCode { get; set; }
        public int PayTypeID { get; set; }
        public int? ItemNo { get; set; }
        [Range(0.0, 24.0)]
        [Display(Name = "Saturday Hours")]
        public decimal? Day1Hrs { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Day1Comments { get; set; }
        [Range(0.0, 24.0)]
        [Display(Name = "Sunday Hours")]
        public decimal? Day2Hrs { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Day2Comments { get; set; }
        [Range(0.0, 24.0)]
        [Display(Name = "Monday Hours")]
        public decimal? Day3Hrs { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Day3Comments { get; set; }
        [Range(0.0, 24.0)]
        [Display(Name = "Tuesday Hours")]
        public decimal? Day4Hrs { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Day4Comments { get; set; }
        [Range(0.0, 24.0)]
        [Display(Name = "Wednesday Hours")]
        public decimal? Day5Hrs { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Day5Comments { get; set; }
        [Range(0.0, 24.0)]
        [Display(Name = "Thursday Hours")]
        public decimal? Day6Hrs { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Day6Comments { get; set; }
        [Range(0.0, 24.0)]
        [Display(Name = "Friday Hours")]
        public decimal? Day7Hrs { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Day7Comments { get; set; }
        public int? InvoiceID { get; set; }
        [StringLength(255, ErrorMessage = "Maximum length is 255")]
        public string Comments { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public TimeCode TimeCode { get; set; }

        public bool? IsTimeSheetApproval { get; set; }
        public string Reviewercomments { get; set; }

        public int? LastApprovedBy { get; set; }

        public DateTime? LastApprovedDate { get; set; }

        [JsonIgnore]
        [NotMapped]
        public string TimeCodeText { get; set; }

        [JsonIgnore]
        [NotMapped]
        public bool Valid { get; set; }

        [JsonIgnore]
        [NotMapped]
        public bool ReadOnly { get; set; }

        [JsonIgnore]
        public virtual EmployeeTimesheet Timesheet { get; set; }
        [JsonIgnore]
        public virtual ProjectVariation Variation { get; set; }
        [JsonIgnore]
        public virtual ProjectTask ProjectTask { get; set; }

        [JsonIgnore]
        [ForeignKey("PayTypeID")]
        public virtual LU_PayType PayType { get; set; }

        [JsonIgnore]
        [ForeignKey("LastModifiedBy")]
        public virtual Employee LastModifiedByUser { get; set; }

        [JsonIgnore]
        [NotMapped]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public Dictionary<string, bool> MergedFields { get; set; }

        public EmployeeTimesheetItem()
        {
            PayTypeID = 1;
        }

        public string GetId()
        {
            return TimesheetItemID.ToString();
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

        public static string ConvertDayNumberToName(int dayNumber0Index)
        {
            return Enum.GetName(typeof(DayOfWeekeTimeTrack), dayNumber0Index);
        }

        public decimal TotalHours()
        {
            return Day1Hrs.GetValueOrDefault() + Day2Hrs.GetValueOrDefault() + Day3Hrs.GetValueOrDefault() + Day4Hrs.GetValueOrDefault() + Day5Hrs.GetValueOrDefault() + Day6Hrs.GetValueOrDefault() + Day7Hrs.GetValueOrDefault();
        }
    }

    public enum DayOfWeekeTimeTrack
    {

        Saturday = 0,
        Sunday = 1,
        Monday = 2,
        Tuesday = 3,
        Wednesday = 4,
        Thursday = 5,
        Friday = 6,

    }
}