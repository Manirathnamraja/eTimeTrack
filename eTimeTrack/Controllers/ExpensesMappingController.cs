using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class ExpensesMappingController : BaseController
    {
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult Index()
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            if (selectedProject == 0) { return InvokeHttp404(HttpContext); }

            var results = (from m in Db.ProjectExpensesMappings
                          join t in Db.ProjectExpenseTypes on m.ProjectTypeID equals t.ExpenseTypeID
                          join u in Db.ProjectExpensesUploads on m.StdExpTypeID equals u.ProjectExpTypeID
                          where m.ProjectID == selectedProject
                          group new {t, m} by new {t.ExpenseType, m.StdExpTypeID, m.ProjectID, m.CompanyID} into grp
                          select new ExpensesMappingDetails
                          {
                              ExpenseType = grp.Key.ExpenseType,
                              StdExpTypeID = grp.Key.StdExpTypeID,
                              ProjectID = grp.Key.ProjectID,
                              CompanyID = grp.Key.CompanyID
                          }).ToList();

            List <SelectListItem> SelectStdExpense = GetStdExpenseTypesSelectItems();

            return View(new ExpensesMappingViewModel
            {
                ExpensesMappingDetails = results,
                StdExpenseTypes = SelectStdExpense
            });
        }

        private List<SelectListItem> GetStdExpenseTypesSelectItems()
        {
            List<SelectListItem> selectItems = Db.ProjectExpensesStdDetails.Select(x => new SelectListItem { Value = x.StdTypeID.ToString(), Text = x.StdType }).ToList();
            return selectItems;

        }
    }
}