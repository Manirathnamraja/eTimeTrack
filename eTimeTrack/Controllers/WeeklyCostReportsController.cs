using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using System;
using System.Web.UI.WebControls;
using EntityState = System.Data.Entity.EntityState;
using System.Web;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Xml;
using OfficeOpenXml;
using System.Globalization;
using System.Web.Hosting;
using System.Threading.Tasks;
using eTimeTrack.Extensions;
using OfficeOpenXml.Style;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class WeeklyCostReportsController : BaseController
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

            WeeklyCostReportsIndexUserVm vm = new WeeklyCostReportsIndexUserVm { ProjectList = GenerateDropdownUserProjects() };

            return View(vm);
        }


        [HttpPost]
        public FileResult PrintFriendlyExcelData(int projectId)
        {

            var allData = Db.Database.SqlQuery<WeeklyCostReportDetails>("EXEC GetWeeklyCostReport @ProjectId",
                   new SqlParameter("ProjectId", projectId)).ToList();

            
            var projectName = "";

            FileInfo filePath = GetGuidFilePath("xlsx");

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorkbook workbook = package.Workbook;
                ExcelWorksheet ws = workbook.Worksheets.Add("WeeklyCostRates");

                int row = 1;
                int col = 1;

                ws.Cells[row, col++].Value = "Employee Number";
                ws.Cells[row, col++].Value = "Employee Name";
                ws.Cells[row, col++].Value = "Project Role";                
                ws.Cells[row, col++].Value = "End Date";
                ws.Cells[row, col++].Value = "Task No";
                ws.Cells[row, col++].Value = "Task Name";
                ws.Cells[row, col++].Value = "AGP Works OrderNo";
                ws.Cells[row, col++].Value = "Works Order Title";
                ws.Cells[row, col++].Value = "Part No";
                ws.Cells[row, col++].Value = "Part Nam";
                ws.Cells[row, col++].Value = "Company Name";
                ws.Cells[row, col++].Value = "Group No";
                ws.Cells[row, col++].Value = "Group Name";
                ws.Cells[row, col++].Value = "Current Classification";
                ws.Cells[row, col++].Value = "Current FeeRate";
                ws.Cells[row, col++].Value = "Employee Discipline";
                ws.Cells[row, col++].Value = "Office Name";
                ws.Cells[row, col++].Value = "Alias Code";
                ws.Cells[row, col++].Value = "OT6 Cost Rate";
                ws.Cells[row, col++].Value = "OT7 Cost Rate";
                ws.Cells[row, col++].Value = "IsApproved";
                ws.Cells[row, col++].Value = "Comments";
                ws.Cells[row, 1, row, col].Style.Font.Bold = true;
                ws.Cells[row, 1, row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;

                row++;

                foreach (WeeklyCostReportDetails reconEntry in allData)
                {
                  //  projectName = reconEntry.Project.Name;

                    col = 1;
                    ws.Cells[row, col++].Value = reconEntry.EmployeeNo;
                    ws.Cells[row, col++].Value = reconEntry.Username;
                    ws.Cells[row, col++].Value = reconEntry.ProjectRole;                    
                    ws.Cells[row, col++].Value = reconEntry.EndDate;
                    ws.Cells[row, col++].Value = reconEntry.TaskNo;
                    ws.Cells[row, col++].Value = reconEntry.TaskName;
                    ws.Cells[row, col++].Value = reconEntry.AGP_Works_OrderNo;
                    ws.Cells[row, col++].Value = reconEntry.Works_Order_Title;
                    ws.Cells[row, col++].Value = reconEntry.PartNo;
                    ws.Cells[row, col++].Value = reconEntry.PartName;
                    ws.Cells[row, col++].Value = reconEntry.Company_Name;
                    ws.Cells[row, col++].Value = reconEntry.GroupNo;
                    ws.Cells[row, col++].Value = reconEntry.GroupName;
                    ws.Cells[row, col++].Value = reconEntry.Current_Classification;
                    ws.Cells[row, col++].Value = reconEntry.Current_FeeRate;
                    ws.Cells[row, col++].Value = reconEntry.Emp_Discipline;
                    ws.Cells[row, col++].Value = reconEntry.OfficeName;
                    ws.Cells[row, col++].Value = reconEntry.AliasCode;
                    ws.Cells[row, col++].Value = reconEntry.IsApproved ? "Y" : "N";
                    ws.Cells[row, col++].Value = reconEntry.Comments;
                    
                    row++;
                }

                for (int i = 1; i < 25; i++)
                    ws.Column(i).AutoFit();

                package.SaveAs(filePath);
            }

            byte[] bytes = System.IO.File.ReadAllBytes(filePath.FullName);

            var date = DateTime.Now.ToString("yyyyMMddHHmmss");
            string filename = $"WeeklyCosReport_{projectName}_{date}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
        }

       

        public class WeeklyCostReportDetails
        {
            public string EmployeeNo { get; set; }
            public string Username { get; set; }
            public string ProjectRole { get; set; }
            public DateTime EndDate { get; set; }
            public string TaskNo { get; set; }
            public string TaskName { get; set; }
            public string AGP_Works_OrderNo { get; set; }
            public string Works_Order_Title { get; set; }
            public string PartNo { get; set; }
            public string PartName { get; set; }
            public string Company_Name { get; set; }
            public string GroupNo { get; set; }
            public string GroupName { get; set; }
            public string Current_Classification { get; set; }
            public string Current_FeeRate { get; set; }
            public string Emp_Discipline { get; set; }
            public string OfficeName { get; set; }
            public string AliasCode { get; set; }
            public bool IsApproved { get; set; }
            public string Comments { get; set; }

            public virtual Project Project { get; set; }
        }

    }
}