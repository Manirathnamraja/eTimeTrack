using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eTimeTrack.Models;
using eTimeTrack.Extensions;
using eTimeTrack.Helpers;
using System.Web.Configuration;
using eTimeTrack.Enums;
using eTimeTrack.Exceptions;
using eTimeTrack.ViewModels;
using OfficeOpenXml;
using Spire.Doc;
using Spire.Xls;

namespace eTimeTrack.Controllers
{
    [Authorize]
    public class EmployeeTimesheetsController : BaseController
    {
        private const int DaysInWeek = 7;
        private static readonly decimal MaximumDailyWorkHours;

        static EmployeeTimesheetsController()
        {
            MaximumDailyWorkHours = decimal.Parse(WebConfigurationManager.AppSettings["MaximumDailyWorkHours"]);
        }

        [AdminRedirectFilter]
        public ActionResult Index()
        {
            return IndexGeneral();
        }

        public ActionResult TimesheetIndex()
        {
            return IndexGeneral();
        }

        private ActionResult IndexGeneral()
        {
            int id = UserHelpers.GetCurrentUserId();
            List<OpenEmployeeTimesheet> employeeTimesheets = Db.EmployeeTimesheets
                .Where(x => x.EmployeeID == id)
                .OrderByDescending(x => x.TimesheetPeriod.StartDate).Select(x => new OpenEmployeeTimesheet { EmployeeTimesheet = x, Open = true })
                .ToList();


            ViewBag.InfoMessage = TempData["InfoMessage"];

            TimesheetPeriod existingPeriod = GetCurrentTimesheetPeriod();
            ViewBag.MobileMode = IsMobileDevice() && existingPeriod != null;

            // Check for closed projects and periods
            if (!User.IsSuperUser())
            {

                CloseUserClosedPeriods(employeeTimesheets);
            }

            return View("Index", new EmployeeTimesheetIndexViewModel
            {
                Employee = UserHelpers.GetCurrentUser(),
                EmployeeTimesheets = employeeTimesheets
            });
        }

        private void CloseUserClosedPeriods(List<OpenEmployeeTimesheet> employeeTimesheets)
        {
            List<int> openUserProjectPeriods = OpenUserProjectPeriods();
            foreach (OpenEmployeeTimesheet timesheet in employeeTimesheets)
            {
                if (!openUserProjectPeriods.Contains(timesheet.EmployeeTimesheet.TimesheetPeriodID) ||
                    timesheet.EmployeeTimesheet.TimesheetPeriod.IsClosed)
                {
                    timesheet.Open = false;
                }
            }
        }

        public ActionResult Details(int? id)
        {
            return TimesheetEditView(id, true);
        }

        public JsonResult GetProjectUserTypeAndMandatoryCommentsInfo(int? employeeId)
        {
            List<EmployeeProject> employeeProjects = Db.EmployeeProjects.Include(x => x.ProjectUserType).Include(x => x.ProjectUserType.UserType).Where(x => x.EmployeeId == employeeId).ToList();

            List<EmployeeProjectUserTypeInfo> projectUserTypesAndMandatoryCommentMode = new List<EmployeeProjectUserTypeInfo>();

            List<Project> projects = Db.Projects.Include(x => x.ProjectUserTypes).ToList();

            foreach (Project project in projects)
            {
                ProjectUserType foundProjectUserType = employeeProjects.SingleOrDefault(x => x.ProjectId == project.ProjectID)?.ProjectUserType;

                if (foundProjectUserType == null)
                {
                    foundProjectUserType = project.ProjectUserTypes.SingleOrDefault(x => x.UserTypeID == null);
                }

                projectUserTypesAndMandatoryCommentMode.Add(new EmployeeProjectUserTypeInfo
                {
                    ProjectName = project.Name,
                    ProjectID = project.ProjectID,
                    MaxNTHours = foundProjectUserType?.MaxNTHours,
                    MaxOT1Hours = foundProjectUserType?.MaxOT1Hours,
                    MaxOT2Hours = foundProjectUserType?.MaxOT2Hours,
                    MaxOT3Hours = foundProjectUserType?.MaxOT3Hours,
                    MandatoryComments = project.CommentsMandatory,
                    //Added 4 new time codes
                    MaxOT4Hours = foundProjectUserType?.MaxOT4Hours,
                    MaxOT5Hours = foundProjectUserType?.MaxOT5Hours,
                    MaxOT6Hours = foundProjectUserType?.MaxOT6Hours,
                    MaxOT7Hours = foundProjectUserType?.MaxOT7Hours,
                });
            }

            return Json(projectUserTypesAndMandatoryCommentMode);
        }

        private ActionResult TimesheetEditView(int? id, bool readOnly = false)
        {
            if (id == null)
            {
                return InvokeHttp400(HttpContext);
            }

            EmployeeTimesheet timesheet = Db.EmployeeTimesheets.Include(x => x.TimesheetItems.Select(y => y.ProjectTask.Project)).Include(x => x.ReconciliationEntries).SingleOrDefault(x => x.TimesheetID == id);

            if (timesheet == null)
            {
                return InvokeHttp404(HttpContext);
            }

            if (timesheet.Employee.Id != UserHelpers.GetCurrentUserId() && !User.IsInAnyAdminRole())
            {
                return InvokeHttp404(HttpContext);
            }

            // set 0 hour values to null for view
            SetZeroHoursToNull(timesheet);

            // check if viewer is able to edit individual item rows
            SetItemReadOnlyStatus(timesheet);

            ViewBag.ReadOnly = readOnly;

            if (!readOnly)
            {
                ViewBag.ProjectSelector = GenerateDropdownUserPeriodOpenProjects(timesheet.TimesheetPeriod); // list for currently logged in user
            }

            ViewBag.PreviousTimesheet = GetPreviousTimesheet(timesheet);
            ViewBag.NextTimesheet = GetNextTimesheet(timesheet);


            ViewBag.MobileMode = IsMobileDevice();

            SetTimeCodeTexts(timesheet);

            return View("Timesheet", timesheet);
        }

