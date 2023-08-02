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

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class UserEndDatesController : BaseController
    {
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult UserSelect()
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            if (selectedProject == 0) { return InvokeHttp404(HttpContext); }

            UserEndDatesViewModel model = new UserEndDatesViewModel
            {
                ProjectId = selectedProject,
                Project = GetProject(selectedProject),
                Company = GetCompany(),
                NewDate = DateTime.Now.ToString("dd/MMM/yyyy")
            };
            SelectList select = GetEnddates(selectedProject);
            model.DateSelect = select;

            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(model);
        }

        [HttpPost]
        public ActionResult UserSelect(UserSelectviewmodel model)
        {
            if(!ModelState.IsValid)
            {
                TempData["InfoMessage"] = new InfoMessage { MessageContent = "Please select all required parameters", MessageType = InfoMessageType.Failure };
                return RedirectToAction("UserSelect");
            }
            return RedirectToAction("UserItemSelect", new
            {
                project = model.Project,
                company = Convert.ToInt64(model.Company),
                newdate = model.NewDate,
                enddate = Convert.ToInt64(model.EndDate)
            });
            
        }

        public ActionResult UserItemSelect(int company, int enddate)
        {
            Company companies = Db.Companies.Where(x => x.Company_Id == company).FirstOrDefault();
            Employee emp = Db.Users.Where(x => x.CompanyID == company).FirstOrDefault();
            UserRate userrates = Db.UserRates.Where(x => x.UserRateId == enddate).FirstOrDefault();
            return View(new UserSelectviewmodel { Company = companies.Company_Name, UserNumber = emp.EmployeeNo, UserName = emp.Names, EndDate = userrates.EndDate.ToString() });
        }

        private SelectList GetProject(int projectId)
        {
            return new SelectList(GetProjectdetails(projectId), "ProjectID", "DisplayName", 1);
        }

        private SelectList GetCompany()
        {
            return new SelectList(Getcompanydetails(), "Company_Id", "Company_Name", 1);
        }
        private List<Project> GetProjectdetails(int? projectId)
        {
            return Db.Projects.Where(x => x.ProjectID == projectId).ToList();
        }

        private List<Company> Getcompanydetails()
        {
            List<Company> company  = Db.Companies.Join(Db.ProjectCompanies, c => c.Company_Id, p => p.CompanyId,(c, p) => c).Distinct().ToList();
            return company;
        }
        private SelectList GetEnddates(int projectId)
        {
            List<UserRate> projectDatePeriods = Db.UserRates.ToList();

            IEnumerable<SelectListItem> selectItems = projectDatePeriods.Select(x => new SelectListItem()
            {
                Value = x.UserRateId.ToString(),
                Text = x.EndDate.ToString()
            });
            SelectList existingPeriods = new SelectList(selectItems, "Value", "Text", 1);

            var texts = selectItems.Select(x => x.Value).ToList();

            return existingPeriods;
        }
    }
}