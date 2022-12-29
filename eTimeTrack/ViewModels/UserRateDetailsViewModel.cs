using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace eTimeTrack.ViewModels
{
    public class UserRateDetailsViewModel
    {
        public int EmployeeID { get; set; }
        [DisplayName("Employee Number")]
        public string EmployeeNo { get; set; }
        [DisplayName("Name")]
        public string Names { get; set; }
        [DisplayName("Email")]
        public string EmailAddress { get; set; }

        [DisplayName("End Date")]
        [DisplayFormat(DataFormatString = "{0:d/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [DisplayName("Start Date")]
        [DisplayFormat(DataFormatString = "{0:d/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }

        
        [DisplayName("Project User Classifications")]
        public string ProjectUserClassificationID { get; set; }
        public IEnumerable<SelectListItem> ProjectUserClassifications { get; set; }
        public int ProjectID { get; set; }

        [Display(Name = "Rates Confirmed")]
        public bool IsRatesConfirmed { get; set; }
        //Fee Rates and Cost Rates
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

    }
}