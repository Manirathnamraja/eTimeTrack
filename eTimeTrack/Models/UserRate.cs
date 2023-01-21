using System.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.WebPages;

namespace eTimeTrack.Models
{
    public class UserRate : ITrackableModel
    {
        [Key]
        public int UserRateId { get; set; }
        [Index("IX_EmployeeProjectRestraint", 1, IsUnique = true)]
        public int EmployeeId { get; set; }
        [Index("IX_EmployeeProjectRestraint", 2, IsUnique = true)]
        public int ProjectId { get; set; }
        public int? ProjectUserClassificationID { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d/MMM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d/MMM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? EndDate { get; set; }


        //public string FormattedFrom => StartDate.ToString("dd/MM/yyyy");
        //public string FormattedTo => EndDate?.ToString("dd/MM/yyyy") ?? "Present";

        [Display(Name = "Rates Confirmed")]
        public bool IsRatesConfirmed { get; set; }

        public bool IsDeleted { get; set; }

        //[JsonIgnore]
        //public string ProjectUserClassificationSelectedValue { get; set; }

        [JsonIgnore]
        public virtual Employee Employee { get; set; }

        [JsonIgnore]
        public virtual Project Project { get; set; }

        [JsonIgnore]
        public IEnumerable<SelectListItem> ProjectUserClassifications { get; set; }

        #region Rates

        public string NTFeeRate { get; set; }
        public string NTCostRate { get; set; }
        public string OT1FeeRate { get; set; }
        public string OT1CostRate { get; set; }
        public string OT2FeeRate { get; set; }
        public string OT2CostRate { get; set; }
        public string OT3FeeRate { get; set; }
        public string OT3CostRate { get; set; }
        public string OT4FeeRate { get; set; }
        public string OT4CostRate { get; set; }
        public string OT5FeeRate { get; set; }
        public string OT5CostRate { get; set; }
        public string OT6FeeRate { get; set; }
        public string OT6CostRate { get; set; }
        public string OT7FeeRate { get; set; }
        public string OT7CostRate { get; set; }
        #endregion

        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }



        public string GetId()
        {
            return UserRateId.ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}