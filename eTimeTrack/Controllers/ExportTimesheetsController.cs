using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Models;
using eTimeTrack.Helpers;
using eTimeTrack.ViewModels;
using eTimeTrack.Extensions;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.RoleSuperUser)]
    public class ExportTimesheetsController : BaseController
    {       
        public ActionResult Index(int? id = null, bool adminMode = false)
        {
            int userId = id ?? UserHelpers.GetCurrentUserId();
            List<EmployeeTimesheet> existingTimesheetsForUser = Db.EmployeeTimesheets.Where(x => x.EmployeeID == userId).ToList();

            List<TimesheetPeriod> periods = GetDbTimesheetPeriods();

            SelectList periodSelectList = CreateTimesheetPeriodDropdown(periods, existingTimesheetsForUser, adminMode);
            ViewBag.TimesheetPeriods = periodSelectList;

            SelectList existingPeriods = GetExistingPeriodsForUser(periods, existingTimesheetsForUser);
            ViewBag.TimesheetPeriodDuplicates = existingPeriods;
            TimesheetPeriod currentPeriod = GetCurrentTimesheetPeriod();
            bool currentExists = currentPeriod != null && existingPeriods.Any(x => x.Value == currentPeriod.ToString());
          
            ExportTimesheetsIndexViewModel viewModel = new ExportTimesheetsIndexViewModel { ProjectList = GenerateDropdownUserProjects(), TimesheetPeriodID = currentExists ? 0 : currentPeriod?.TimesheetPeriodID ?? 0 };


            return View(viewModel);
        }
        private SelectList CreateTimesheetPeriodDropdown(List<TimesheetPeriod> periods, List<EmployeeTimesheet> existingTimesheetsForUser, bool adminMode)
        {
            return new SelectList(from period in periods.Where(x => adminMode || !x.IsClosed)
                                  where !(from ex in existingTimesheetsForUser
                                          select ex.TimesheetPeriodID)
                                      .Contains(period.TimesheetPeriodID)
                                  select new
                                  {
                                      Value = period.TimesheetPeriodID.ToString(),
                                      Text = period.GetStartEndDates(),
                                  },
                "Value",
                "Text");
        }

        private static SelectList GetExistingPeriodsForUser(List<TimesheetPeriod> periods, List<EmployeeTimesheet> existingTimesheetsForUser)
        {
            List<SelectListItem> items = (from period in periods
                                          where (from ex in existingTimesheetsForUser
                                                 select ex.TimesheetPeriodID)
                                          .Contains(period.TimesheetPeriodID)
                                          select new SelectListItem
                                          {
                                              Value = period.TimesheetPeriodID.ToString(),
                                              Text = period.GetStartEndDates(),
                                          }).ToList();

            SelectList existingPeriods = new SelectList(items, "Value", "Text", items.FirstOrDefault()?.Value);
            return existingPeriods;
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}