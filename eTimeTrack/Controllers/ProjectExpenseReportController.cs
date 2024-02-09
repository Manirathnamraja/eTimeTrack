using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eTimeTrack.Extensions;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using static eTimeTrack.Controllers.WeeklyCostReportsController;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class ProjectExpenseReportController : BaseController
    {
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult Index()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            ExportRatesIndexUserVm vm = new ExportRatesIndexUserVm { ProjectList = GenerateDropdownUserProjects() };

            return View(vm);
        }

        [HttpPost]
        public FileResult PrintFriendlyExcelData(int projectId)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {

                List<ProjectExpenseReportViewModel> allData = Db.Database.SqlQuery<ProjectExpenseReportViewModel>("EXEC GetProjectExpenseUploads @ProjectId",
                   new SqlParameter("ProjectId", projectId)).ToList();


                var projectName = "";

                FileInfo filePath = GetGuidFilePath("xlsx");

                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorkbook workbook = package.Workbook;
                    ExcelWorksheet ws = workbook.Worksheets.Add("ProjectExpenseReport");

                    int row = 1;
                    int col = 1;

                    ws.Cells[row, col++].Value = "TransactionID";
                    ws.Cells[row, col++].Value = "ExpenseDate";
                    ws.Cells[row, col++].Value = "CostedInWeekEnding";
                    ws.Cells[row, col++].Value = "Cost";
                    ws.Cells[row, col++].Value = "HomeOfficeType";
                    ws.Cells[row, col++].Value = "EmployeeSupplierName";
                    ws.Cells[row, col++].Value = "ExpenditureComment";
                    ws.Cells[row, col++].Value = "ProjectComment";
                    ws.Cells[row, col++].Value = "InvoiceNumber";
                    ws.Cells[row, col++].Value = "IsFeeRecovery";
                    ws.Cells[row, col++].Value = "IsCostRecovery";
                    ws.Cells[row, col++].Value = "Completed";
                    ws.Cells[row, col++].Value = "ExpenseType";
                    ws.Cells[row, col++].Value = "VariationNo";
                    ws.Cells[row, col++].Value = "Description";
                    ws.Cells[row, col++].Value = "TaskNo";
                    ws.Cells[row, col++].Value = "Name";
                    ws.Cells[row, col++].Value = "EmployeeNo";
                    ws.Cells[row, col++].Value = "TravellerName";
                    ws.Cells[row, col++].Value = "ProjectName";
                    ws.Cells[row, col++].Value = "Company_Name";
                    ws.Cells[row, 1, row, col].Style.Font.Bold = true;
                    ws.Cells[row, 1, row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;

                    row++;

                    foreach (ProjectExpenseReportViewModel reconEntry in allData)
                    {
                        projectName = reconEntry.ProjectName;
                        col = 1;
                        ws.Cells[row, col++].Value = reconEntry.TransactionID;
                        ws.Cells[row, col++].Value = reconEntry.ExpenseDate;
                        ws.Cells[row, col++].Value = reconEntry.CostedInWeekEnding;
                        ws.Cells[row, col++].Value = reconEntry.Cost;
                        ws.Cells[row, col++].Value = reconEntry.HomeOfficeType;
                        ws.Cells[row, col++].Value = reconEntry.EmployeeSupplierName;
                        ws.Cells[row, col++].Value = reconEntry.ExpenditureComment;
                        ws.Cells[row, col++].Value = reconEntry.ProjectComment;
                        ws.Cells[row, col++].Value = reconEntry.InvoiceNumber;
                        ws.Cells[row, col++].Value = reconEntry.IsFeeRecovery;
                        ws.Cells[row, col++].Value = reconEntry.IsCostRecovery;
                        ws.Cells[row, col++].Value = reconEntry.Completed;
                        ws.Cells[row, col++].Value = reconEntry.ExpenseType;
                        ws.Cells[row, col++].Value = reconEntry.VariationNo;
                        ws.Cells[row, col++].Value = reconEntry.Description;
                        ws.Cells[row, col++].Value = reconEntry.TaskNo;
                        ws.Cells[row, col++].Value = reconEntry.Name;
                        ws.Cells[row, col++].Value = reconEntry.EmployeeNo;
                        ws.Cells[row, col++].Value = reconEntry.TravellerName;
                        ws.Cells[row, col++].Value = reconEntry.ProjectName;
                        ws.Cells[row, col++].Value = reconEntry.Company_Name;
                        row++;
                    }

                    for (int i = 1; i < 25; i++)
                        ws.Column(i).AutoFit();

                    package.SaveAs(filePath);
                }



                byte[] bytes = System.IO.File.ReadAllBytes(filePath.FullName);

                var date = DateTime.Now.ToString("yyyyMMddHHmmss");
                string filename = $"ExpenseReport_{projectName}_{date}.xlsx";

                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);

            }
        }
    }
}