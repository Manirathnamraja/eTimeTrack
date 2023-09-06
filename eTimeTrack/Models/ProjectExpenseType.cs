using System;
using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.Models
{
    public class ProjectExpenseType
    {
        [Key]
        public int ExpenseTypeID { get; set; }
        public string ExpenseType { get; set; }
        public int VariationID { get; set; }
        public int TaskID { get; set; }
        public int ProjectID { get; set; }
        public bool IsClosed { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public bool IsFeeRecovery { get; set; }
        public bool IsCostRecovery { get; set; }
    }
}