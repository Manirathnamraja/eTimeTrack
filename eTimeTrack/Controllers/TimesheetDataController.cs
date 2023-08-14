using eTimeTrack.Extensions;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using eTimeTrack.Enums;
using System.Threading.Tasks;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class TimesheetDataController : BaseController
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

            ExportTimesheetDataViewModel vm = new ExportTimesheetDataViewModel { ProjectList = GenerateDropdownUserProjects() };

            return View(vm);
        }

        [HttpPost]
        public FileResult PrintFriendlyExcelData(int projectId)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {

                var alldata = from e in Db.EmployeeTimesheetItems
                              join t in Db.ProjectTasks on e.TaskID equals t.TaskID
                              join v in Db.ProjectVariations on e.VariationID equals v.VariationID
                              join et in Db.EmployeeTimesheets on e.TimesheetID equals et.TimesheetID
                              join tp in Db.TimesheetPeriods on et.TimesheetPeriodID equals tp.TimesheetPeriodID
                              join emp in Db.Users on et.EmployeeID equals emp.Id
                              where t.ProjectID == projectId
                              select new ExportTimesheetDataViewModel
                              {
                                  employeeTimesheetItem = e,
                                  projectTask = t,
                                  projectVariation = v,
                                  employeeTimesheet = et,
                                  timesheetPeriod = tp,
                                  employee = emp
                              };

                var projectconfig = Db.ProjectTimeCodeConfigs.Where(x => x.ProjectID == projectId).ToList();

                var projectName = "";

                FileInfo filePath = GetGuidFilePath("xlsx");

                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorkbook workbook = package.Workbook;
                    ExcelWorksheet ws = workbook.Worksheets.Add("TimesheetData");

                    int row = 1;
                    int col = 1;

                    ws.Cells[row, col++].Value = "TimesheetItemID";
                    ws.Cells[row, col++].Value = "TimesheetID";
                    ws.Cells[row, col++].Value = "Week Ending";
                    ws.Cells[row, col++].Value = "Task No";
                    ws.Cells[row, col++].Value = "Task Name";
                    ws.Cells[row, col++].Value = "Variation No";
                    ws.Cells[row, col++].Value = "Variation Name";
                    ws.Cells[row, col++].Value = "Employee Number";
                    ws.Cells[row, col++].Value = "User Name";
                    ws.Cells[row, col++].Value = "Time Code";
                    ws.Cells[row, col++].Value = "Time Code Name";
                    ws.Cells[row, col++].Value = "Day1 Hrs";
                    ws.Cells[row, col++].Value = "Day 1 Comments";
                    ws.Cells[row, col++].Value = "Day2 Hrs";
                    ws.Cells[row, col++].Value = "Day 2 Comments";
                    ws.Cells[row, col++].Value = "Day3 Hrs";
                    ws.Cells[row, col++].Value = "Day 3 Comments";
                    ws.Cells[row, col++].Value = "Day4 Hrs";
                    ws.Cells[row, col++].Value = "Day 4 Comments";
                    ws.Cells[row, col++].Value = "Day5 Hrs";
                    ws.Cells[row, col++].Value = "Day 5 Comments";
                    ws.Cells[row, col++].Value = "Day6 Hrs";
                    ws.Cells[row, col++].Value = "Day 6 Comments";
                    ws.Cells[row, col++].Value = "Day7 Hrs";
                    ws.Cells[row, col++].Value = "Day 7 Comments";
                    ws.Cells[row, col++].Value = "Invoice ID";
                    ws.Cells[row, col++].Value = "Comments";
                    ws.Cells[row, col++].Value = "Last Modified By (Name)";
                    ws.Cells[row, col++].Value = "Last Modified Date";
                    ws.Cells[row, 1, row, col].Style.Font.Bold = true;
                    ws.Cells[row, 1, row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;

                    row++;

                    foreach (ExportTimesheetDataViewModel Entry in alldata)
                    {
                        var timecodename = "";
                        if (Entry.employeeTimesheetItem.TimeCode == TimeCode.NT)
                        {
                            timecodename = projectconfig.Select(x => x.NTName).FirstOrDefault();
                        }
                        if (Entry.employeeTimesheetItem.TimeCode == TimeCode.OT1)
                        {
                            timecodename = projectconfig.Select(x => x.OT1Name).FirstOrDefault();
                        }
                        if (Entry.employeeTimesheetItem.TimeCode == TimeCode.OT2)
                        {
                            timecodename = projectconfig.Select(x => x.OT2Name).FirstOrDefault();
                        }
                        if (Entry.employeeTimesheetItem.TimeCode == TimeCode.OT3)
                        {
                            timecodename = projectconfig.Select(x => x.OT3Name).FirstOrDefault();
                        }


                        col = 1;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.TimesheetItemID;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.TimesheetID;
                        ws.Cells[row, col++].Value = Entry.timesheetPeriod.EndDate.ToString("yyyy-MM-dd HH:mm:ss");
                        ws.Cells[row, col++].Value = Entry.projectTask.TaskNo;
                        ws.Cells[row, col++].Value = Entry.projectTask.Name;
                        ws.Cells[row, col++].Value = Entry.projectVariation.VariationNo;
                        ws.Cells[row, col++].Value = Entry.projectVariation.Description;
                        ws.Cells[row, col++].Value = Entry.employee.EmployeeNo;
                        ws.Cells[row, col++].Value = Entry.employee.UserName;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.TimeCode;
                        ws.Cells[row, col++].Value = timecodename;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Day1Hrs;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Day1Comments;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Day2Hrs;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Day2Comments;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Day3Hrs;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Day3Comments;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Day4Hrs;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Day4Comments;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Day5Hrs;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Day5Comments;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Day6Hrs;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Day6Comments;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Day7Hrs;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Day7Comments;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.InvoiceID;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.Comments;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.LastModifiedBy;
                        ws.Cells[row, col++].Value = Entry.employeeTimesheetItem.LastModifiedDate?.ToString("yyyy-MM-dd HH:mm:ss");
                        row++;
                    }

                    for (int i = 1; i < 25; i++)
                        ws.Column(i).AutoFit();

                    package.SaveAs(filePath);
                }



                byte[] bytes = System.IO.File.ReadAllBytes(filePath.FullName);

                var date = DateTime.Now.ToString("yyyyMMddHHmmss");
                string filename = $"TimesheetData_{projectName}_{date}.xlsx";

                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
            }
        }
    }
}