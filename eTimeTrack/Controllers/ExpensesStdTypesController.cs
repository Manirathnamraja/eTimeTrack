using eTimeTrack.Enums;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using System;
using System.Collections.Generic;
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
                          }).ToList();

            return View(new ExpensesStdTypesViewModel
            {
                expensesStdTypesDetails = results
            });
        }

        [HttpPost]
        public ActionResult ExpensesDetails(int id) 
        {
            return View();
        }

        private SelectList GetCompany(string val)
        {
            return new SelectList(Getcompanydetails(), "Company_Id", "Company_Name", val);
        }

        private List<Company> Getcompanydetails()
        {
            List<Company> company = Db.Companies.Join(Db.ProjectCompanies, c => c.Company_Id, p => p.CompanyId, (c, p) => c).Distinct().ToList();
            return company;
        }
    }
}