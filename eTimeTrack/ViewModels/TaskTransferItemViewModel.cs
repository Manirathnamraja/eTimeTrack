using System.Collections.Generic;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class TaskTransferItemViewModel
    {
        public List<TransferItemViewModel> EmployeeTimesheetItems { get; set; }
        public ProjectVariationItem ProjectVariationItemFrom { get; set; }
        public ProjectVariationItem ProjectVariationItemTo { get; set; }
    }

    public class TransferItemViewModel
    {
        public EmployeeTimesheetItem EmployeeTimesheetItem { get; set; }
        public bool Transfer { get; set; }
    }
}