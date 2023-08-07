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
using Elmah.ContentSyndication;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.ComponentModel.Design;

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
            };
            
            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(model);
        }

        [HttpPost]
        public ActionResult UserSelect(UserSelectviewmodel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["InfoMessage"] = new InfoMessage { MessageContent = "Please select all required parameters", MessageType = InfoMessageType.Failure };
                return RedirectToAction("UserSelect");
            }
            return RedirectToAction("UserItemSelect", new
            {
                project = Convert.ToInt64(model.Project),
                enddate = model.EndDate,
                newdate = model.NewDate,
                company = Convert.ToInt32(model.Company),
            });

        }

        public ActionResult UserItemSelect(int project, DateTime enddate, DateTime newdate, int company)
        {
            var results = from u in Db.UserRates
                          join e in Db.Users on u.EmployeeId equals e.Id
                          join c in Db.Companies on e.CompanyID equals c.Company_Id
                          join p in Db.Projects on u.ProjectId equals p.ProjectID
                          where p.ProjectID == project && u.EndDate == enddate
                          select new UserSelectviewmodel
                          {
                              Company = c.Company_Name,
                              Company_Id = c.Company_Id,
                              EmployeeID = e.Id,
                              UserNumber = e.EmployeeNo,
                              UserName = e.Names,
                              UserRateId = u.UserRateId,
                              Project = p.Name,
                              ProjectId = p.ProjectID,
                              EndDate = u.EndDate,
                              NewDate = newdate
                          };

            var projectname = Db.Projects.Where(x => x.ProjectID == project).Select(x => x.Name).FirstOrDefault();
            var companies = Db.Companies.Where(x => x.Company_Id == company).Select(x => new
            {
                x.Company_Id,
                x.Company_Name
            }).FirstOrDefault();

            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(new UserSelectUpdateViewModel { 
                UserRatesDetails = results.ToList(),
                Company = companies.Company_Name,
                Project = projectname,
                EndDate = enddate,
                NewDate = newdate,
                CompanyId = companies.Company_Id
            });
        }

        [HttpPost]
        public ActionResult UserItemSelect(UserSelectUpdateViewModel model, DateTime enddate, int projectid, DateTime newdate)
        {

            foreach (var item in model.UserRatesDetails)
            {
                if (item.Transfer == true)
                {
                    TransferItems(item.UserRateId, item.NewDate);
                }
            }

            TempData["InfoMessage"] = new InfoMessage { MessageContent = $"<p>{model.UserRatesDetails.Count(x => x.Transfer)} User End Dates Updated Succesfully</p>", MessageType = InfoMessageType.Success };
            return RedirectToAction("UserItemSelect", new
            {
                enddate = enddate,
                project = projectid,
                newdate = newdate,
                company = model.CompanyId

            });
        }

        private void TransferItems(int userrateid, DateTime? newDate)
        {
            UserRate item = Db.UserRates.Find(userrateid);
            item.EndDate = newDate;
            Db.SaveChanges();
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
            List<Company> company = Db.Companies.Join(Db.ProjectCompanies, c => c.Company_Id, p => p.CompanyId, (c, p) => c).Distinct().ToList();
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