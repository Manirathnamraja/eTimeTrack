using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace eTimeTrack.Models
{
    public class ProjectExpensesStdDetails
    {
        [Key]
        public int StdTypeID { get; set; }
        public string StdType { get; set; }
        public int CompanyID { get; set; }
        public bool IsActive { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}