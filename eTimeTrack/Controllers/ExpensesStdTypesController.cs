using eTimeTrack.Enums;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;


namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class ExpensesStdTypesController : BaseController
    {
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult Index()
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            if (selectedProject == 0) { return InvokeHttp404(HttpContext); }

            var results = (from c in Db.Companies
                          join e in Db.ProjectExpensesStdDetails on c.Company_Id equals e.CompanyID
                          select new ExpensesStdTypesDetails
                          {
                              StdType = e.StdType,
                              CompanyID = e.CompanyID,
                              IsActive = e.IsActive,
                              StdTypeID = e.StdTypeID,
                              CompanyName = c.Company_Name
                          }).OrderByDescending(x => x.StdTypeID).ToList();
            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(new ExpensesStdTypesViewModel
            {
                expensesStdTypesDetails = results
            });
        }

        public ActionResult Create()
        {
            ProjectExpensesStdDetails Expense = new ProjectExpensesStdDetails();
            ViewBag.CompanyID = GetCompany();
            ViewBag.Source = Source.Create;
            return View("CreateEdit", Expense);
        }

        public ActionResult edit(int? id)
        {
            ProjectExpensesStdDetails Expense = Db.ProjectExpensesStdDetails.Find(id);
            ViewBag.CompanyID = GetCompany();
            ViewBag.Source = Source.Existing;
            return View("CreateEdit", Expense);
        }


        [HttpPost]
        public ActionResult CreateEdit(ProjectExpensesStdDetails projectExpensesStdDetails, Source source)
        {
            ProjectExpensesStdDetails existing = Db.ProjectExpensesStdDetails.Find(projectExpensesStdDetails.StdTypeID);
            projectExpensesStdDetails.LastModifiedBy = UserHelpers.GetCurrentUserId();
            projectExpensesStdDetails.LastModifiedDate = DateTime.UtcNow;
            if (existing != null)
            {
                Db.Entry(existing).CurrentValues.SetValues(projectExpensesStdDetails);
                Db.Entry(existing).State = EntityState.Modified;
            }
            else
            {
                Db.ProjectExpensesStdDetails.Add(projectExpensesStdDetails);
            }
            Db.SaveChanges();
            TempData["InfoMessage"] = new InfoMessage { MessageContent = $"Project Expense Std Type {projectExpensesStdDetails.StdType}  Saved.", MessageType = InfoMessageType.Success };
            return RedirectToAction("Index");
        }

        private SelectList GetCompany()
        {
            return new SelectList(Getcompanydetails(), "Company_Id", "Company_Name");
        }

        private List<Company> Getcompanydetails()
        {
            List<Company> company = Db.Companies.Join(Db.ProjectCompanies, c => c.Company_Id, p => p.CompanyId, (c, p) => c).Distinct().ToList();
            return company;
        }
    }
}