        private void SetTimeCodeTexts(EmployeeTimesheet timesheet)
        {
            IEnumerable<int?> configIds = timesheet.TimesheetItems.Select(x => x.ProjectTask.Project.ProjectTimeCodeConfigId ?? x.ProjectTask.Project.ProjectTimeCodeConfig?.ProjectTimeCodeConfigId).Where(x => x.HasValue).Distinct();
            List<ProjectTimeCodeConfig> allTimeSheetProjectTimeCodeConfigs = Db.ProjectTimeCodeConfigs.Where(x => configIds.Contains(x.ProjectTimeCodeConfigId)).ToList();

            foreach (EmployeeTimesheetItem item in timesheet.TimesheetItems)
            {
                item.TimeCodeText = GetTimeCodeText(item.TimeCode, allTimeSheetProjectTimeCodeConfigs.SingleOrDefault(x => x.ProjectID == item.ProjectTask.ProjectID));
            }
        }

        private string GetTimeCodeText(TimeCode itemTimeCode, ProjectTimeCodeConfig config)
        {
            switch (itemTimeCode)
            {
                case TimeCode.NT:
                    return config?.NTName ?? TimeCode.NT.GetDisplayName();
                case TimeCode.OT1:
                    return config?.OT1Name ?? TimeCode.OT1.GetDisplayName();
                case TimeCode.OT2:
                    return config?.OT2Name ?? TimeCode.OT2.GetDisplayName();
                case TimeCode.OT3:
                    return config?.OT3Name ?? TimeCode.OT3.GetDisplayName();
            }
            return null;
        }

        [Authorize(Roles = UserHelpers.AuthTextTimesheetEditorOrAbove)]
        public ActionResult CreateSelect(string message = null)
        {
            ViewBag.EmployeeList = GenerateDropdownUsers();

            if (message != null)
            {
                ViewBag.Message = message;
            }
            return View();
        }

        public ActionResult CreateCurrent(int? id = null, bool duplicate = false)
        {
            int userId = id ?? UserHelpers.GetCurrentUserId();
            List<EmployeeTimesheet> existingTimesheetsForUser = Db.EmployeeTimesheets.Where(x => x.EmployeeID == userId).OrderByDescending(x => x.TimesheetPeriod.EndDate).ToList();
            TimesheetPeriod currentPeriod = GetCurrentTimesheetPeriod();

            EmployeeTimesheet currentTimesheet = existingTimesheetsForUser.FirstOrDefault(x => x.TimesheetPeriodID == currentPeriod.TimesheetPeriodID);

            if (currentTimesheet != null)
            {
                return RedirectToAction("Timesheet", new { id = currentTimesheet.TimesheetID });
            }

            EmployeeTimesheet timesheet = new EmployeeTimesheet { EmployeeID = userId, TimesheetPeriodID = currentPeriod.TimesheetPeriodID };

            if (duplicate && existingTimesheetsForUser.Any())
            {
                EmployeeTimesheet previousTimesheet = GetMostRecentTimesheetWithItems(timesheet);
                ApplyTimesheetItemsToTimesheet(timesheet, previousTimesheet);
            }

            Db.EmployeeTimesheets.Add(timesheet);
            Db.SaveChanges();
            return RedirectToAction("Timesheet", new { id = timesheet.TimesheetID });
        }

        [HttpPost]
        [Authorize(Roles = UserHelpers.AuthTextTimesheetEditorOrAbove)]
        public ActionResult CreateSelect(int? EmployeeId)
        {
            return RedirectToAction("Create", new { id = EmployeeId, adminMode = true });
        }

        public ActionResult Create(int? id = null, bool adminMode = false)
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
            EmployeeTimesheet timesheet = new EmployeeTimesheet { EmployeeID = userId, TimesheetPeriodID = currentExists ? 0 : currentPeriod?.TimesheetPeriodID ?? 0 };

            return View(timesheet);
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



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EmployeeTimesheet employeeTimesheet, int? timesheetPeriodDuplicateID = null, bool duplicateHours = false)
        {
            bool existingFromPeriod = Db.EmployeeTimesheets.Any(x => x.EmployeeID == employeeTimesheet.EmployeeID && x.TimesheetPeriodID == employeeTimesheet.TimesheetPeriodID);

            if (existingFromPeriod)
            {
                TempData["InfoMessage"] = new InfoMessage { MessageContent = "Timesheet for period already exists", MessageType = InfoMessageType.Failure };
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                if (employeeTimesheet.EmployeeID == 0)
                {
                    employeeTimesheet.EmployeeID = UserHelpers.GetCurrentUserId();
                }
                else
                {
                    // flag that editing the timesheet has come from a create method from an admin
                    Session.Add("_EditTimesheetFromCreate", true);
                }

                // If a duplicate timesheet is specified then bind duplicates of the items onto the newly created timesheet
                if (timesheetPeriodDuplicateID != null)
                {
                    ApplyPeriodTimesheetItemsToTimesheet(employeeTimesheet, timesheetPeriodDuplicateID);
                }

                Db.EmployeeTimesheets.Add(employeeTimesheet);
                Db.SaveChanges();
                return RedirectToAction("Timesheet", new { id = employeeTimesheet.TimesheetID });
            }

            ViewBag.TimesheetPeriodID = new SelectList(Db.TimesheetPeriods, "TimesheetPeriodID", "TimesheetPeriodID", employeeTimesheet.TimesheetPeriodID);

            return View(employeeTimesheet);
        }

        private void ApplyPeriodTimesheetItemsToTimesheet(EmployeeTimesheet timesheet, int? periodDuplicateId)
        {
            // get existing timesheetitems associated with the existing timesheet
            EmployeeTimesheet existingPreviousTimesheet = Db.EmployeeTimesheets.SingleOrDefault(
                x => x.TimesheetPeriodID == periodDuplicateId && x.EmployeeID == timesheet.EmployeeID);

            ApplyTimesheetItemsToTimesheet(timesheet, existingPreviousTimesheet);
        }

