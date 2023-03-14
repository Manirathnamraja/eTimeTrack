using eTimeTrack.Extensions;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace eTimeTrack.ViewModels
{
    public class UserRatesUploadCreateViewModel
    {
        //[Required]
        public int UserRatesUploadId { get; set; }
        public int EmployeeId { get; set; }
        public int ProjectID { get; set; }
        [Required]
        [Display(Name = "Project User Classifications Column")]
        public string ProjectUserClassification { get; set; }
        [Required]
        [Display(Name = "User ID Column")]
        public string UserID { get; set; }

        [Required]
        [Display(Name = "Start Date Column")]
        public string StartDate { get; set; }
      
        [Required]
        [Display(Name = "End Date Column")]
        public string EndDate { get; set; }

        [Required(ErrorMessage = "Please select file")]
        [FileExtension(Allow = ".xls,.xlsx", ErrorMessage = "Only excel file")]
        public HttpPostedFileBase file { get; set; }

        public SelectList ProjectList { get; set; }


        //public string FormattedFrom => StartDate.ToString("dd/MM/yyyy");
        //public string FormattedTo => EndDate?.ToString("dd/MM/yyyy") ?? "Present";
        [Required]
        [Display(Name = "Rates Confirmed Column")]
        public string IsRatesConfirmed { get; set; }

        //public bool IsDeleted { get; set; }

        #region Rates
        [Display(Name = "NT Fee Rate Column")]
        public string NTFeeRate { get; set; }

        [Display(Name = "NT Cost Rate Column")]
        public string NTCostRate { get; set; }

        [Display(Name = "OT1 Fee Rate Column")]
        public string OT1FeeRate { get; set; }

        [Display(Name = "OT1 Cost Rate Column")]
        public string OT1CostRate { get; set; }

        [Display(Name = "OT2 Fee Rate Column")]
        public string OT2FeeRate { get; set; }

        [Display(Name = "OT2 Cost Rate Column")]
        public string OT2CostRate { get; set; }

        [Display(Name = "OT3 Fee Rate Column")]
        public string OT3FeeRate { get; set; }

        [Display(Name = "OT3 Cost Rate Column")]
        public string OT3CostRate { get; set; }

        [Display(Name = "OT4 Fee Rate Column")]
        public string OT4FeeRate { get; set; }

        [Display(Name = "OT4 Cost Rate Column")]
        public string OT4CostRate { get; set; }

        [Display(Name = "OT5 Fee Rate Column")]
        public string OT5FeeRate { get; set; }

        [Display(Name = "OT5 Cost Rate Column")]
        public string OT5CostRate { get; set; }

        [Display(Name = "OT6 Fee Rate Column")]
        public string OT6FeeRate { get; set; }

        [Display(Name = "OT6 Cost Rate Column")]
        public string OT6CostRate { get; set; }

        [Display(Name = "OT7 Fee Rate Column")]
        public string OT7FeeRate { get; set; }

        [Display(Name = "OT7 Cost Rate Column")]
        public string OT7CostRate { get; set; }
        #endregion
    }

    //public class ReconciliationTemplateCreateViewModel : ReconciliationTemplateBaseViewModel
    //{
    //    [DisplayName("Company")]
    //    [Required]
    //    public int CompanyId { get; set; }
    //}

    //public class ReconciliationTemplateUpdateViewModel : ReconciliationTemplateBaseViewModel
    //{
    //    [Required]
    //    public int ReconciliationTemplateId { get; set; }
    //}
}