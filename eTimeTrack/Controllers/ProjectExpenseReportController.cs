using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;

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
                var results = context.Database.SqlQuery<ProjectExpenseReportViewModel>("select * from dbo.vw_MET_ProjectExpenses");

                byte[] bytes = System.IO.File.ReadAllBytes("");

                var date = DateTime.Now.ToString("yyyyMMddHHmmss");
                string filename = $"UserRates_{""}_{date}.xlsx";

                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
            }
        }
    }
}