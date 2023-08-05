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

        public ActionResult UserItemSelect(int company, int enddate, string newdate)
        {
            var results = from emp1 in Db.Users
                          join ur in Db.UserRates on emp1.Id equals ur.EmployeeId
                          join comp in Db.Companies on emp1.CompanyID equals comp.Company_Id
                          where comp.Company_Id == company && ur.UserRateId == enddate
                          select new UserSelectviewmodel
                          {
                              Company = comp.Company_Name,
                              Company_Id = comp.Company_Id,
                              EmployeeID = emp1.Id,
                              UserNumber = emp1.EmployeeNo,
                              UserName = emp1.Names,
                              UserRateId = ur.UserRateId,
                              EndDate = ur.EndDate.ToString(),
                              NewDate = newdate
                          };
            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(results.ToList());
        }

        [HttpPost]
        public ActionResult UserItemSelect(FormCollection formCollection)
        {
            var companyid = formCollection["item.Company_Id"];
            var employeeid = formCollection["item.EmployeeID"];
            var userrateid = formCollection["item.UserRateId"];
            var enddate = formCollection["item.EndDate"];
            var username = formCollection["item.UserName"];
            var usernumber = formCollection["item.UserNumber"];
            var company = formCollection["item.Company"];
            var newDate = formCollection["item.NewDate"];
            var updateTransfer = formCollection["item.Transfer"].Split(',')[0];

            if (updateTransfer == "false")
            {
                TempData["InfoMessage"] = new InfoMessage { MessageContent = "Please select the check box and proceed to Ok", MessageType = InfoMessageType.Failure };
                return RedirectToAction("UserItemSelect", new
                {
                    company = Convert.ToInt32(companyid),
                    enddate = userrateid,
                    newdate = newDate
                });
            }
            else
            {
                TransferItems(Convert.ToInt32(userrateid), Convert.ToDateTime(newDate));
                TempData["InfoMessage"] = new InfoMessage { MessageContent = "User End Dates Updated Succesfully", MessageType = InfoMessageType.Success };
                return RedirectToAction("UserItemSelect", new
                {
                    company = Convert.ToInt32(companyid),
                    enddate = userrateid,
                    newdate = newDate
                });
            }
        }

        private void TransferItems(int userrateid, DateTime newDate)
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