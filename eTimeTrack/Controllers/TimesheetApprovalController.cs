using eTimeTrack.Enums;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace eTimeTrack.Controllers
{
    [Authorize]
    public class TimesheetApprovalController : BaseController
    {
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
                                 && (e.Day1Hrs > 0 || e.Day2Hrs > 0 || e.Day3Hrs > 0 || e.Day4Hrs > 0 || e.Day5Hrs > 0 || e.Day6Hrs > 0 || e.Day7Hrs > 0)
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
                                     Reviewercomments = e.Reviewercomments,
                                     ProjectId = t.ProjectID
                                 };

            var res = empresultslist.Distinct().OrderByDescending(x => x.EndDate).ToList();
            if (res.Count > 0)
            {
                foreach (var item in res)
                {
                    item.Timecodes = timecode(item.TimeCode);
                    item.TimecodesName = timecodename(item.Timecodes, item.ProjectId);
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
                if (item.IsApproval == true)
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

        public JsonResult SaveComment(int? id, string comment)
        {
            EmployeeTimesheetItem existingItem = Db.EmployeeTimesheetItems.Find(id);
            var result = (from i in Db.EmployeeTimesheetItems
                         join t in Db.EmployeeTimesheets on i.TimesheetID equals t.TimesheetID
                         join e in Db.Users on t.EmployeeID equals e.Id
                         where i.TimesheetItemID == id
                         select new
                         {
                             email = e.Email,
                             lastapprovedby = i.LastApprovedBy
                         }).FirstOrDefault();

            var emailto = result.email;
            var sub = "Timesheet Reviewer Comments";
            var body = "A reviewer has raised a query on your recent eTimeTrack timesheet. The query can be accessed via My Timesheets in eTimeTrack. Please address this as soon as possible.";
            if (existingItem != null)
            {
                existingItem.Reviewercomments = comment;
                Db.SaveChanges();
                EmailHelper.SendEmail(emailto, sub, body);
                return Json(true);
            }
            return Json(false);
        }

        private string timecodename(int time, int projectid)
        {
            var Configs = Db.ProjectTimeCodeConfigs.Where(x => x.ProjectID == projectid).FirstOrDefault();

            var result = string.Empty;
            switch (time)
            {
                case 0:
                    result = !string.IsNullOrEmpty(Configs.NTName) ? Configs.NTName : "NT: Normal Time";
                    break;
                case 1:
                    result = !string.IsNullOrEmpty(Configs.OT1Name) ? Configs.OT1Name : "Unapproved Overtime & Non-Billable Time - OT1: Other Time 1";
                    break;
                case 2:
                    result = !string.IsNullOrEmpty(Configs.OT2Name) ? Configs.OT2Name : "Unapproved Overtime & Non-Billable Time - OT2: Other Time 2";
                    break;
                case 3:
                    result = !string.IsNullOrEmpty(Configs.OT3Name) ? Configs.OT3Name : "Unapproved Overtime & Non-Billable Time - OT3: Other Time 3";
                    break;
                case 4:
                    result = !string.IsNullOrEmpty(Configs.OT4Name) ? Configs.OT4Name : "Unapproved Overtime & Non-Billable Time - OT4: Other Time 4";
                    break;
                case 5:
                    result = !string.IsNullOrEmpty(Configs.OT5Name) ? Configs.OT5Name : "Unapproved Overtime & Non-Billable Time - OT5: Other Time 5";
                    break;
                case 6:
                    result = !string.IsNullOrEmpty(Configs.OT6Name) ? Configs.OT6Name : "Unapproved Overtime & Non-Billable Time - OT6: Other Time 6";
                    break;
                case 7:
                    result = !string.IsNullOrEmpty(Configs.OT7Name) ? Configs.OT7Name : "Unapproved Overtime & Non-Billable Time - OT7: Other Time 7";
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