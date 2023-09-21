﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eTimeTrack.Models
{
    public class ProjectExpensesUpload
    {
        [Key]
        public int ExpenseUploadID { get; set; }
        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        public int CompanyId { get; set; }

        public string InvoiceNumber { get; set; }

        [Display(Name = "Transaction ID")]
        public string TransactionID { get; set; }
        [Required]
        [Display(Name = "Expense Date")]
        public string ExpenseDate { get; set; }
        [Display(Name = "Costed In Week Ending")]
        public string CostedInWeekEnding { get; set; }
        [Required]
        public string Cost { get; set; }
        [Display(Name = "Home Office Type")]
        public string HomeOfficeType { get; set; }
        [Required]
        [Display(Name = "Employee Supplier Name")]
        public string EmployeeSupplierName { get; set;}
        public string UOM { get; set; }
        [Display(Name = "Expenditure Comment")]
        public string ExpenditureComment { get; set; }
        [Display(Name = "Project Comment")]
        public string ProjectComment { get; set; }
        public int? AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public int ProjectExpTypeID { get; set; }
        public int VariationID { get; set; }
        public int TaskID { get; set; }
        public bool IsFeeRecovery { get; set; }
        public bool IsCostRecovery { get; set; }
        public bool Completed { get; set; }
        public bool IsUpload { get; set; }

    }
}