        private void ApplyTimesheetItemsToTimesheet(EmployeeTimesheet timesheet, EmployeeTimesheet existingPreviousTimesheet, bool deleteExistingItems = true)
        {
            if (existingPreviousTimesheet?.TimesheetItems == null) return;

            List<int> assignedProjects = GetAssignedProjects(existingPreviousTimesheet.EmployeeID).Select(x => x.ProjectID).ToList();

            // get available items
            List<EmployeeTimesheetItem> existingPreviousTimesheetItems = Db.EmployeeTimesheetItems.Where(x => x.TimesheetID == existingPreviousTimesheet.TimesheetID && assignedProjects.Contains(x.ProjectTask.ProjectID) && !x.ProjectTask.Project.IsClosed && !x.ProjectTask.ProjectGroup.ProjectPart.IsClosed && !x.ProjectTask.ProjectGroup.IsClosed && !x.ProjectTask.IsClosed && !x.Variation.IsClosed).ToList();
            List<EmployeeTimesheetItem> clonedTimesheetItems = new List<EmployeeTimesheetItem>();

            // manually create duplicates from the existing items
            foreach (EmployeeTimesheetItem item in existingPreviousTimesheetItems)
            {
                // skip existing items in timesheet
                List<EmployeeTimesheetItem> itemDuplicateTaskItems = timesheet.TimesheetItems
                    .Where(x => x.TaskID == item.TaskID && x.VariationID == item.VariationID && x.TimeCode == item.TimeCode)
                    .ToList();

                if (itemDuplicateTaskItems.Any())
                {
                    continue;
                }

                clonedTimesheetItems.Add(CreateNewTimesheetItem(timesheet.TimesheetID, item.VariationID, item.TaskID, item.TimeCode));
            }

            if (deleteExistingItems) timesheet.TimesheetItems.Clear();

            ((List<EmployeeTimesheetItem>)timesheet.TimesheetItems).AddRange(clonedTimesheetItems);
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

        public ActionResult EditExistingTimesheet()
        {
            EmployeeTimesheet existing = GetCurrentExistingTimesheet();

            if (existing == null)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Timesheet", new { id = existing.TimesheetID });
        }

        public ActionResult Timesheet(int? id)
        {
            if (id == null) return InvokeHttp404(HttpContext);

            ViewBag.DuplicatesAvailable = PotentialDuplicateAddItems((int)id);
            ViewBag.InfoMessage = TempData["InfoMessage"];
            return TimesheetEditView(id);
        }

        private void SetItemReadOnlyStatus(EmployeeTimesheet employeeTimesheet)
        {
            List<Project> assignedProjects = GetOpenProjectsAssignedToUser(employeeTimesheet.TimesheetPeriod);
            foreach (EmployeeTimesheetItem item in employeeTimesheet.TimesheetItems)
            {
                if (!assignedProjects.Select(x => x.ProjectID).Contains(item.ProjectTask.ProjectID))
                {
                    item.ReadOnly = true;
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Timesheet(EmployeeTimesheet employeeTimesheet, int? nextTimesheetAfterSave)
        {
            ViewBag.MobileMode = IsMobileDevice();
            // in case of null items list, create empty list
            if (employeeTimesheet.TimesheetItems == null)
            {
                employeeTimesheet.TimesheetItems = new List<EmployeeTimesheetItem>();
            }

            bool alreadyExistingTimesheetForPeriod = Db.EmployeeTimesheets.Any(x => x.TimesheetID != employeeTimesheet.TimesheetID && x.EmployeeID == employeeTimesheet.EmployeeID && x.TimesheetPeriodID == employeeTimesheet.TimesheetPeriodID);

            // another timesheet already exists
            if (alreadyExistingTimesheetForPeriod)
            {
                TempData["InfoMessage"] = new InfoMessage { MessageContent = "A timesheet for this period already exists. Data has not been saved.", MessageType = InfoMessageType.Failure };
                return RedirectToAction("TimesheetIndex");
            }

            // Determine if hours are valid (within maximum limit)
            if (!ValidDailyHours(employeeTimesheet))
            {
                ViewBag.ProjectSelector = new SelectList(Db.Projects, "ProjectID", "Name");

                return View(employeeTimesheet);
            }

            if (ModelState.IsValid)
            {
                // remove high-level links to Project and Project Part
                foreach (EmployeeTimesheetItem item in employeeTimesheet.TimesheetItems)
                {
                    item.ProjectTask.ProjectGroup = null;
                }

                // set null hour values to 0 for database
                SetNullHoursToZero(employeeTimesheet);

                UpdateTimesheetItems(employeeTimesheet);
                Db.SaveChanges();

                // redirect to next timesheet if selected
                if (nextTimesheetAfterSave.HasValue)
                {
                    TempData["InfoMessage"] = new InfoMessage { MessageContent = "Timesheet save completed", MessageType = InfoMessageType.Success };
                    return RedirectToAction("Timesheet", new { id = nextTimesheetAfterSave });
                }

                //admin redirect
                if (UserHelpers.GetCurrentUserId() != employeeTimesheet.EmployeeID)
                {
                    if (Session["_EditTimesheetFromCreate"] != null)
                    {
                        Session.Remove("_EditTimesheetFromCreate");
                        return RedirectToAction("CreateSelect", "EmployeeTimesheets", new { message = "Timesheet save completed" });
                    }
                    return RedirectToAction("EditSelect", "EmployeeTimesheets", new { message = "Timesheet save completed" });
                }

                // normal redirect
                TempData["InfoMessage"] = new InfoMessage { MessageContent = "Timesheet save completed", MessageType = InfoMessageType.Success };
                return RedirectToAction("TimesheetIndex");
            }

            string messages = string.Join(" ",
                ModelState.Values.Where(x => x.Errors.Any())
                    .Select(y => string.Join(" ", y.Errors.Select(a => a.ErrorMessage))));

            ViewBag.ProjectSelector = new SelectList(Db.Projects, "ProjectID", "Name");

            ViewBag.Message = "Invalid entries are detected, please try again: " + messages;
            ViewBag.DuplicatesAvailable = PotentialDuplicateAddItems(employeeTimesheet.EmployeeID);

            return View(employeeTimesheet);
        }

        private static void SetNullHoursToZero(EmployeeTimesheet employeeTimesheet)
        {
            SetHours(employeeTimesheet, null, 0);
        }


        private static void SetZeroHoursToNull(EmployeeTimesheet employeeTimesheet)
        {
            SetHours(employeeTimesheet, 0, null);
        }

        private static void SetHours(EmployeeTimesheet employeeTimesheet, decimal? check, decimal? set)
        {
            foreach (EmployeeTimesheetItem item in employeeTimesheet.TimesheetItems)
            {
                if (item.Day1Hrs == check) item.Day1Hrs = set;
                if (item.Day2Hrs == check) item.Day2Hrs = set;
                if (item.Day3Hrs == check) item.Day3Hrs = set;
                if (item.Day4Hrs == check) item.Day4Hrs = set;
                if (item.Day5Hrs == check) item.Day5Hrs = set;
                if (item.Day6Hrs == check) item.Day6Hrs = set;
                if (item.Day7Hrs == check) item.Day7Hrs = set;
            }
        }

        private void UpdateTimesheetItems(EmployeeTimesheet timesheet)
        {
            EmployeeTimesheet existingTimesheet = Db.EmployeeTimesheets.SingleOrDefault(x => x.TimesheetID == timesheet.TimesheetID);

            // Add timesheet items
            foreach (EmployeeTimesheetItem item in timesheet.TimesheetItems)
            {
                EmployeeTimesheetItem existingItem = existingTimesheet?.TimesheetItems.SingleOrDefault(x => x.TimesheetItemID == item.TimesheetItemID);
                if (existingItem != null)
                {
                    Db.Entry(existingItem).CurrentValues.SetValues(item);
                }
                else
                {
                    Db.EmployeeTimesheetItems.Add(item);
                }
            }

            // Remove deleted items from DB
            if (existingTimesheet != null)
            {
                List<EmployeeTimesheetItem> deleted = existingTimesheet.TimesheetItems.Where(x => timesheet.TimesheetItems.All(y => y.TimesheetItemID != x.TimesheetItemID)).ToList();
                foreach (EmployeeTimesheetItem item in deleted)
                {
                    Db.EmployeeTimesheetItems.Remove(item);
                }
            }
            Db.Entry(existingTimesheet).State = EntityState.Modified;
        }

        [HttpGet]
        [Authorize(Roles = UserHelpers.AuthTextTimesheetEditorOrAbove)]
        public ActionResult EditSelect(string message = null)
        {
            ViewBag.EmployeeList = GenerateDropdownUsers();

            if (message != null)
            {
                ViewBag.Message = message;
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = UserHelpers.AuthTextTimesheetEditorOrAbove)]
        public ActionResult EditSelect(int? timesheetId)
        {
            Session.Remove("_EditTimesheetFromCreate");
            return RedirectToAction("Timesheet", new { Id = timesheetId });
        }

        public JsonResult GetEmployeeTimesheetPeriods(int? employeeId)
        {
            List<EmployeeTimesheet> timesheets = Db.EmployeeTimesheets.Where(x => x.EmployeeID == employeeId).OrderByDescending(x => x.TimesheetPeriod.StartDate).ToList();
            return Json(timesheets.Select(x => new { Value = x.TimesheetID, Text = x.TimesheetPeriod.GetStartEndDates() }));
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return InvokeHttp404(HttpContext);

            EmployeeTimesheet employeeTimesheet = Db.EmployeeTimesheets.Find(id);
            if (employeeTimesheet == null)
                return InvokeHttp404(HttpContext);

            return View(employeeTimesheet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EmployeeTimesheet employeeTimesheet = Db.EmployeeTimesheets.Find(id);
            if (employeeTimesheet != null) Db.EmployeeTimesheets.Remove(employeeTimesheet);
            Db.SaveChanges();
            return RedirectToAction("Index");
        }

        public FileResult DownloadUserTimesTemplate()
        {
            FileInfo dir = new FileInfo(Server.MapPath("~/Content/Templates/User Times Import Template.xlsx"));
            return File(dir.FullName, System.Net.Mime.MediaTypeNames.Application.Octet, "User Times Import Template.xlsx");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        public JsonResult GetProjectPartListAndTimeCodes(int? projectId)
        {
            if (projectId != null)
            {
                var projectParts = GetProjectParts(projectId).Select(x => new
                {
                    PartID = x.PartID,
                    Name = x.DisplayName
                });
                return Json(new
                {
                    ProjectParts = projectParts,
                    TimeCodes = GetProjectTimeCodes(projectId)
                });
            }
            return Json(null);
        }

        [HttpPost]
        public JsonResult GetProjectPartList(int? projectId)
        {
            if (projectId != null)
            {
                return Json(GetProjectParts(projectId).Select(x => new
                {
                    PartID = x.PartID,
                    Name = x.DisplayName
                }));
            }
            return Json(null);
        }

        private IEnumerable<TimeCodeInfoViewModel> GetProjectTimeCodes(int? projectId)
        {
            Project project = Db.Projects.Find(projectId);

            ProjectTimeCodeConfig timeCodeConfig = project?.ProjectTimeCodeConfig;

            if (timeCodeConfig == null)
            {
                yield return new TimeCodeInfoViewModel { TimeCode = TimeCode.NT, Name = TimeCode.NT.GetDisplayName(), Notes = null };
                yield break;
            }

            yield return new TimeCodeInfoViewModel { TimeCode = TimeCode.NT, Name = timeCodeConfig.NTName ?? TimeCode.NT.GetDisplayName(), Notes = timeCodeConfig.NTNotes };

            if (timeCodeConfig.DisplayOT1)
            {
                yield return new TimeCodeInfoViewModel { TimeCode = TimeCode.OT1, Name = timeCodeConfig.OT1Name ?? TimeCode.OT1.GetDisplayName(), Notes = timeCodeConfig.OT1Notes };
            }
            if (timeCodeConfig.DisplayOT2)
            {
                yield return new TimeCodeInfoViewModel { TimeCode = TimeCode.OT2, Name = timeCodeConfig.OT2Name ?? TimeCode.OT2.GetDisplayName(), Notes = timeCodeConfig.OT2Notes };
            }
            if (timeCodeConfig.DisplayOT3)
            {
                yield return new TimeCodeInfoViewModel { TimeCode = TimeCode.OT3, Name = timeCodeConfig.OT3Name ?? TimeCode.OT3.GetDisplayName(), Notes = timeCodeConfig.OT3Notes };
            }
            //Added new 4 time codes
            if (timeCodeConfig.DisplayOT4)
            {
                yield return new TimeCodeInfoViewModel { TimeCode = TimeCode.OT4, Name = timeCodeConfig.OT4Name ?? TimeCode.OT4.GetDisplayName(), Notes = timeCodeConfig.OT4Notes };
            }
            if (timeCodeConfig.DisplayOT5)
            {
                yield return new TimeCodeInfoViewModel { TimeCode = TimeCode.OT5, Name = timeCodeConfig.OT5Name ?? TimeCode.OT5.GetDisplayName(), Notes = timeCodeConfig.OT5Notes };
            }
            if (timeCodeConfig.DisplayOT6)
            {
                yield return new TimeCodeInfoViewModel { TimeCode = TimeCode.OT6, Name = timeCodeConfig.OT6Name ?? TimeCode.OT6.GetDisplayName(), Notes = timeCodeConfig.OT6Notes };
            }
            if (timeCodeConfig.DisplayOT7)
            {
                yield return new TimeCodeInfoViewModel { TimeCode = TimeCode.OT7, Name = timeCodeConfig.OT7Name ?? TimeCode.OT7.GetDisplayName(), Notes = timeCodeConfig.OT7Notes };
            }
        }


        [HttpPost]
        public JsonResult GetProjectGroupList(int? projectPartId)
        {
            if (projectPartId != null)
            {
                List<ProjectGroup> projectGroups = Db.ProjectGroups.Where(x => x.PartID == projectPartId && !x.IsClosed).OrderBy(x => x.GroupNo).ToList();
                return Json(projectGroups.Select(x => new
                {
                    GroupID = x.GroupID,
                    Name = x.DisplayName
                }));
            }
            return null;
        }

        [HttpPost]
        public FileResult PrintFriendly(int? id, DateTime dateTime)
        {
            EmployeeTimesheet timesheet = Db.EmployeeTimesheets.Find(id);
            SetZeroHoursToNull(timesheet);
            FileInfo filePath = GetGuidFilePath("xlsx");
            PrintFriendlyTimesheetExcel.WriteTimesheet(timesheet, filePath, dateTime);

            FileInfo pdfFilePath = WritePdfExcel(filePath);
            byte[] bytes = System.IO.File.ReadAllBytes(pdfFilePath.FullName);

            // delete local files
            filePath.Delete();
            pdfFilePath.Delete();

            string filename = $"Timesheet_{dateTime:yyyy-MM-dd_HHmmss}.pdf";
            return File(bytes, "application/pdf", filename);
        }

        [HttpPost]
        public JsonResult GetTaskList(int? projectGroupId)
        {
            if (projectGroupId != null)
            {
                List<ProjectTask> projectTasks = Db.ProjectTasks.Where(x => x.GroupID == projectGroupId && !x.IsClosed).OrderBy(x => x.TaskNo).ToList();
                return Json(projectTasks.Select(x => new
                {
                    TaskID = x.TaskID,
                    Name = x.DisplayName
                }));
            }
            return null;
        }

        [HttpPost]
        public JsonResult GetVariationList(int? projectTaskId)
        {
            if (projectTaskId == null) return Json(null);

            // Exclude Closed and Non-approved ProjectVariations and ProjectVariationItems
            List<ProjectVariationItem> projectVariationItems = Db.ProjectVariationItems.Where(x => !x.ProjectVariation.IsClosed && x.TaskID == projectTaskId && !x.IsClosed).OrderBy(x => x.ProjectVariation.VariationNo).ToList();
            return Json(projectVariationItems.Select(x => new
            {
                VariationID = x.VariationID,
                Description = x.ProjectVariation.DisplayName
            }));
        }

        public ActionResult AddTimesheetItem(int timesheetId, int variationId, int taskId, TimeCode timeCode, string timeCodeText)
        {
            // create a new entry
            EmployeeTimesheetItem newItem = CreateNewTimesheetItem(timesheetId, variationId, taskId, timeCode, timeCodeText);
            Db.EmployeeTimesheetItems.Add(newItem);
            Db.SaveChanges();

            EmployeeTimesheetItem inDb = Db.EmployeeTimesheetItems.SingleOrDefault(x => x.TimesheetItemID == newItem.TimesheetItemID);
            EmployeeTimesheet timesheet = new EmployeeTimesheet { TimesheetID = timesheetId };
            timesheet.TimesheetItems.Add(inDb);

            SetZeroHoursToNull(timesheet);

            ViewBag.MobileMode = IsMobileDevice();
            return View(timesheet);
        }

        public ActionResult DuplicateAddItems(int timesheetId)
        {
            EmployeeTimesheet tempTimesheet = AddDuplicatedItemsToTempTimesheet(timesheetId);

            foreach (EmployeeTimesheetItem item in tempTimesheet.TimesheetItems)
            {
                Db.EmployeeTimesheetItems.Add(item);
            }
            Db.SaveChanges();

            ViewBag.MobileMode = IsMobileDevice();
            return View("AddTimesheetItem", tempTimesheet);
        }

        public bool PotentialDuplicateAddItems(int timesheetId)
        {
            EmployeeTimesheet tempTimesheet = AddDuplicatedItemsToTempTimesheet(timesheetId);
            return tempTimesheet.TimesheetItems.Any();
        }

        private EmployeeTimesheet AddDuplicatedItemsToTempTimesheet(int timesheetId)
        {
            EmployeeTimesheet existingTimesheet = Db.EmployeeTimesheets.Find(timesheetId);
            EmployeeTimesheet mostRecentTimesheet = GetMostRecentTimesheetWithItems(existingTimesheet);
            EmployeeTimesheet tempTimesheet = new EmployeeTimesheet
            {
                TimesheetID = timesheetId,
                TimesheetItems = existingTimesheet?.TimesheetItems.ToList()
            };

            // create a new entry
            ApplyTimesheetItemsToTimesheet(tempTimesheet, mostRecentTimesheet);
            SetZeroHoursToNull(tempTimesheet);
            return tempTimesheet;
        }

        private EmployeeTimesheet GetMostRecentTimesheetWithItems(EmployeeTimesheet timesheet)
        {
            EmployeeTimesheet existingTimesheet = Db.EmployeeTimesheets
                .Where(x => x.EmployeeID == timesheet.EmployeeID &&
                            x.TimesheetPeriodID != timesheet.TimesheetPeriodID && x.TimesheetItems.Any(i => !i.ProjectTask.Project.IsClosed && !i.ProjectTask.Project.IsArchived))
                .OrderByDescending(x => x.TimesheetPeriod.EndDate)
                .FirstOrDefault();

            return existingTimesheet;
        }

        private EmployeeTimesheet GetPreviousTimesheet(EmployeeTimesheet timesheet)
        {
            EmployeeTimesheet previous = Db.EmployeeTimesheets
                .Where(x => x.EmployeeID == timesheet.EmployeeID &&
                            x.TimesheetPeriod.EndDate < timesheet.TimesheetPeriod.EndDate)
                .OrderByDescending(x => x.TimesheetPeriod.EndDate)
                .FirstOrDefault();

            return previous;
        }

        private EmployeeTimesheet GetNextTimesheet(EmployeeTimesheet timesheet)
        {
            EmployeeTimesheet previous = Db.EmployeeTimesheets
                .Where(x => x.EmployeeID == timesheet.EmployeeID &&
                            x.TimesheetPeriod.EndDate > timesheet.TimesheetPeriod.EndDate)
                .OrderBy(x => x.TimesheetPeriod.EndDate)
                .FirstOrDefault();

            return previous;
        }

        private EmployeeTimesheetItem CreateNewTimesheetItem(int timesheetId, int variationId, int taskId, TimeCode timeCode, string timeCodeText = null)
        {
            LU_PayType payType = Db.LU_PayTypes.SingleOrDefault(x => x.PayTypeCode == "NT");
            return new EmployeeTimesheetItem
            {
                TimesheetID = timesheetId,
                VariationID = variationId,
                Variation = Db.ProjectVariations.Find(variationId),
                TaskID = taskId,
                ProjectTask = Db.ProjectTasks.Find(taskId),
                PayTypeID = payType?.PayTypeID ?? 0,
                PayType = payType,
                TimeCode = timeCode,
                TimeCodeText = timeCodeText,
                Day1Hrs = 0,
                Day2Hrs = 0,
                Day3Hrs = 0,
                Day4Hrs = 0,
                Day5Hrs = 0,
                Day6Hrs = 0,
                Day7Hrs = 0
            };
        }

        private bool ValidDailyHours(EmployeeTimesheet employeeTimesheet)
        {
            decimal[] hoursTotal = new decimal[DaysInWeek];
            foreach (var item in employeeTimesheet.TimesheetItems)
            {
                hoursTotal[0] += item.Day1Hrs ?? 0;
                hoursTotal[1] += item.Day2Hrs ?? 0;
                hoursTotal[2] += item.Day3Hrs ?? 0;
                hoursTotal[3] += item.Day4Hrs ?? 0;
                hoursTotal[4] += item.Day5Hrs ?? 0;
                hoursTotal[5] += item.Day6Hrs ?? 0;
                hoursTotal[6] += item.Day7Hrs ?? 0;
            }

            for (int i = 0; i < hoursTotal.Length; i++)
            {
                if (hoursTotal[i] > MaximumDailyWorkHours)
                {
                    ViewBag.Message = string.Format("Timesheet has not been saved. The number of hours exceeds the maximum ({0} hours) on {1}.", MaximumDailyWorkHours, EmployeeTimesheetItem.ConvertDayNumberToName(i));
                    return false;
                }
            }
            return true;
        }

        public ActionResult ImportUserTimes()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = UserHelpers.AuthTextAnyAdminRole)]
        public ActionResult ImportUserTimes(HttpPostedFileBase file)
        {
            try
            {
                ValidateExcelFileImportBasic(file);

                List<ImportUserTimes> excelData;

                using (ExcelPackage package = new ExcelPackage(file.InputStream))
                {
                    // extract data from excel file
                    excelData = GetUserTimes(package.Workbook.Worksheets[1]).ToList();
                }

                // insert values into database and store a list of failed import rows
                List<BulkImportStatus> results = ImportUserTimesIntoDb(excelData);
                List<BulkImportStatus> failedResults = results.Where(x => x.Status != ImportStatus.SuccessExistingTimesheet && x.Status != ImportStatus.SuccessNewTimesheet).ToList();

                Guid guid = Guid.NewGuid();
                TempData["DownloadFile"] = guid;
                string folder = Server.MapPath("~/App_Data/Downloads");
                Directory.CreateDirectory(folder);
                string filePath = Path.Combine(folder, guid + ".xlsx");
                ViewBag.DownloadLocation = filePath;

                WriteBulkImportLog(results, filePath);

                if (failedResults.Any())
                {
                    int successful = excelData.Count - failedResults.Count;
                    throw new StatusException(
                        $"{successful} entries successfully added.\r\nErrors/duplicates were encountered in {failedResults.Count} rows and were not imported. Please refer to the import log.", successful > 0 ? InfoMessageType.Warning : InfoMessageType.Failure);
                }

                TempData["InfoMessage"] = new InfoMessage { MessageContent = "Upload complete", MessageType = InfoMessageType.Success };
            }
            catch (StatusException e)
            {
                TempData["InfoMessage"] = new InfoMessage { MessageContent = e.Message, MessageType = e.Status };
            }
            catch (Exception e)
            {
                TempData["InfoMessage"] = new InfoMessage { MessageContent = "No changes saved. " + e.Message, MessageType = InfoMessageType.Failure };
            }
            return RedirectToAction("Index", "Manage");
        }

        private void WriteBulkImportLog(List<BulkImportStatus> results, string filePath)
        {
            using (ExcelPackage packageExport = new ExcelPackage())
            {
                PopulateExcel(packageExport, results);
                packageExport.SaveAs(new FileInfo(filePath));
            }
        }

        private void PopulateExcel(ExcelPackage excel, List<BulkImportStatus> results)
        {
            ExcelWorksheet sheet = excel.Workbook.Worksheets.Add("Results");

            int row = 1;

            sheet.Cells[row, 1].Value = "Excel Row";
            sheet.Cells[row, 2].Value = "User";
            sheet.Cells[row, 3].Value = "Period End";
            sheet.Cells[row, 4].Value = "Task No";
            sheet.Cells[row, 5].Value = "Variation No";
            sheet.Cells[row, 6].Value = "Result";

            foreach (BulkImportStatus result in results)
            {
                sheet.Cells[++row, 1].Value = result.Row;
                sheet.Cells[row, 2].Value = result.User;
                sheet.Cells[row, 3].Value = result.Period?.EndDate.ToString("yyyy-MM-dd");
                sheet.Cells[row, 4].Value = result.Task;
                sheet.Cells[row, 5].Value = result.Variation;
                sheet.Cells[row, 6].Value = result.Status.GetDisplayName();
            }

            sheet.Cells.AutoFitColumns();
        }

        public FileResult DownloadBulkImportLog(Guid id)
        {
            const string xlsxExtension = ".xlsx";
            string path = Server.MapPath($"~/App_Data/Downloads/{id + xlsxExtension}");
            if (!System.IO.File.Exists(path)) return null;
            return File(path, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Import_Log_" + DateTime.UtcNow.ToStringDateAndTimeReverse() + xlsxExtension);
        }

        private List<BulkImportStatus> ImportUserTimesIntoDb(List<ImportUserTimes> userTimes)
        {
            List<BulkImportStatus> rowResults = new List<BulkImportStatus>();

            int sessionProject = (int?)Session["SelectedProject"] ?? 0;

            List<ProjectTask> projectTasks = Db.ProjectTasks.Where(x => x.ProjectID == sessionProject).ToList();
            List<ProjectVariation> projectVariations = Db.ProjectVariations.Where(x => x.ProjectID == sessionProject)
                .ToList();
            List<TimesheetPeriod> periods = Db.TimesheetPeriods.ToList();

            LU_PayType normalPayType = Db.LU_PayTypes.SingleOrDefault(x => x.PayTypeCode == "NT");

            foreach (ImportUserTimes userTime in userTimes)
            {
                TimesheetPeriod matchingPeriod = periods.FirstOrDefault(x => x.EndDate == userTime.EndDate);

                ProjectTask matchingProjectTask =
                    projectTasks.FirstOrDefault(x => x.TaskNo == userTime.ProjectTask.TaskNo);
                ProjectVariation matchingVariation =
                    projectVariations.FirstOrDefault(x => x.VariationNo == userTime.Variation.VariationNo);

                Employee matchingEmployee = Db.Users.FirstOrDefault(x => x.EmployeeNo == userTime.EmployeeNo);

                BulkImportStatus status = new BulkImportStatus { User = userTime.Username, Row = userTime.Row, Period = matchingPeriod, Task = userTime.ProjectTask.TaskNo, Variation = userTime.Variation.VariationNo };

                try
                {
                    VerifyFoundBulkImportValues(matchingProjectTask, matchingVariation, matchingPeriod,
                        matchingEmployee);
                    EmployeeTimesheet timesheet = new EmployeeTimesheet
                    {
                        TimesheetPeriodID = matchingPeriod.TimesheetPeriodID,
                        EmployeeID = matchingEmployee.Id,
                    };

                    EmployeeTimesheetItem item = GenerateTimesheetItem(matchingProjectTask, matchingVariation, userTime, normalPayType, timesheet);
                    status = VerifyAndInsertUserTimes(status, timesheet, matchingPeriod, item);
                }
                catch (BulkImportException e)
                {
                    status.Status = e.Status;
                }
                catch (Exception)
                {
                    status.Status = ImportStatus.FailureUnknown;
                }
                finally
                {
                    rowResults.Add(status);
                }
            }
            Db.SaveChanges();
            return rowResults;
        }

        private static EmployeeTimesheetItem GenerateTimesheetItem(ProjectTask matchingProjectTask, ProjectVariation matchingVariation, ImportUserTimes userTime, LU_PayType normalPayType, EmployeeTimesheet timesheet)
        {
            return new EmployeeTimesheetItem
            {
                TaskID = matchingProjectTask.TaskID,
                VariationID = matchingVariation.VariationID,
                Day1Hrs = userTime.Day1Hours,
                Day2Hrs = userTime.Day2Hours,
                Day3Hrs = userTime.Day3Hours,
                Day4Hrs = userTime.Day4Hours,
                Day5Hrs = userTime.Day5Hours,
                Day6Hrs = userTime.Day6Hours,
                Day7Hrs = userTime.Day7Hours,
                PayType = normalPayType,
                Timesheet = timesheet
            };
        }

        private static void VerifyFoundBulkImportValues(ProjectTask matchingProjectTask, ProjectVariation matchingVariation,
            TimesheetPeriod matchingPeriod, Employee matchingEmployee)
        {
            if (matchingProjectTask == null)
            {
                throw new BulkImportException { Status = ImportStatus.FailureNonexistentTask };
            }
            if (matchingVariation == null)
            {
                throw new BulkImportException { Status = ImportStatus.FailureNonexistentVariation };
            }
            if (matchingPeriod == null)
            {
                throw new BulkImportException { Status = ImportStatus.FailureNonexistentTimesheetPeriod };
            }
            if (matchingEmployee == null)
            {
                throw new BulkImportException { Status = ImportStatus.FailureNonexistentUser };
            }
        }

        private BulkImportStatus VerifyAndInsertUserTimes(BulkImportStatus status, EmployeeTimesheet timesheet, TimesheetPeriod matchingPeriod,
            EmployeeTimesheetItem item)
        {
            bool foundInLocal = true;
            // check for already-added timesheets in context
            EmployeeTimesheet existingTimesheet = Db.EmployeeTimesheets.Local.FirstOrDefault(x => x.EmployeeID == timesheet.EmployeeID && x.TimesheetPeriodID == matchingPeriod.TimesheetPeriodID);

            // otherwise check database again
            if (existingTimesheet == null)
            {
                foundInLocal = false;
                existingTimesheet = Db.EmployeeTimesheets.FirstOrDefault(x => x.EmployeeID == timesheet.EmployeeID && x.TimesheetPeriodID == matchingPeriod.TimesheetPeriodID);
            }

            EmployeeTimesheet timesheetToUse = existingTimesheet;
            if (timesheetToUse == null)
            {
                timesheetToUse = Db.EmployeeTimesheets.Add(timesheet);
                status.Status = ImportStatus.SuccessNewTimesheet;
            }
            else
            {
                status.Status = foundInLocal ? ImportStatus.SuccessNewTimesheet : ImportStatus.SuccessExistingTimesheet;
            }

            EmployeeTimesheetItem existingItem = timesheetToUse.TimesheetItems.FirstOrDefault(x => x.TaskID == item.TaskID && x.VariationID == item.VariationID);

            if (existingItem == null)
            {
                item.Timesheet = timesheetToUse;
                Db.EmployeeTimesheetItems.Add(item);
            }
            else
            {
                if (item.Day1Hrs.GetValueOrDefault() < 0 || item.Day2Hrs.GetValueOrDefault() < 0 || item.Day3Hrs.GetValueOrDefault() < 0 || item.Day4Hrs.GetValueOrDefault() < 0 || item.Day5Hrs.GetValueOrDefault() < 0 || item.Day6Hrs.GetValueOrDefault() < 0 || item.Day7Hrs.GetValueOrDefault() < 0)
                {
                    status.Status = ImportStatus.NegativeValue;
                }
                else if (existingItem.Day1Hrs.GetValueOrDefault() > 0 || existingItem.Day2Hrs.GetValueOrDefault() > 0 || existingItem.Day3Hrs.GetValueOrDefault() > 0 || existingItem.Day4Hrs.GetValueOrDefault() > 0 || existingItem.Day5Hrs.GetValueOrDefault() > 0 || existingItem.Day6Hrs.GetValueOrDefault() > 0 || existingItem.Day7Hrs.GetValueOrDefault() > 0)
                {
                    status.Status = ImportStatus.FailureDuplicateOfExisting;
                }
                else
                {
                    Db.EmployeeTimesheetItems.Remove(existingItem);
                    item.Timesheet = timesheetToUse;
                    Db.EmployeeTimesheetItems.Add(item);
                }
            }
            return status;
        }

        private IEnumerable<ImportUserTimes> GetUserTimes(ExcelWorksheet ws)
        {
            const int excelStartRow = 2;

            const string headingEmployeeNo = "EmployeeNo";
            const string headingUsername = "Username";
            const string headingEndDate = "EndDate";
            const string headingProjectTaskHours = "Project Task";
            const string headingVariationId = "Variation ID";
            const string headingDay1Hrs = "Day 1 (Sat)";
            const string headingDay2Hrs = "Day 2 (Sun)";
            const string headingDay3Hrs = "Day 3 (Mon)";
            const string headingDay4Hrs = "Day 4 (Tue)";
            const string headingDay5Hrs = "Day 5 (Wed)";
            const string headingDay6Hrs = "Day 6 (Thur)";
            const string headingDay7Hrs = "Day 7 (Fri)";

            int? colEmployeeNo = ws.Cells["1:1"].SingleOrDefault(x => x.Text == headingEmployeeNo).FirstOrDefault()?.Start.Column;
            int? colUsername = ws.Cells["1:1"].SingleOrDefault(x => x.Text == headingUsername).FirstOrDefault()?.Start.Column;
            int? colEndDate = ws.Cells["1:1"].SingleOrDefault(x => x.Text == headingEndDate).FirstOrDefault()?.Start.Column;
            int? colProjectTaskHours = ws.Cells["1:1"].SingleOrDefault(x => x.Text == headingProjectTaskHours).FirstOrDefault()?.Start.Column;
            int? colVariationId = ws.Cells["1:1"].SingleOrDefault(x => x.Text == headingVariationId).FirstOrDefault()?.Start.Column;
            int? colDay1Hrs = ws.Cells["1:1"].SingleOrDefault(x => x.Text == headingDay1Hrs).FirstOrDefault()?.Start.Column;
            int? colDay2Hrs = ws.Cells["1:1"].SingleOrDefault(x => x.Text == headingDay2Hrs).FirstOrDefault()?.Start.Column;
            int? colDay3Hrs = ws.Cells["1:1"].SingleOrDefault(x => x.Text == headingDay3Hrs).FirstOrDefault()?.Start.Column;
            int? colDay4Hrs = ws.Cells["1:1"].SingleOrDefault(x => x.Text == headingDay4Hrs).FirstOrDefault()?.Start.Column;
            int? colDay5Hrs = ws.Cells["1:1"].SingleOrDefault(x => x.Text == headingDay5Hrs).FirstOrDefault()?.Start.Column;
            int? colDay6Hrs = ws.Cells["1:1"].SingleOrDefault(x => x.Text == headingDay6Hrs).FirstOrDefault()?.Start.Column;
            int? colDay7Hrs = ws.Cells["1:1"].SingleOrDefault(x => x.Text == headingDay7Hrs).FirstOrDefault()?.Start.Column;

            if (colEmployeeNo == null || colUsername == null || colEndDate == null || colProjectTaskHours == null || colVariationId == null || colDay1Hrs == null || colDay2Hrs == null || colDay3Hrs == null || colDay4Hrs == null || colDay5Hrs == null || colDay6Hrs == null || colDay7Hrs == null)
            {
                throw new Exception("Invalid file - required fields for import not found");
            }

            // loop through rows until an empty cell is detected
            for (int i = excelStartRow; i < int.MaxValue; i++)
            {
                string employeeNo = ws.Cells[i, (int)colEmployeeNo].Text;

                // end of file when a break is detected
                if (string.IsNullOrWhiteSpace(employeeNo)) break;

                yield return new ImportUserTimes
                {
                    Row = i,
                    EmployeeNo = employeeNo,
                    Username = ws.Cells[i, (int)colUsername].Text,
                    EndDate = DateTime.Parse(ws.Cells[i, (int)colEndDate].Text),
                    ProjectTask = new ProjectTask { TaskNo = ws.Cells[i, (int)colProjectTaskHours].Text },
                    Variation = new ProjectVariation { VariationNo = ws.Cells[i, (int)colVariationId].Text },
                    Day1Hours = ParseTimesheetImportHours(ws, i, colDay1Hrs),
                    Day2Hours = ParseTimesheetImportHours(ws, i, colDay2Hrs),
                    Day3Hours = ParseTimesheetImportHours(ws, i, colDay3Hrs),
                    Day4Hours = ParseTimesheetImportHours(ws, i, colDay4Hrs),
                    Day5Hours = ParseTimesheetImportHours(ws, i, colDay5Hrs),
                    Day6Hours = ParseTimesheetImportHours(ws, i, colDay6Hrs),
                    Day7Hours = ParseTimesheetImportHours(ws, i, colDay7Hrs)
                };
            }
        }

        [HttpPost]
        public JsonResult GetTaskDescription(int? projectTaskId)
        {
            if (!projectTaskId.HasValue) return Json(null);
            ProjectTask task = Db.ProjectTasks.Find(projectTaskId);
            bool displayForProject = task != null && task.Project.DisplayTaskNotes;
            return displayForProject ? Json(Db.ProjectTasks.Find(projectTaskId)?.Notes) : Json(null);
        }

        private static decimal ParseTimesheetImportHours(ExcelWorksheet ws, int i, int? colDay1Hrs)
        {
            decimal parsed = 0;
            if (colDay1Hrs != null) decimal.TryParse(ws.Cells[i, (int)colDay1Hrs].Text, out parsed);
            return parsed;
        }

        [HttpPost]
        public JsonResult GetUserTypeDescription(int? employeeId, int? projectId)
        {
            ProjectUserType projectUserType = Db.EmployeeProjects.SingleOrDefault(x => x.EmployeeId == employeeId && x.ProjectId == projectId)?.ProjectUserType;
            if (projectUserType == null)
            {
                return Json(false);
            }

            string usedDescription = projectUserType.AliasDescription ?? projectUserType.UserType.Description;

            return usedDescription == null ? Json(false) : Json(usedDescription);
        }
    }

    public class EmployeeProjectUserTypeInfo
    {
        public int ProjectID { get; set; }
        public float? MaxNTHours { get; set; }
        public float? MaxOT1Hours { get; set; }
        public float? MaxOT2Hours { get; set; }
        public float? MaxOT3Hours { get; set; }
        public bool MandatoryComments { get; set; }
        public string ProjectName { get; set; }
        //Added new Time Codes
        public float? MaxOT4Hours { get; set; }
        public float? MaxOT5Hours { get; set; }
        public float? MaxOT6Hours { get; set; }
        public float? MaxOT7Hours { get; set; }
    }
}