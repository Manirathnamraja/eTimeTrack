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
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Spire.Doc;
using Elmah.ContentSyndication;
using System.Configuration;
using System.Web.UI;
using System.Web.Services.Description;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class WeeklyCostReportsController : BaseController
    {
        WeeklyCostReportDetails costeport = null;

        SqlConnection con;
        SqlDataAdapter adap;
        DataTable dt;
        public WeeklyCostReportsController()
        {
            con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        }

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
        public FileResult PrintFriendlyExcelData(int projectId,DateTime fromDate,DateTime toDate)
        {
            InfoMessage message;
            var allData = Db.Database.SqlQuery<WeeklyCostReportDetails>("EXEC GetWeeklyCostReport2 @ProjectId, @FromDate, @ToDate",
                   new SqlParameter("ProjectId", projectId), new SqlParameter("FromDate", fromDate), new SqlParameter("ToDate", toDate)).ToList();

            if(allData.Count == 0)
            {
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = "There are no rates available for this date range. Please try for different date range." };
                ViewBag.InfoMessage = message;
                return null;
            }
             
            var projectName = "";

            FileInfo filePath = GetGuidFilePath("xlsx");

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorkbook workbook = package.Workbook;
                ExcelWorksheet ws = workbook.Worksheets.Add("DailyCostRates");

                int row = 1;
                int col = 1;

                ws.Cells[row, col++].Value = "Project";
                ws.Cells[row, col++].Value = "Company Code";
                ws.Cells[row, col++].Value = "Employee Number";
                ws.Cells[row, col++].Value = "Employee Name";
                ws.Cells[row, col++].Value = "Project Role";
                ws.Cells[row, col++].Value = "Classification";
                ws.Cells[row, col++].Value = "Discipline";
                ws.Cells[row, col++].Value = "Weekending Date";
                ws.Cells[row, col++].Value = "Work Date";
                ws.Cells[row, col++].Value = "Task No";
                ws.Cells[row, col++].Value = "Alias Code";
                ws.Cells[row, col++].Value = "Task Name";
                ws.Cells[row, col++].Value = "Variation No";
                ws.Cells[row, col++].Value = "Variation Name";
                ws.Cells[row, col++].Value = "Daily Hrs";
                ws.Cells[row, col++].Value = "Daily Comments";
                ws.Cells[row, col++].Value = "General Comments";
                ws.Cells[row, col++].Value = "Part No";
                ws.Cells[row, col++].Value = "Part Name";
                ws.Cells[row, col++].Value = "Time Code";                
                ws.Cells[row, col++].Value = "Time Code Name";                
                ws.Cells[row, col++].Value = "Fee Rate";                
                ws.Cells[row, col++].Value = "Cost Rate";                
                ws.Cells[row, col++].Value = "Fee";                
                ws.Cells[row, col++].Value = "Cost";                            
                ws.Cells[row, col++].Value = "Variation Approved";               
                ws.Cells[row, 1, row, col].Style.Font.Bold = true;
                ws.Cells[row, 1, row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;

                row++;

                foreach (WeeklyCostReportDetails dailycostRate in allData)
                {
                   // projectName = dailycostRate.Project.Name;

                    col = 1;
                    ws.Cells[row, col++].Value = dailycostRate.ProjectName;
                    ws.Cells[row, col++].Value = dailycostRate.Company_Code;
                    ws.Cells[row, col++].Value = dailycostRate.EmployeeNo;
                    ws.Cells[row, col++].Value = dailycostRate.EmployeeName;
                    ws.Cells[row, col++].Value = dailycostRate.ProjectRole;
                    ws.Cells[row, col++].Value = dailycostRate.Classifications;
                    ws.Cells[row, col++].Value = dailycostRate.Disciplines;
                    ws.Cells[row, col++].Value = dailycostRate.WeekendingDate;
                    ws.Cells[row, col++].Value = dailycostRate.WorkDate;
                    ws.Cells[row, col++].Value = dailycostRate.TaskNo;
                    ws.Cells[row, col++].Value = dailycostRate.AliasCode;
                    ws.Cells[row, col++].Value = dailycostRate.TaskName;
                    ws.Cells[row, col++].Value = dailycostRate.VariationNo;
                    ws.Cells[row, col++].Value = dailycostRate.VariationName;
                    ws.Cells[row, col++].Value = dailycostRate.DailyHrs;
                    ws.Cells[row, col++].Value = dailycostRate.DailyComments;
                    ws.Cells[row, col++].Value = dailycostRate.GeneralComments;
                    ws.Cells[row, col++].Value = dailycostRate.PartNo;
                    ws.Cells[row, col++].Value = dailycostRate.PartName;
                    ws.Cells[row, col++].Value = dailycostRate.TimeCode;                    
                    ws.Cells[row, col++].Value = dailycostRate.TimeCodeName;                    
                    ws.Cells[row, col++].Value = dailycostRate.FeeRate;
                    ws.Cells[row, col++].Value = dailycostRate.CostRate;
                    ws.Cells[row, col++].Value = dailycostRate.Fee;
                    ws.Cells[row, col++].Value = dailycostRate.Cost;                    
                    ws.Cells[row, col++].Value = dailycostRate.VariationApproved ? "Y" : "N";                    
                    row++;
                }

                for (int i = 1; i < 25; i++)
                    ws.Column(i).AutoFit();

                package.SaveAs(filePath);
            }

            byte[] bytes = System.IO.File.ReadAllBytes(filePath.FullName);

            var date = DateTime.Now.ToString("yyyyMMddHHmmss");
            string filename = $"DailyCostReport_{projectName}_{date}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
        }

        public class WeeklyCostReportDetails
        {
            public string ProjectName { get; set; }
            public string EmployeeNo { get; set; }
            public string EmployeeName { get; set; }
            public string ProjectRole { get; set; }
            public string WeekendingDate { get; set; }
            public string WorkDate { get; set; }
            public decimal? DailyHrs { get; set; }
            public string TaskNo { get; set; }
            public string AliasCode { get; set; }
            public string TaskName { get; set; }            
            public string PartNo { get; set; }
            public string PartName { get; set; }
            public int TimeCode { get; set; }
            public string TimeCodeName { get; set; }
            public string Company_Code { get; set; }           
            public string Classifications { get; set; }            
            public string Disciplines { get; set; }
            public string FeeRate { get; set; }
            public string CostRate { get; set; }
            public decimal? Fee { get; set; }
            public decimal? Cost { get; set; }            
            public string VariationNo { get; set; }
            public string VariationName { get; set; }            
            public bool VariationApproved { get; set; }
            public string DailyComments { get; set; }
            public string GeneralComments { get; set; }

            public virtual Project Project { get; set; }
        }

    }
}