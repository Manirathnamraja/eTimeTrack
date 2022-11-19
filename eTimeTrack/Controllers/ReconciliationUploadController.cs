using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using OfficeOpenXml;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
    public class ReconciliationUploadController : BaseController
    {
        public ActionResult ImportReconciliationFile()
        {
            ReconciliationFileViewModel viewModel = new ReconciliationFileViewModel { TemplateList = GenerateDropdownReconciliationTemplates(), ProjectList = GenerateDropdownUserProjects() };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult ImportReconciliationFile(ReconciliationFileViewModel model)
        {
            DateTime checkTime = DateTime.UtcNow.AddMinutes(-3);
            bool existingRunningUpload = Db.ReconciliationUploads.Any(x => x.ProjectId == model.ProjectID && x.UploadDateTimeUtc > checkTime);

            if (existingRunningUpload)
            {
                InfoMessage message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = "Please wait 3 minutes between uploads for a project" };
                ViewBag.InfoMessage = message;
                model.TemplateList = GenerateDropdownReconciliationTemplates();
                model.ProjectList = GenerateDropdownUserProjects();
                return View(model);
            }

            Project project;
            ReconciliationTemplate template;

            try
            {
                ValidateExcelFileImportBasic(model.File);

                project = Db.Projects.Find(model.ProjectID);
                if (project == null)
                {
                    {
                        return InvokeHttp400(HttpContext);
                    }
                }

                template = Db.ReconciliationTemplates.Include(x => x.Company).Single(x => x.Id == model.ReconciliationTemplateId);
                if (template == null)
                {
                    {
                        return InvokeHttp400(HttpContext);
                    }
                }
            }

            catch (Exception e)
            {
                TempData["InfoMessage"] = new InfoMessage { MessageContent = "Error: could not begin import of reconciliation data: " + e.Message, MessageType = InfoMessageType.Failure };
                return RedirectToAction("Index", "Manage");
            }

            byte[] data;
            using (MemoryStream target = new MemoryStream())
            {
                model.File.InputStream.CopyTo(target);
                data = target.ToArray();
            }

            Employee user = UserHelpers.GetCurrentUser();
            HostingEnvironment.QueueBackgroundWorkItem(ct => ProcessReconciliationFile(model, project, template, data, user.Id, user.Email));

            TempData["InfoMessage"] = new InfoMessage { MessageContent = "The upload process will run in the background. You will receive an email notification when complete.", MessageType = InfoMessageType.Success };
            return RedirectToAction("Index", "Manage");
        }

        private bool ProcessReconciliationFile(ReconciliationFileViewModel model, Project project, ReconciliationTemplate template, byte[] fileData, int userId, string email)
        {
            int invalidRowsEmpty = 0;
            int unrequiredRows = 0;
            int invalidRowsNoLink = 0;
            int insertedRows = 0;
            int updatedRows = 0;
            List<List<int>> invalidRowsNoLinkRowNumbers = new List<List<int>>();

            ApplicationDbContext context = new ApplicationDbContext();

            try
            {
                //
                //get raw data from the spreadsheet
                //
                bool identifierSpecified = !string.IsNullOrWhiteSpace(template.TypeIdentifierColumn);
                List<string> identifierStrings = new List<string>();

                if (identifierSpecified && !string.IsNullOrWhiteSpace(template.TypeIdentifierText))
                {
                    identifierStrings = template.TypeIdentifierText.Split(';').Select(x => x?.Trim().ToLower()).ToList();
                }

                int hoursColumnNumber = ColumnNumber(template.HoursColumn);
                int? identifierColumnNumber = !identifierSpecified ? (int?)null : ColumnNumber(template.TypeIdentifierColumn);
                int employeeNumberColumn = ColumnNumber(template.EmployeeNumberColumn);
                int weekEndingColumn = ColumnNumber(template.WeekEndingColumn);

                List<TemporaryReconciliationEntry> tempEntries = new List<TemporaryReconciliationEntry>();

                using (MemoryStream stream = new MemoryStream())
                {
                    stream.Write(fileData, 0, fileData.Length);
                    using (ExcelPackage package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets[1];


                        for (int i = 2; i < int.MaxValue; i++) // skip header row
                        {
                            if (string.IsNullOrWhiteSpace(ws.Cells[i, 1].Text))
                            {
                                if (!string.IsNullOrWhiteSpace(ws.Cells[i + 1, 1].Text) || !string.IsNullOrWhiteSpace(ws.Cells[i + 2, 1].Text))
                                {
                                    continue;
                                }
                                break;
                            }

                            TemporaryReconciliationEntry tempEntry = new TemporaryReconciliationEntry
                            {
                                EmployeeNumber = ws.Cells[i, employeeNumberColumn].Text?.Trim(),
                                EndDate = ws.Cells[i, weekEndingColumn].Text?.Trim(),
                                HoursString = ws.Cells[i, hoursColumnNumber].Value?.ToString()?.Trim(),
                                Identifier =
                                    identifierColumnNumber.HasValue
                                        ? ws.Cells[i, (int)identifierColumnNumber].Text?.Trim()
                                        : null,
                                ImportRowNumbers = new List<int> { i }
                            };

                            if (string.IsNullOrWhiteSpace(tempEntry.EmployeeNumber) || string.IsNullOrWhiteSpace(tempEntry.EndDate) || string.IsNullOrWhiteSpace(tempEntry.HoursString))
                            {
                                invalidRowsEmpty++;
                            }
                            else if (identifierSpecified && !identifierStrings.Contains(tempEntry.Identifier?.ToLower()))
                            {
                                unrequiredRows++;
                            }
                            else
                            {
                                tempEntries.Add(tempEntry);
                            }
                        }
                    }
                }

                //
                //check and parse data
                //
                //convert weekendings and employee numbers to foreign keys
                List<TimesheetPeriod> periods = context.TimesheetPeriods.ToList();

                // changed to use all employees 13/04/2022 to allow for users that have been removed from the project
                //Dictionary<string, int> employees = context.Users.Where(x => x.Projects.Any(y => y.ProjectId == project.ProjectID)).ToDictionary(x => x.EmployeeNo.Trim(), x => x.Id);

                Dictionary<string, int> employees = context.Users.ToDictionary(x => x.EmployeeNo.Trim(), x => x.Id);

                foreach (TemporaryReconciliationEntry entry in tempEntries)
                {
                    int empId;

                    if (entry.EmployeeNumber == "AUR111764")
                    {
                        int xxx = 1;
                    }

                    if (employees.TryGetValue(entry.EmployeeNumber, out empId))
                    {
                        entry.EmployeeId = empId;
                    }
                    else
                    {
                        invalidRowsNoLinkRowNumbers.Add(entry.ImportRowNumbers.ToList());
                        invalidRowsNoLink++;
                        continue;
                    }

                    DateTime dt;
                    if (DateTime.TryParse(entry.EndDate, out dt))
                    {
                        TimesheetPeriod period = periods.SingleOrDefault(x => x.EndDate.Year == dt.Year && x.EndDate.Month == dt.Month && x.EndDate.Day == dt.Day);

                        if (period == null && template.DailyDates)
                        {
                            period = periods.SingleOrDefault(x => dt <= x.EndDate && dt >= x.StartDate);
                        }

                        if (period == null)
                        {
                            entry.TimesheetPeriodId = null;
                        }
                        else
                        {
                            entry.TimesheetPeriodId = period.TimesheetPeriodID;
                        }

                        entry.TimesheetPeriodEndDate = dt;
                    }
                    else
                    {
                        invalidRowsNoLinkRowNumbers.Add(entry.ImportRowNumbers.ToList());
                        invalidRowsNoLink++;
                        continue;
                    }

                    decimal hours;
                    if (decimal.TryParse(entry.HoursString, out hours))
                    {
                        entry.Hours = hours;
                    }
                    else
                    {
                        invalidRowsNoLinkRowNumbers.Add(entry.ImportRowNumbers.ToList());
                        invalidRowsNoLink++;
                    }
                }

                ReconciliationUpload upload = new ReconciliationUpload { ReconciliationTemplateId = template.Id, Filename = model.File.FileName, UploadDateTimeUtc = DateTime.UtcNow, ProjectId = project.ProjectID };
                context.ReconciliationUploads.Add(upload);
                context.SaveChangesWithChangelog(userId);

                tempEntries = tempEntries.Where(x => x.EmployeeId.HasValue && x.TimesheetPeriodId.HasValue && x.Hours.HasValue).GroupBy(x => new { x.EmployeeId, x.TimesheetPeriodId }).Select(x => new TemporaryReconciliationEntry { EmployeeId = x.Key.EmployeeId, TimesheetPeriodId = x.Key.TimesheetPeriodId, Hours = x.Sum(y => y.Hours), ImportRowNumbers = x.SelectMany(y => y.ImportRowNumbers) }).ToList();

                //
                //update the database
                //
                List<ReconciliationEntry> existingEntries = context.ReconciliationEntries.Include(x => x.CurrentReconciliationUpload.ReconciliationTemplate).Where(x => x.OriginalReconciliationUpload.ReconciliationTemplateId == template.Id && !x.Deleted && x.OriginalReconciliationUpload.ProjectId == project.ProjectID).ToList();

                List<int?> allEmployeeIds = tempEntries.Select(x => x.EmployeeId).Union(context.Users.Where(x => x.CompanyID == template.CompanyId).Select(x => (int?)x.Id)).ToList();

                List<EmployeeTimesheetInPeriod> allRequiredTimesheets = context.EmployeeTimesheets.Where(x => allEmployeeIds.Contains(x.EmployeeID)).Select(x => new EmployeeTimesheetInPeriod { EmployeeId = x.EmployeeID, TimesheetPeriodId = x.TimesheetPeriodID, TimesheetId = x.TimesheetID }).ToList();

                List<EmployeeTimesheetItem> allTimesheetItemsForProject = context.EmployeeTimesheetItems.Where(x => allEmployeeIds.Contains(x.Timesheet.EmployeeID) && x.ProjectTask.ProjectID == project.ProjectID).Include(x => x.Timesheet).ToList();

                int count = 0;

                List<ReconciliationType> allReconciliationTypes = context.ReconciliationTypes.ToList();
                ReconciliationType reconciliationTypeeTimeTrackMissing = allReconciliationTypes.Single(x => x.Text == "eTimeTrack timesheet missing");
                ReconciliationType reconciliationTypeHomeOfficeMissing = allReconciliationTypes.Single(x => x.Text == "Home office timesheet missing");
                ReconciliationType reconciliationTypeRequiresReview = allReconciliationTypes.Single(x => x.Text == "Requires review");

                List<ReconciliationEntry> newEntriesFromHomeOfficeEntries = new List<ReconciliationEntry>();

                // add in missing home office entries into entriesTemp
                var groupedTimesheetItems = allTimesheetItemsForProject.GroupBy(x => new { x.Timesheet.EmployeeID, x.Timesheet.TimesheetPeriodID });

                foreach (var groupedTimesheetItemForEmployeeAndPeriod in groupedTimesheetItems)
                {
                    int employeeId = groupedTimesheetItemForEmployeeAndPeriod.Key.EmployeeID;
                    int timesheetPeriodId = groupedTimesheetItemForEmployeeAndPeriod.Key.TimesheetPeriodID;
                    decimal hours = groupedTimesheetItemForEmployeeAndPeriod.Sum(x => x.TotalHours());

                    if (employeeId == 20745)
                    {
                        int xxxxx = 1;
                    }

                    List<TemporaryReconciliationEntry> foundEntryInImport = tempEntries.Where(x => x.EmployeeId == employeeId && x.TimesheetPeriodId == timesheetPeriodId).ToList();

                    if (!foundEntryInImport.Any())
                    {
                        tempEntries.Add(new TemporaryReconciliationEntry
                        {
                            EmployeeId = employeeId,
                            TimesheetPeriodId = timesheetPeriodId,
                            TimesheetPeriodEndDate = periods.Single(x => x.TimesheetPeriodID == timesheetPeriodId).EndDate,
                            Hours = null,
                            HourseTimeTrack = hours,
                            EndDate = "<ignored>",
                            Identifier = "<ignored>",
                            EmployeeNumber = "<ignored>",
                            HoursString = "<ignored>"
                        });
                    }
                }
                // end: add in missing home office entries into entriesTemp

                foreach (TemporaryReconciliationEntry tempEntry in tempEntries)
                {
                    List<ReconciliationEntry> reconciliationEntries = existingEntries.Where(x => x.EmployeeId == tempEntry.EmployeeId && x.TimesheetPeriodId == tempEntry.TimesheetPeriodId).ToList();

                    ReconciliationEntry reconciliationEntry;

                    if (tempEntry.EmployeeId == 20745)
                    {
                        int xyxyx = 1;
                    }

                    if (reconciliationEntries.Count > 1)
                    {
                        reconciliationEntry = reconciliationEntries.FirstOrDefault(x => x.ReconciliationTypeId.HasValue) ?? reconciliationEntries.FirstOrDefault();

                        foreach (ReconciliationEntry entry in reconciliationEntries)
                        {
                            if (entry.Id != reconciliationEntry.Id)
                            {
                                entry.Deleted = true;
                            }
                        }
                    }
                    else
                    {
                        reconciliationEntry = reconciliationEntries.FirstOrDefault();
                    }

                    if (reconciliationEntry != null)
                    {
                        reconciliationEntry.CurrentReconciliationUploadId = upload.Id;
                        if (reconciliationEntry.Hours != tempEntry.Hours)
                            reconciliationEntry.Hours = tempEntry.Hours;

                        if (reconciliationEntry.EmployeeTimesheetId == null)
                        {
                            int? timesheetId = allRequiredTimesheets.FirstOrDefault(x => x.TimesheetPeriodId == tempEntry.TimesheetPeriodId && x.EmployeeId == tempEntry.EmployeeId)?.TimesheetId;

                            if (timesheetId.HasValue)
                                reconciliationEntry.EmployeeTimesheetId = timesheetId;
                        }

                        updatedRows++;
                    }
                    else
                    {
                        int? timesheetId = allRequiredTimesheets.FirstOrDefault(x => x.TimesheetPeriodId == tempEntry.TimesheetPeriodId && x.EmployeeId == tempEntry.EmployeeId)?.TimesheetId;

                        reconciliationEntry = new ReconciliationEntry { Hours = tempEntry.Hours, EmployeeId = tempEntry.EmployeeId.Value, CurrentReconciliationUploadId = upload.Id, OriginalReconciliationUploadId = upload.Id, TimesheetPeriodId = tempEntry.TimesheetPeriodId.Value, EmployeeTimesheetId = timesheetId };
                        newEntriesFromHomeOfficeEntries.Add(reconciliationEntry);
                        insertedRows++;
                    }

                    List<EmployeeTimesheetItem> timesheetItems = allTimesheetItemsForProject.Where(x => x.Timesheet.TimesheetPeriodID == tempEntry.TimesheetPeriodId && x.Timesheet.EmployeeID == tempEntry.EmployeeId).ToList();

                    decimal totalHoursForProjectFromExistingTimesheetItems = timesheetItems.Sum(x => x.TotalHours());

                    decimal eTimeTrackHours = tempEntry.HourseTimeTrack ?? totalHoursForProjectFromExistingTimesheetItems;

                    reconciliationEntry.HoursEqual = eTimeTrackHours == (reconciliationEntry.Hours ?? 0);
                    if (reconciliationEntry.HoursEqual)
                    {
                        reconciliationEntry.ReconciliationType = null;
                        reconciliationEntry.Status = null;
                        reconciliationEntry.ReconciliationComment = null;
                        reconciliationEntry.EmployeeComment = null;
                    }
                    else
                    {
                        if (!reconciliationEntry.ReconciliationTypeId.HasValue)
                        {
                            if (eTimeTrackHours > 0)
                            {
                                reconciliationEntry.ReconciliationType = (!reconciliationEntry.Hours.HasValue || reconciliationEntry.Hours == 0) ? reconciliationTypeHomeOfficeMissing : reconciliationTypeRequiresReview;
                            }
                            else
                            {
                                reconciliationEntry.ReconciliationType = (!reconciliationEntry.Hours.HasValue || reconciliationEntry.Hours > 0) ? reconciliationTypeeTimeTrackMissing : reconciliationTypeRequiresReview;
                            }

                            if (reconciliationEntry.Status == ReconciliationDiscrepencyStatus.Actioned)
                                reconciliationEntry.Status = ReconciliationDiscrepencyStatus.PostActionChangeToBeActioned;
                            else
                                reconciliationEntry.Status = ReconciliationDiscrepencyStatus.ToBeActioned;
                        }
                    }

                    count++;
                }

                context.ReconciliationEntries.AddRange(newEntriesFromHomeOfficeEntries);

                // Delete old entries that are no longer valid
                DateTime? importTimesheetPeriodStart = tempEntries.Min(x => x.TimesheetPeriodEndDate);
                DateTime? importTimesheetPeriodEnd = tempEntries.Max(x => x.TimesheetPeriodEndDate);

                bool importFileHasValidPeriods = importTimesheetPeriodStart.HasValue && importTimesheetPeriodEnd.HasValue;

                if (importFileHasValidPeriods)
                {
                    List<ReconciliationEntry> existingEntriesInImportDateRange = existingEntries.Where(x => x.TimesheetPeriod.EndDate >= importTimesheetPeriodStart && x.TimesheetPeriod.EndDate <= importTimesheetPeriodEnd && x.CurrentReconciliationUpload.ReconciliationTemplate.CompanyId == template.CompanyId).ToList();
                    foreach (ReconciliationEntry existingEntry in existingEntriesInImportDateRange)
                    {
                        TemporaryReconciliationEntry foundTempEntry = tempEntries.SingleOrDefault(x => x.TimesheetPeriodId == existingEntry.TimesheetPeriodId && existingEntry.CurrentReconciliationUpload.ReconciliationTemplate.CompanyId == template.CompanyId && x.EmployeeId == existingEntry.EmployeeId);

                        if (foundTempEntry == null && !existingEntry.Deleted)
                        {
                            existingEntry.Deleted = true;
                        }
                    }
                }

                context.SaveChangesWithChangelog(userId);
            }
            catch (Exception e)
            {
                EmailHelper.SendEmail(email, $"eTimeTrack reconciliation import failed for {project.Name}", "Error: could not import reconciliation data: " + e.Message + ". Please contact an administrator for assistance.");
                TempData["InfoMessage"] = new InfoMessage { MessageContent = "Error: could not import reconciliation data: " + e.Message, MessageType = InfoMessageType.Failure };
                return false;
            }

            string emailText = $"<p>{template.Company.Company_Name} Upload complete using template: {template.Name}. Added:</p><ul><li>Unrequired Rows (no data): {unrequiredRows}</li><li>Invalid Rows (no data): {invalidRowsEmpty}</li><li>Invalid Rows (no match): {invalidRowsNoLink}</li><li>New Rows: {insertedRows}</li><li>Updated Rows: {updatedRows}</li></ul>";


            invalidRowsNoLinkRowNumbers = invalidRowsNoLinkRowNumbers.Where(x => x != null && x.Any()).ToList();

            if (invalidRowsNoLinkRowNumbers.Any())
            {
                emailText += "<p>These rows are Invalid (no match)</p>";
                emailText += "<ul>";
                foreach (List<int> invalidRowsNoLinkRowNumber in invalidRowsNoLinkRowNumbers)
                {
                    emailText += $"<li>{string.Join(", ", invalidRowsNoLinkRowNumber)}</li>";
                }
                emailText += "</ul>";
            }

            EmailHelper.SendEmail(email, $"eTimeTrack reconciliation import succeeded for {project.Name}", emailText);
            return true;
        }

        public static int ColumnNumber(string colAddress)
        {
            string colAddressUpper = colAddress.ToUpper();
            int[] digits = new int[colAddressUpper.Length];
            for (int i = 0; i < colAddressUpper.Length; ++i)
            {
                digits[i] = Convert.ToInt32(colAddressUpper[i]) - 64;
            }
            int mul = 1; int res = 0;
            for (int pos = digits.Length - 1; pos >= 0; --pos)
            {
                res += digits[pos] * mul;
                mul *= 26;
            }
            return res;
        }

        private class TemporaryReconciliationEntry
        {
            public string EmployeeNumber { get; set; }
            public int? EmployeeId { get; set; }
            public string HoursString { get; set; }
            public decimal? Hours { get; set; }
            public string EndDate { get; set; }
            public string Identifier { get; set; }
            public int? TimesheetPeriodId { get; set; }
            public DateTime? TimesheetPeriodEndDate { get; set; }
            public decimal? HourseTimeTrack { get; set; }
            public IEnumerable<int> ImportRowNumbers { get; set; }
        }
    }

    public class EmployeeTimesheetInPeriod
    {
        public int EmployeeId { get; set; }
        public int TimesheetPeriodId { get; set; }
        public int TimesheetId { get; set; }
    }
}