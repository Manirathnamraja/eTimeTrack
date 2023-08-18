using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class TimesheetApprovalController : BaseController
    {
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult Index()
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            if (selectedProject == 0) { return InvokeHttp404(HttpContext); }

            var empresultslist = from e in Db.EmployeeTimesheetItems
                                 join t in Db.ProjectTasks on e.TaskID equals t.TaskID
                                 join v in Db.ProjectVariations on e.VariationID equals v.VariationID
                                 join et in Db.EmployeeTimesheets on e.TimesheetID equals et.TimesheetID
                                 join emp in Db.Users on et.EmployeeID equals emp.Id
                                 join tp in Db.TimesheetPeriods on et.TimesheetPeriodID equals tp.TimesheetPeriodID
                                 where t.ProjectID == selectedProject && e.IsTimeSheetApproval != true 
                                 select new TimesheetApprovaldetails
                                 {
                                     Comments = e.Comments,
                                     TaskNo = t.TaskNo,
                                     Name = t.Name,
                                     VariationNo = v.VariationNo,
                                     Description = v.Description,
                                     Names = emp.Names,
                                     Day1Hrs = e.Day1Hrs,
                                     Day1Comments = e.Day1Comments,
                                     Day2Comments = e.Day2Comments,
                                     Day2Hrs = e.Day2Hrs,
                                     Day3Comments = e.Day3Comments,
                                     Day4Comments = e.Day4Comments,
                                     Day5Comments = e.Day5Comments,
                                     Day6Comments = e.Day6Comments,
                                     Day7Comments = e.Day7Comments,
                                     Day3Hrs = e.Day3Hrs,
                                     Day4Hrs = e.Day4Hrs,
                                     Day5Hrs = e.Day5Hrs,
                                     Day6Hrs = e.Day6Hrs,
                                     Day7Hrs = e.Day7Hrs,
                                     EndDate = tp.EndDate,
                                     TimesheetItemID = e.TimesheetItemID
                                 };

            var res = empresultslist.Distinct().OrderByDescending(x => x.EndDate).ToList();
            foreach (var item in res)
            {
                DateTime dateValue = new DateTime(item.EndDate.Year, item.EndDate.Month, item.EndDate.Day);
                item.days = dateValue.ToString("dddd");
                switch (item.days.ToLower())
                {
                    case "saturday":
                        item.Hours = item.Day1Hrs;
                        item.DailyComments = item.Day1Comments;
                        break;
                    case "sunday":
                        item.Hours = item.Day2Hrs;
                        item.DailyComments = item.Day2Comments;
                        break;
                    case "monday":
                        item.Hours = item.Day3Hrs;
                        item.DailyComments = item.Day3Comments;
                        break;
                    case "tuesday":
                        item.Hours = item.Day4Hrs;
                        item.DailyComments = item.Day4Comments;
                        break;
                    case "wednesday":
                        item.Hours = item.Day5Hrs;
                        item.DailyComments = item.Day5Comments;
                        break;
                    case "thursday":
                        item.Hours = item.Day6Hrs;
                        item.DailyComments = item.Day6Comments;
                        break;
                    case "friday":
                        item.Hours = item.Day7Hrs;
                        item.DailyComments = item.Day7Comments;
                        break;
                }
            }
            return View(new TimesheetApprovalViewModel
            {
                timesheetApprovaldetails = res
            });
        }

        [HttpPost]
        public ActionResult ApproveTimesheet(TimesheetApprovalViewModel timesheet)
        {
            foreach (var item in timesheet.timesheetApprovaldetails)
            {
                if(item.IsApproval == true)
                {
                    EmployeeTimesheetItem timesheetItem = Db.EmployeeTimesheetItems.Find(item.TimesheetItemID);
                    if (timesheetItem != null)
                    {
                        timesheetItem.IsTimeSheetApproval = item.IsApproval;
                        timesheetItem.Reviewercomments = item.Reviewercomments;
                        timesheetItem.LastApprovedBy = UserHelpers.GetCurrentUserId();
                        timesheetItem.LastApprovedDate = DateTime.UtcNow;
                        Db.SaveChanges();
                    }
                }
            }
            return RedirectToAction("Index");
        }
        
    }
}