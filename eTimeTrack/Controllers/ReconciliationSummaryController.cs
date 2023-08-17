using eTimeTrack.Extensions;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
    public class ReconciliationSummaryController : BaseController
    {
        public ActionResult Index()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            List<ReconciliationSummaryIndexViewModel> vm = Db.ReconciliationEntries.Where(x => !x.Deleted && x.CurrentReconciliationUpload.ProjectId == projectId).Select(x => new ReconciliationSummaryIndexViewModel
                {
                    TimesheetPeriod = x.TimesheetPeriod
                }).Distinct().OrderByDescending(x => x.TimesheetPeriod.StartDate).ToList();
            return View(vm);
        }

        public ActionResult Details(int timesheetPeriodId, int? companyId)
        {
            ReconciliationSummaryDetailsViewModel vm = GetSummaryViewModel(timesheetPeriodId, companyId);

            List<SelectListItem> companies = GetCompanyDropdown();
            ViewBag.Companies = companies;

            return View(vm);
        }

        private ReconciliationSummaryDetailsViewModel GetSummaryViewModel(int timesheetPeriodId, int? companyId)
        {
            int projectId = (int?) Session["SelectedProject"] ?? 0;

            TimesheetPeriod timesheetPeriod = Db.TimesheetPeriods.Single(x => x.TimesheetPeriodID == timesheetPeriodId);

            var entriesQuery = Db.ReconciliationEntries.Where(x => !x.Deleted &&
                x.TimesheetPeriod.EndDate <= timesheetPeriod.EndDate &&
                x.CurrentReconciliationUpload.ProjectId == projectId);

            if (companyId.HasValue)
                entriesQuery = entriesQuery.Where(x => x.OriginalReconciliationUpload.ReconciliationTemplate.CompanyId == companyId);

            List<ReconciliationEntry> entriesAll = entriesQuery.ToList();
            
            List<EmployeeTimesheetItem> timesheetItemsAll = entriesAll.Where(x => x.EmployeeTimesheet != null).SelectMany(x => x.EmployeeTimesheet.TimesheetItems.Where(y => y.ProjectTask.ProjectID == projectId)).ToList();

            IEnumerable<IGrouping<ReconciliationType, ReconciliationEntry>> typeGrouped = entriesAll.GroupBy(x => x.ReconciliationType);

            ReconciliationSummaryDetailsViewModel vm = new ReconciliationSummaryDetailsViewModel
            {
                TimesheetPeriod = timesheetPeriod,
                TotalOtherHours = entriesAll.Sum(x => x.Hours),
                TotalEttHours = timesheetItemsAll.Sum(x => x.TotalHours()),
                TotalEmployees = entriesAll.Count,
                ProjectName = entriesAll.FirstOrDefault()?.CurrentReconciliationUpload.Project.DisplayName,
                CompanyName = entriesAll.FirstOrDefault()?.CurrentReconciliationUpload.ReconciliationTemplate.Company.Company_Name,
                CompanyId = companyId,
                ReconciliationHours = typeGrouped.Select(x => new ReconciliationTypeHourSummary
                {
                    ReconciliationType = x.Key,
                    OtherHours = x.Sum(y => y.Hours),
                    EttHours = x.Where(y => y.EmployeeTimesheet != null)
                        .SelectMany(y => y.EmployeeTimesheet.TimesheetItems.Where(z => z.ProjectTask.ProjectID == projectId))
                        .Sum(i => i.TotalHours()),
                    Employees = x.Count()
                }).ToList()
            };

            if (!companyId.HasValue)
                vm.CompanyName = "";

            return vm;
        }

        private List<SelectListItem> GetCompanyDropdown()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            List<SelectListItem> selectItems = Db.ProjectCompanies.Where(x => x.ProjectId == projectId).Select(x => new SelectListItem { Value = x.CompanyId.ToString(), Text = x.Company.Company_Name }).ToList();
            return selectItems;
        }

        [HttpPost]
        public FileResult PrintFriendlyExcelData(int timesheetPeriodId, int? companyId)
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;

            TimesheetPeriod endPeriod = Db.TimesheetPeriods.Single(x => x.TimesheetPeriodID == timesheetPeriodId);

            IQueryable<ReconciliationEntry> query = Db.ReconciliationEntries.Where(x => x.TimesheetPeriod.EndDate <= endPeriod.EndDate &&
                !x.Deleted && x.OriginalReconciliationUpload.ProjectId == projectId);

            var assignproject = Db.Users.Where(x => Db.EmployeeProjects.Any(y => y.EmployeeId == x.Id && y.ProjectId == projectId)).ToList();
            var assignToProject = assignproject.Count > 0 ? "Y" : "N";

            if (companyId.HasValue)
                query = query.Where(x => x.OriginalReconciliationUpload.ReconciliationTemplate.CompanyId == companyId);

            List<ReconciliationEntry> allData = query.OrderByDescending(x => x.TimesheetPeriod.EndDate).ToList();

            FileInfo filePath = GetGuidFilePath("xlsx");

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorkbook workbook = package.Workbook;
                ExcelWorksheet ws = workbook.Worksheets.Add("Reconciliation");

                int row = 1;
                int col = 1;

                ws.Cells[row, col++].Value = "Company";
                ws.Cells[row, col++].Value = "Employee Number";
                ws.Cells[row, col++].Value = "Employee Names";
                ws.Cells[row, col++].Value = "Timesheet Period Start";
                ws.Cells[row, col++].Value = "Timesheet Period End";
                ws.Cells[row, col++].Value = "Home Office Hours";
                ws.Cells[row, col++].Value = "eTimeTrack Hours";
                ws.Cells[row, col++].Value = "Reconciliation Type";
                ws.Cells[row, col++].Value = "Complete";
                ws.Cells[row, col++].Value = "Comments";
                ws.Cells[row, col++].Value = "Employee Comments";
                ws.Cells[row, col++].Value = "Email Address";
                ws.Cells[row, col++].Value = "Assign to Project";
                ws.Cells[row, 1, row, col].Style.Font.Bold = true;
                ws.Cells[row, 1, row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;

                row++;

                foreach (ReconciliationEntry reconEntry in allData)
                {
                    col = 1;
                    ws.Cells[row, col++].Value = reconEntry.OriginalReconciliationUpload.ReconciliationTemplate.Company.Company_Name;
                    ws.Cells[row, col++].Value = reconEntry.Employee.EmployeeNo;
                    ws.Cells[row, col++].Value = reconEntry.Employee.Names;
                    ws.Cells[row, col++].Value = reconEntry.TimesheetPeriod.StartDate.ToString("yyyy-MM-dd HH:mm:ss");
                    ws.Cells[row, col++].Value = reconEntry.TimesheetPeriod.EndDate.ToString("yyyy-MM-dd HH:mm:ss");
                    ws.Cells[row, col].Value = reconEntry.Hours;
                    ws.Cells[row, col++].Style.Numberformat.Format = "0.00";
                    ws.Cells[row, col].Value = reconEntry.EmployeeTimesheet?.TimesheetItems.Where(z => z.ProjectTask.ProjectID == projectId).Sum(i => i.TotalHours()) ?? 0.0m;
                    ws.Cells[row, col++].Style.Numberformat.Format = "0.00";
                    ws.Cells[row, col++].Value = reconEntry.ReconciliationType?.Text;
                    ws.Cells[row, col++].Value = reconEntry.Complete;
                    ws.Cells[row, col++].Value = reconEntry.ReconciliationComment;
                    ws.Cells[row, col++].Value = reconEntry.EmployeeComment;
                    ws.Cells[row, col++].Value = reconEntry.Employee.Email;
                    ws.Cells[row, col++].Value = assignToProject;

                    row++;
                }

                for (int i = 1; i < 11; i++)
                    ws.Column(i).AutoFit();

                package.SaveAs(filePath);
            }

            byte[] bytes = System.IO.File.ReadAllBytes(filePath.FullName);

            string filename = $"ReconciliationSummary_{endPeriod.EndDate:yyyy-MM-dd_HHmmss}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
        }

        [HttpPost]
        public FileResult PrintFriendly(int timesheetPeriodId, int? companyId)
        {
            ReconciliationSummaryDetailsViewModel vm = GetSummaryViewModel(timesheetPeriodId, companyId);

            FileInfo filePath = GetGuidFilePath("xlsx");

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorkbook workbook = package.Workbook;
                ExcelWorksheet ws = workbook.Worksheets.Add("Reconciliation");

                int row = 1;
                int col = 1;

                ws.Cells[row, col].Value = "Project";
                ws.Cells[row, col++].Style.Font.Bold = true;
                ws.Cells[row, col].Value = vm.ProjectName;
                ws.Cells[row, col, row, col + 2].Merge = true;

                col = 1;
                row++;

                ws.Cells[row, col].Value = "Week Ending";
                ws.Cells[row, col++].Style.Font.Bold = true;
                ws.Cells[row, col].Value = vm.TimesheetPeriod.GetStartEndDates();
                ws.Cells[row, col, row, col + 2].Merge = true;

                col = 1;
                row++;

                ws.Cells[row, col].Value = "Company";
                ws.Cells[row, col++].Style.Font.Bold = true;
                ws.Cells[row, col].Value = vm.CompanyName;
                ws.Cells[row, col, row, col + 2].Merge = true;

                col = 2;
                row++;
                row++;

                ws.Cells[row, col++].Value = "eTimeTrack Hours";
                ws.Cells[row, col++].Value = "Home Office Hours";
                ws.Cells[row, col].Value = "Employees Count";
                ws.Cells[row, 1, row, col].Style.Font.Bold = true;
                ws.Cells[row, 1, row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[row, 1, row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;

                row++;
                int startVerticalBold = row;
                bool first = true;
                foreach (ReconciliationTypeHourSummary reconHours in vm.ReconciliationHours)
                {
                    col = 1;
                    ws.Cells[row, col++].Value = reconHours.ReconciliationType?.Text ?? "Reconciled";
                    ws.Cells[row, col].Value = reconHours.EttHours;
                    ws.Cells[row, col++].Style.Numberformat.Format = "0.00";
                    ws.Cells[row, col].Value = reconHours.OtherHours;
                    ws.Cells[row, col++].Style.Numberformat.Format = "0.00";
                    ws.Cells[row, col].Value = reconHours.Employees;
                    if (first)
                        first = false;
                    else
                        ws.Cells[row, 1, row, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                    row++;
                }

                col = 1;
                ws.Cells[row, col++].Value = "Totals";
                ws.Cells[row, col].Value = vm.TotalEttHours;
                ws.Cells[row, col++].Style.Numberformat.Format = "0.00";
                ws.Cells[row, col].Value = vm.TotalOtherHours;
                ws.Cells[row, col++].Style.Numberformat.Format = "0.00";
                ws.Cells[row, col].Value = vm.TotalEmployees;
                ws.Cells[row, 1, row, 4].Style.Border.Top.Style = ExcelBorderStyle.Thick;

                row++;
                col = 1;
                ws.Cells[row, col++].Value = "Difference";
                ws.Cells[row, col].Value = vm.TotalEttHours - vm.TotalOtherHours;
                ws.Cells[row, col].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 1, row, 4].Style.Border.Top.Style = ExcelBorderStyle.Medium;

                ws.Cells[startVerticalBold, 1, row, 1].Style.Font.Bold = true;

                for (int i = 1; i < 5; i++)
                    ws.Column(i).AutoFit();

                // write time
                ws.HeaderFooter.differentFirst = false;
                ws.HeaderFooter.differentOddEven = false;

                package.SaveAs(filePath);
            }

            FileInfo pdfFilePath = WritePdfExcel(filePath);
            byte[] bytes = System.IO.File.ReadAllBytes(pdfFilePath.FullName);

            // delete local files
            filePath.Delete();
            pdfFilePath.Delete();

            string filename = $"ReconciliationSummary_{vm.TimesheetPeriod.EndDate:yyyy-MM-dd_HHmmss}.pdf";
            return File(bytes, "application/pdf", filename);
        }
    }
}