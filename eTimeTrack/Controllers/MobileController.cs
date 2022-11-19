using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;

namespace eTimeTrack.Controllers
{
    [Authorize]
    public class MobileController : BaseController
    {
        public ActionResult NewTimesheet()
        {
            TimesheetPeriod existingPeriod = GetCurrentTimesheetPeriod();
            if (existingPeriod == null)
            {
                return RedirectToAction("Index", "EmployeeTimesheets");
            }

            EmployeeTimesheet existing = GetCurrentExistingTimesheet();
            return View(new MobileState {ExistingCurrentTimesheet = existing?.TimesheetID, ExistingCurrentPeriod = existingPeriod.TimesheetPeriodID});
        }

        public ActionResult CreateCurrentWeek()
        {
            return RedirectToAction("Create", "EmployeeTimesheets", new { id = UserHelpers.GetCurrentUserId(), adminMode = false });
        }
    }

    public class MobileState
    {
        public int? ExistingCurrentTimesheet { get; set; }
        public int? ExistingCurrentPeriod { get; set; }
    }

}