using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Models;
using System;
using eTimeTrack.Extensions;
using eTimeTrack.Helpers;
using eTimeTrack.ViewModels;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.RoleSuperUser)]
    public class TimesheetPeriodsController : BaseController
    {
        // GET: TimesheetPeriods
        public ActionResult Index()
        {
            List<TimesheetPeriod> timesheetPeriods = Db.TimesheetPeriods.OrderByDescending(x => x.StartDate).ToList();

            ViewBag.InfoMessage = TempData["message"];
            return View(timesheetPeriods);
        }

        [HttpPost]
        public void PeriodClosure(int timesheetId, bool state)
        {
            TimesheetPeriod period = Db.TimesheetPeriods.SingleOrDefault(x => x.TimesheetPeriodID == timesheetId);
            if (period != null)
            {
                period.IsClosed = state;
                Db.SaveChanges();
            }
        }

        public ActionResult CreateTimesheetPeriods()
        {
            ViewBag.StartDate = GetLastExistingPeriod().EndDate.AddDays(1).ToDateStringGeneral();
            return View();
        }

        [HttpPost]
        public ActionResult CreateTimesheetPeriods(TimesheetCreateViewModel model)
        {
            const string periodsAheadText = "six weeks";
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            int countInserted = 0;
            if (model.Weeks > 0)
            {
                TimesheetPeriod lastExisting = GetLastExistingPeriod();
                DateTime maxDate = DateTime.Now.AddDays(44); // allow for maximum of 6 weeks (2 day overshoot for timezone differences)


                DateTime start = lastExisting.EndDate.AddDays(1);
                int startWeekNo = lastExisting.WeekNo + 1;

                for (int i = 0; i < model.Weeks; i++)
                {
                    TimesheetPeriod timesheetPeriod = new TimesheetPeriod
                    {
                        StartDate = start,
                        EndDate = start.AddDays(6),
                        IsClosed = model.IsClosed,
                        WeekNo = startWeekNo++
                    };

                    if (timesheetPeriod.EndDate > maxDate) break;

                    Db.TimesheetPeriods.Add(timesheetPeriod);
                    countInserted++;
                    start = start.AddDays(7);
                }
                Db.SaveChanges();
            }

            InfoMessage message = null;

            if (countInserted == 0)
            {
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = $"Failed to create any new timesheet periods (maximum of {periodsAheadText} ahead)." };
            }
            else if (countInserted < model.Weeks)
            {
                message = new InfoMessage { MessageType = InfoMessageType.Warning, MessageContent = $"Could only create {countInserted} out of {model.Weeks} requested timesheet periods (maximum of {periodsAheadText} ahead)." };
            }
            else if (countInserted == model.Weeks)
            {
                message = new InfoMessage
                {
                    MessageType = InfoMessageType.Success,
                    MessageContent = $"Successfully created {model.Weeks} timesheet period(s)."
                };
            }
            TempData["message"] = message;
            return RedirectToAction("Index");
        }

        private TimesheetPeriod GetLastExistingPeriod()
        {
            return Db.TimesheetPeriods.OrderByDescending(x => x.EndDate).First();
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
