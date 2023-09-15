using eTimeTrack.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace eTimeTrack.ViewModels
{
    public class ExpensesAllocationsViewModel
    {
        public List<ProjectExpensesUploadDetails> ProjectExpensesUploadDetails {  get; set; }
        public bool HideCompleted { get; set; }
    }

    public class ProjectExpensesUploadDetails
    {
        public int ExpenseUploadID { get; set; }
        public int ProjectId { get; set; }

        public int CompanyId { get; set; }

        public string InvoiceNumber { get; set; }

        public string TransactionID { get; set; }
        
        public string ExpenseDate { get; set; }
       
        public string CostedInWeekEnding { get; set; }
        
        public string Cost { get; set; }
        public string HomeOfficeType { get; set; }
        
        public string EmployeeSupplierName { get; set; }
        public string UOM { get; set; }
        
        public string ExpenditureComment { get; set; }
       
        public string ProjectComment { get; set; }
        public int? AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public int ProjectExpTypeID { get; set; }
        public int VariationID { get; set; }
        public int TaskID { get; set; }
        public bool IsFeeRecovery { get; set; }
        public bool IsCostRecovery { get; set; }
        public bool Completed { get; set; }
        public string CompanyName { get; set; }
        public string ExpensesTypes { get; set; }
    }
}