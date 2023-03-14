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
    public class UserRatesUpload : ITrackableModel
    {
        [Key]
        public int UserRatesUploadId { get; set; }
       
        //[Index("IX_EmployeeProjectRestraint", 2, IsUnique = true)]
        public int ProjectId { get; set; }

        public string UserIDColumn { get; set; }
        public string ProjectUserClassificationIDColumn { get; set; }

        public string StartDateColumn { get; set; }

        
        public string EndDateColumn { get; set; }
        public string FilePath { get; set; }
               

        [Display(Name = "Rates Confirmed")]
        public string IsRatesConfirmedColumn { get; set; }

        [JsonIgnore]
        public virtual Project Project { get; set; }

        [JsonIgnore]
        public IEnumerable<SelectListItem> ProjectUserClassifications { get; set; }

        #region Rates

        public string NTFeeRateColumn { get; set; }
        public string NTCostRateColumn { get; set; }
        public string OT1FeeRateColumn { get; set; }
        public string OT1CostRateColumn { get; set; }
        public string OT2FeeRateColumn { get; set; }
        public string OT2CostRateColumn { get; set; }
        public string OT3FeeRateColumn { get; set; }
        public string OT3CostRateColumn { get; set; }
        public string OT4FeeRateColumn { get; set; }
        public string OT4CostRateColumn { get; set; }
        public string OT5FeeRateColumn { get; set; }
        public string OT5CostRateColumn { get; set; }
        public string OT6FeeRateColumn { get; set; }
        public string OT6CostRateColumn { get; set; }
        public string OT7FeeRateColumn { get; set; }
        public string OT7CostRateColumn { get; set; }
        #endregion

        public int? AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }



        public string GetId()
        {
            return UserRatesUploadId.ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}