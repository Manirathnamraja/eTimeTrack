using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eTimeTrack.ViewModels
{
    public class ExpensesStdTypesViewModel
    {
        public List<ExpensesStdTypesDetails> expensesStdTypesDetails { get; set; }
    }

    public class ExpensesStdTypesDetails
    {
        public int StdTypeID { get; set; }
        public string StdType { get; set; }
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        public bool IsActive { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public SelectList Company { get; set; }
    }
}