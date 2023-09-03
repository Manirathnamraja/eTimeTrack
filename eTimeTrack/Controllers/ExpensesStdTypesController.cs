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
        public ActionResult Index(int? id)
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            if (selectedProject == 0) { return InvokeHttp404(HttpContext); }
            int companyid = id ?? 1;

            var results = (from c in Db.Companies
                          join e in Db.ProjectExpensesStdDetails on c.Company_Id equals e.CompanyID
                          where e.CompanyID == companyid
                          select new ExpensesStdTypesDetails
                          {
                              StdType = e.StdType,
                              CompanyID = e.CompanyID,
                              IsActive = e.IsActive,
                              StdTypeID = e.StdTypeID
                          }).ToList();

            var Compvalue = Db.Companies.Where(x => x.Company_Id == companyid).Select(x => x.Company_Name).FirstOrDefault();

            return View(new ExpensesStdTypesViewModel
            {
                Company = GetCompany(Compvalue),
                expensesStdTypesDetails = results,
            });
        }

        [HttpPost]
        public ActionResult ExpensesDetails(int id) 
        {
            var results = Db.ProjectExpensesStdDetails.Where(x => x.CompanyID == id).Select( x => new ExpensesStdTypesDetails
            {
                StdType = x.StdType,
                CompanyID = x.CompanyID,
                IsActive = x.IsActive,
                StdTypeID = x.StdTypeID
            }).ToList();
            return View("Index", new ExpensesStdTypesViewModel
            {
                expensesStdTypesDetails = results
            }); 
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