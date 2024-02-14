using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
    public class ReconciliationEntriesController : BaseController
    {
        public const string UnclassifiedText = "Unclassified";

        private class TempItem
        {
            public int TimesheetId { get; set; }
            public decimal? Hours { get; set; }
        }

        public ActionResult Index(int? reconciliationTypeId, bool? hideComplete, bool? IsRefresh)
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;

            if (IsRefresh == true)
            {
                List<RefreshreconcillationViewModel> allData = Db.Database.SqlQuery<RefreshreconcillationViewModel>("EXEC GetRefreshReconciliation @ProjectId",
                   new SqlParameter("ProjectId", projectId)).ToList();

                foreach (var item in allData)
                {
                    SaveRefreshTimesheetId(item.Id, item.EmployeeTimesheetId_SHOULDBE);
                }
            }

            Dictionary<int, decimal?> etthours = Db.EmployeeTimesheetItems
                .Where(x => x.ProjectTask.ProjectID == projectId)
                .Select(x => new TempItem
                {
                    TimesheetId = x.TimesheetID,
                    Hours = x.Day1Hrs + x.Day2Hrs + x.Day3Hrs + x.Day4Hrs + x.Day5Hrs + x.Day6Hrs + x.Day7Hrs
                }).ToList().GroupBy(x => x.TimesheetId).ToDictionary(x => x.Key, x => x.Sum(y => y.Hours));

            IQueryable<ReconciliationEntry> query = Db.ReconciliationEntries.Where(x => !x.Deleted && !x.HoursEqual && x.OriginalReconciliationUpload.ProjectId == projectId);

            if (reconciliationTypeId.HasValue)
            {
                query = reconciliationTypeId == -1 ? query.Where(x => x.ReconciliationTypeId == null) : query.Where(x => x.ReconciliationTypeId == reconciliationTypeId);
            }

            if (hideComplete.HasValue)
            {
                bool boolHideComplete = (bool) hideComplete;
                if (boolHideComplete)
                {
                    query = query.Where(x => !x.Complete);
                }
            }
            else
            {
                query = query.Where(x => !x.Complete);
            }

            List<ReconciliationEntryItemViewModel> items = query.Select(x => new ReconciliationEntryItemViewModel
            {
                Id = x.Id,
                EmployeeNo = x.Employee.EmployeeNo,
                EmployeeNames = x.Employee.Names,
                HoursEqual = x.HoursEqual,
                ReconciliationTypeId = x.ReconciliationTypeId,
                TimesheetPeriodEndDate = x.TimesheetPeriod.EndDate,
                CompanyName = x.Employee.Company.Company_Name,
                Comments = x.ReconciliationComment,
                HoursExternal = x.Hours,
                EmployeeTimesheetId = x.EmployeeTimesheetId,
                EmployeeComments = x.EmployeeComment,
                Complete = x.Complete
            }).OrderBy(x => x.EmployeeNames).ThenByDescending(x => x.TimesheetPeriodEndDate).ToList();

            foreach (ReconciliationEntryItemViewModel item in items)
            {
                item.HoursETT = item.EmployeeTimesheetId.HasValue && etthours.ContainsKey(item.EmployeeTimesheetId.Value)
                    ? etthours[item.EmployeeTimesheetId.Value]
                    : (decimal?) null;
            }

            List<SelectListItem> reconciliationTypes = GetReconciliationTypesDropdown();
            ViewBag.ReconciliationTypes = reconciliationTypes;

            SelectListItem nullSelectListItem = new SelectListItem {Text = UnclassifiedText, Value = "-1"};
            List<SelectListItem> fullList = new List<SelectListItem> {nullSelectListItem}.Union(reconciliationTypes).ToList();
            ViewBag.ReconciliationTypeFilterSelectList = fullList;

            ReconciliationEntriesIndexViewModel vm = new ReconciliationEntriesIndexViewModel
            {
                HideComplete = hideComplete ?? true,
                ReconciliationTypeIdFilter = reconciliationTypeId,
                ReconciliationEntries = items
            };

            return View(vm);
        }

        private List<SelectListItem> GetReconciliationTypesDropdown()
        {
            List<SelectListItem> selectItems = Db.ReconciliationTypes.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Text }).ToList();
            return selectItems;
        }

        public JsonResult SaveCompleteStatus(int id, bool complete)
        {
            ReconciliationEntry existingItem = Db.ReconciliationEntries.Find(id);
            if (existingItem != null)
            {
                existingItem.Complete = complete;
                Db.SaveChanges();
                return Json(true);
            }

            return Json(false);
        }

        public JsonResult SaveComment(int id, string comment)
        {
            ReconciliationEntry existingItem = Db.ReconciliationEntries.Find(id);
            if (existingItem != null)
            {
                existingItem.ReconciliationComment = comment;
                Db.SaveChanges();
                return Json(true);
            }

            return Json(false);
        }

        public JsonResult SaveEmployeeComment(int id, string comment)
        {
            ReconciliationEntry existingItem = Db.ReconciliationEntries.Find(id);
            if (existingItem != null)
            {
                existingItem.EmployeeComment = comment;
                Db.SaveChanges();
                return Json(true);
            }

            return Json(false);
        }

        public void SaveRefreshTimesheetId(int id, int? employeeTimesheetId)
        {
            ReconciliationEntry existingItem = Db.ReconciliationEntries.Find(id);
            if (existingItem != null)
            {
                existingItem.EmployeeTimesheetId = employeeTimesheetId;
                Db.SaveChanges();
            }
        }

        public JsonResult SaveReconciliationType(int id, int? reconciliationTypeId)
        {
            ReconciliationEntry existingItem = Db.ReconciliationEntries.Find(id);
            if (existingItem != null)
            {
                existingItem.ReconciliationTypeId = reconciliationTypeId;
                Db.SaveChanges();
                return Json(true);
            }

            return Json(false);
        }
    }
}