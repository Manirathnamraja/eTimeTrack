using eTimeTrack.Enums;
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
    public class DeleteZeroTimesheetItemsController : BaseController
    {
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult Index()
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            if (selectedProject == 0) { return InvokeHttp404(HttpContext); }

            int? groupid = 0; int? taskid = 0; int? partid = 0;

            var employeeid = UserHelpers.GetCurrentUserId();
            var empPt = Db.ProjectTasks.Where(x => x.PM == employeeid).FirstOrDefault();
            if (empPt != null)
            {
                taskid = empPt.PM;
            }
            var empPg = Db.ProjectGroups.Where(x => x.PM == employeeid).FirstOrDefault();
            if (empPg != null)
            {
                groupid = empPg.PM;
            }
            var empPp = Db.ProjectParts.Where(x => x.PM == employeeid).FirstOrDefault();
            if (empPp != null)
            {
                partid = empPp.PM;
            }

            var empresultslist = from e in Db.EmployeeTimesheetItems
                                 join v in Db.ProjectVariations on e.VariationID equals v.VariationID
                                 join et in Db.EmployeeTimesheets on e.TimesheetID equals et.TimesheetID
                                 join emp in Db.Users on et.EmployeeID equals emp.Id
                                 join t in Db.ProjectTasks on e.TaskID equals t.TaskID
                                 join g in Db.ProjectGroups on t.GroupID equals g.GroupID
                                 join p in Db.ProjectParts on g.PartID equals p.PartID
                                 join tp in Db.TimesheetPeriods on et.TimesheetPeriodID equals tp.TimesheetPeriodID
                                 where t.ProjectID == selectedProject
                                 && e.IsTimeSheetApproval != true
                                 && (t.PM == taskid || g.PM == groupid || p.PM == partid)
                                 && (e.Day1Hrs <= 0 && e.Day2Hrs <= 0 && e.Day3Hrs <= 0 && e.Day4Hrs <= 0 && e.Day5Hrs <= 0 && e.Day6Hrs <= 0 && e.Day7Hrs <= 0)
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
                                     TimesheetItemID = e.TimesheetItemID,
                                     TimeCode = e.TimeCode,
                                     Reviewercomments = e.Reviewercomments
                                 };

            var res = empresultslist.Distinct().OrderByDescending(x => x.EndDate).ToList();
            if (res.Count > 0)
            {
                foreach (var item in res)
                {
                    item.Timecodes = timecode(item.TimeCode);
                    item.TimecodesName = timecodename(item.Timecodes);
                }
            }
            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(new TimesheetApprovalViewModel
            {
                timesheetApprovaldetails = res
            });
        }

        public ActionResult DeleteConfirmed(List<TimesheetApprovaldetails> timesheetApprovaldetails)
        {
            foreach (var item in timesheetApprovaldetails)
            {
                if(item.IsDeleted == true)
                {
                    var timeSheetItems = Db.EmployeeTimesheetItems.Where(x => x.TimesheetItemID == item.TimesheetItemID).FirstOrDefault();
                    if (timeSheetItems != null)
                    {
                        Db.EmployeeTimesheetItems.Attach(timeSheetItems);
                        Db.EmployeeTimesheetItems.Remove(timeSheetItems);
                        Db.SaveChanges();
                    }
                    TempData["InfoMessage"] = new InfoMessage(InfoMessageType.Success, "Succesfully deleted Zero Timesheet Items");
                }
            }
            
            return RedirectToAction("Index");
        }

        private string timecodename(int time)
        {
            var result = string.Empty;
            switch (time)
            {
                case 0:
                    result = "NT: Normal Time";
                    break;
                case 1:
                    result = "OT1: Other Time 1";
                    break;
                case 2:
                    result = "OT2: Other Time 2";
                    break;
                case 3:
                    result = "OT3: Other Time 3";
                    break;
                case 4:
                    result = "OT4: Other Time 4";
                    break;
                case 5:
                    result = "OT5: Other Time 5";
                    break;
                case 6:
                    result = "OT6: Other Time 6";
                    break;
                case 7:
                    result = "OT7: Other Time 7";
                    break;
            }
            return result;
        }
        private int timecode(TimeCode time)
        {
            int result = 0;
            switch (time)
            {
                case TimeCode.NT:
                    result = 0;
                    break;
                case TimeCode.OT1:
                    result = 1;
                    break;
                case TimeCode.OT2:
                    result = 2;
                    break;
                case TimeCode.OT3:
                    result = 3;
                    break;
                case TimeCode.OT4:
                    result = 4;
                    break;
                case TimeCode.OT5:
                    result = 5;
                    break;
                case TimeCode.OT6:
                    result = 6;
                    break;
                case TimeCode.OT7:
                    result = 7;
                    break;

            }
            return result;
        }
    }
}