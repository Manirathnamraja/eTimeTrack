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

            List<ProjectExpenseType> ProjectExpenseType = Db.ProjectExpenseTypes.Where(x => x.ProjectID == selectedProject).ToList();
            List<SelectListItem> SelectStdExpense = GetStdExpenseTypesSelectItems();

            return View(new ExpensesMappingViewModel
            {
                projectExpenseTypes = ProjectExpenseType,
                StdExpenseTypes = SelectStdExpense,
            });
        }

        private List<SelectListItem> GetStdExpenseTypesSelectItems()
        {
            List<SelectListItem> selectItems = Db.ProjectExpensesStdDetails.Select(x => new SelectListItem { Value = x.StdTypeID.ToString(), Text = x.StdType }).ToList();
            return selectItems;

        }
    }
}