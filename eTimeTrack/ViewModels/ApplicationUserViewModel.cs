using eTimeTrack.Helpers;
using eTimeTrack.Models;

namespace eTimeTrack.ViewModels
{
    public class ApplicationUserViewModel
    {
        public EmployeeViewModel Employee { get; set; }
        public RoleType RoleType { get; set; }
    }
}