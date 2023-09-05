using System.Collections.Generic;
using System.Web.Mvc;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class ExpensesMappingViewModel
    {
        public List<ProjectExpenseType> projectExpenseTypes { get; set; }
        public IEnumerable<SelectListItem> StdExpenseTypes { get; set; }
    }
}