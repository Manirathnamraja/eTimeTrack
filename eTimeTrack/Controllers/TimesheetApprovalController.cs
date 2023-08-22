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

            var employeeid = UserHelpers.GetCurrentUserId();

            var empresultslist = from e in Db.EmployeeTimesheetItems
                                 join t in Db.ProjectTasks on e.TaskID equals t.TaskID
                                 join v in Db.ProjectVariations on e.VariationID equals v.VariationID
                                 join et in Db.EmployeeTimesheets on e.TimesheetID equals et.TimesheetID
                                 join emp in Db.Users on et.EmployeeID equals emp.Id
                                 join tp in Db.TimesheetPeriods on et.TimesheetPeriodID equals tp.TimesheetPeriodID
                                 where t.ProjectID == selectedProject && e.IsTimeSheetApproval != true && emp.Id == employeeid
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