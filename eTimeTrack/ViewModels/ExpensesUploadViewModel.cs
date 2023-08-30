using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using eTimeTrack.Extensions;
using System.Web.Mvc;

namespace eTimeTrack.ViewModels
{
    public class ExpensesUploadViewModel
    {
        [Key]
        public int ExpenseUploadID { get; set; }

        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        [Display(Name = "Transaction ID")]
        public string TransactionID { get; set; }

        [Required]
        [Display(Name = "Expense Item Date")]
        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        [Display(Name = "Costed In Week Ending")]
        public string CostedInWeekEnding { get; set; }

        [Required]
        public string Cost { get; set; }

        [Display(Name = "Home Office Type")]
        public string HomeOfficeType { get; set; }

        [Required]
        [Display(Name = "Employee Supplier Name")]
        public string EmployeeSupplierName { get; set; }

        public string UOM { get; set; }

        [Display(Name = "Expenditure Comment")]
        public string ExpenditureComment { get; set; }

        [Display(Name = "Project Comment")]
        public string ProjectComment { get; set; }

        public int? AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }

        [Required(ErrorMessage = "Please select file")]
        [FileExtension(Allow = ".xls,.xlsx", ErrorMessage = "Only excel file")]
        public HttpPostedFileBase file { get; set; }

        public SelectList ProjectList { get; set; }
    }
}