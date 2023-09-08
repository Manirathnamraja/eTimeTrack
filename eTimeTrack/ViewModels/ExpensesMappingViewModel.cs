using System.Collections.Generic;
using System.Web.Mvc;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class ExpensesMappingViewModel
    {
        public List<ExpensesMappingDetails> ExpensesMappingDetails { get; set; }
        public IEnumerable<SelectListItem> StdExpenseTypes { get; set; }
    }

    public class ExpensesMappingDetails
    {
        public string ExpenseType { get; set; }
        public int ProjectTypeID { get; set; }
        public int ProjectID { get; set; }
        public int CompanyID { get; set; }
        public int StdExpTypeID { get; set; }
    }
}