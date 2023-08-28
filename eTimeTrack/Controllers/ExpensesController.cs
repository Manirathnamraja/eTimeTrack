using Elmah.ContentSyndication;
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
    public class ExpensesController : BaseController
    {

        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult ExpensesTypes()
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            if (selectedProject == 0) { return InvokeHttp404(HttpContext); }
            ExpensesTypesViewModel expensesTypesViewModel = new ExpensesTypesViewModel();
            var result = (from e in Db.ProjectExpenseTypes
                         join t in Db.ProjectTasks on e.TaskID equals t.TaskID
                         join v in Db.ProjectVariations on e.VariationID equals v.VariationID
                         where t.ProjectID == selectedProject
                         select new ExpensesTypes
                         {
                             ExpenseTypeID = e.ExpenseTypeID,
                             ExpenseType = e.ExpenseType,
                             TaskID = e.TaskID,
                             VariationID = e.VariationID,
                             Name = t.Name,
                             Description = v.Description,
                             ProjectId = t.ProjectID
                         }).ToList();

            // SetViewbag(result);
            foreach (var item in result)
            {
                expensesTypesViewModel.Tasks = Gettask(item.ProjectId, item.Name);
                expensesTypesViewModel.variations = Getvariations(item.ProjectId, item.Description);

            }
            return View(new ExpensesTypesViewModel
            {
                ExpensesTypesDetails = result,
                Tasks = expensesTypesViewModel.Tasks,
                variations = expensesTypesViewModel.variations
            }); 
        }

        private SelectList Gettask(int projectId, string name)
        {
            return new SelectList(GetProjectTaskdetails(projectId), "TaskID", "Name", name);
        }
        private SelectList Getvariations(int projectId, string name)
        {
            return new SelectList(GetProjectVariationdetails(projectId), "VariationID", "Description", name);
        }
        private List<ProjectTask> GetProjectTaskdetails(int? projectId)
        {
            var deta =  Db.ProjectTasks.Where(x => x.ProjectID == projectId).ToList();
            return deta;
        }
       
        private List<ProjectVariation> GetProjectVariationdetails(int? projectId)
        {
            return Db.ProjectVariations.Where(x => x.ProjectID == projectId).ToList();
        }
    }
}