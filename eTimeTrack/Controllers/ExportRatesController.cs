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
    public class ExportRatesController : BaseController
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

            IQueryable<UserRate> query = Db.UserRates.Where(x => x.ProjectId == projectId);
            List<UserRate> allData = query.OrderByDescending(x=>x.LastModifiedDate).ToList();
            var projectName = "";

            FileInfo filePath = GetGuidFilePath("xlsx");

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorkbook workbook = package.Workbook;
                ExcelWorksheet ws = workbook.Worksheets.Add("ProjectUserRates");

                int row = 1;
                int col = 1;

                ws.Cells[row, col++].Value = "Project";
                ws.Cells[row, col++].Value = "Employee Number";
                ws.Cells[row, col++].Value = "Employee Name";
                ws.Cells[row, col++].Value = "Email";
                ws.Cells[row, col++].Value = "Project Role/Position";
                ws.Cells[row, col++].Value = "Company";
                ws.Cells[row, col++].Value = "Office";
                ws.Cells[row, col++].Value = "Discipline";
                ws.Cells[row, col++].Value = "Classification";
                ws.Cells[row, col++].Value = "Rate Start Date";
                ws.Cells[row, col++].Value = "Rate End Date";
                ws.Cells[row, col++].Value = "NT Fee Rate";
                ws.Cells[row, col++].Value = "OT1 Fee Rate";
                ws.Cells[row, col++].Value = "OT2 Fee Rate";
                ws.Cells[row, col++].Value = "OT3 Fee Rate";
                ws.Cells[row, col++].Value = "OT4 Fee Rate";
                ws.Cells[row, col++].Value = "OT5 Fee Rate";
                ws.Cells[row, col++].Value = "OT6 Fee Rate";
                ws.Cells[row, col++].Value = "OT7 Fee Rate";
                ws.Cells[row, col++].Value = "NT Cost Rate";
                ws.Cells[row, col++].Value = "OT1 Cost Rate";
                ws.Cells[row, col++].Value = "OT2 Cost Rate";
                ws.Cells[row, col++].Value = "OT3 Cost Rate";
                ws.Cells[row, col++].Value = "OT4 Cost Rate";
                ws.Cells[row, col++].Value = "OT5 Cost Rate";
                ws.Cells[row, col++].Value = "OT6 Cost Rate";
                ws.Cells[row, col++].Value = "OT7 Cost Rate";
                ws.Cells[row, col++].Value = "Rates Confirmed";
                ws.Cells[row, 1, row, col].Style.Font.Bold = true;
                ws.Cells[row, 1, row, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;

                row++;

                foreach (UserRate reconEntry in allData)
                {

                    var userDetail = GetUserDetails(reconEntry.EmployeeId, reconEntry.ProjectId);

                    var officeName = Db.ProjectOffices.Where(x => x.OfficeId == userDetail.OfficeID).Select(y => y.OfficeName);
                    var disciplineText = Db.ProjectDisciplines.Where(x => x.ProjectDisciplineId == userDetail.ProjectDisciplineID).Select(y => y.Text);
                    var projectUserClassificationText = Db.ProjectUserClassifications.Where(x => x.ProjectUserClassificationId == reconEntry.ProjectUserClassificationID).Select(c => c.ProjectClassificationText);
                    DateTime sDate = (DateTime)reconEntry.StartDate;
                    DateTime eDate = (DateTime)reconEntry.EndDate;
                    projectName = reconEntry.Project.Name;
                    var companyName = Db.Companies.Where(x => x.Company_Id == reconEntry.Employee.CompanyID).Select(y => y.Company_Name);

                    col = 1;
                    ws.Cells[row, col++].Value = reconEntry.Project.Name;
                    ws.Cells[row, col++].Value = reconEntry.Employee.EmployeeNo;
                    ws.Cells[row, col++].Value = reconEntry.Employee.Names;
                    ws.Cells[row, col++].Value = reconEntry.Employee.Email;
                    ws.Cells[row, col++].Value = userDetail.ProjectRole;
                    ws.Cells[row, col++].Value = companyName;                   
                    ws.Cells[row, col++].Value = officeName;
                    ws.Cells[row, col++].Value = disciplineText;                    
                    ws.Cells[row, col++].Value = projectUserClassificationText;
                    ws.Cells[row, col++].Value = sDate.ToDateStringGeneral();
                    ws.Cells[row, col++].Value = eDate.ToDateStringGeneral();
                    ws.Cells[row, col++].Value = reconEntry.NTFeeRate;
                    ws.Cells[row, col++].Value = reconEntry.OT1FeeRate;
                    ws.Cells[row, col++].Value = reconEntry.OT2FeeRate;
                    ws.Cells[row, col++].Value = reconEntry.OT3FeeRate;
                    ws.Cells[row, col++].Value = reconEntry.OT4FeeRate;
                    ws.Cells[row, col++].Value = reconEntry.OT5FeeRate;
                    ws.Cells[row, col++].Value = reconEntry.OT6FeeRate;
                    ws.Cells[row, col++].Value = reconEntry.OT7FeeRate;
                    ws.Cells[row, col++].Value = reconEntry.NTCostRate;
                    ws.Cells[row, col++].Value = reconEntry.OT1CostRate;
                    ws.Cells[row, col++].Value = reconEntry.OT2CostRate;
                    ws.Cells[row, col++].Value = reconEntry.OT3CostRate;
                    ws.Cells[row, col++].Value = reconEntry.OT4CostRate;
                    ws.Cells[row, col++].Value = reconEntry.OT5CostRate;
                    ws.Cells[row, col++].Value = reconEntry.OT6CostRate;
                    ws.Cells[row, col++].Value = reconEntry.OT7CostRate;
                    ws.Cells[row, col++].Value = reconEntry.IsRatesConfirmed ? "Y" : "N";
                    row++;
                }

                for (int i = 1; i < 25; i++)
                    ws.Column(i).AutoFit();

                package.SaveAs(filePath);
            }

            byte[] bytes = System.IO.File.ReadAllBytes(filePath.FullName);

            var date = DateTime.Now.ToString("yyyyMMddHHmmss");
            string filename = $"UserRates_{projectName}_{date}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
        }

        private UserDetails GetUserDetails(int employeeId, int projectId)
        {
            var userDetails = Db.EmployeeProjects.Where(x => x.EmployeeId == employeeId && x.ProjectId == projectId).FirstOrDefault();
            UserDetails user = new UserDetails();

            if (userDetails != null)
            {
                 user = new UserDetails
                {
                    ProjectDisciplineID = userDetails.ProjectDisciplineID,
                    ProjectRole = userDetails.ProjectRole,
                    OfficeID = userDetails.OfficeID,
                };
                
            }
            return user;
        }

        public class UserDetails
        {
            public string ProjectRole { get; set; }
            public int? ProjectDisciplineID { get; set; }
            public int? OfficeID { get; set; }
        }

    }
}