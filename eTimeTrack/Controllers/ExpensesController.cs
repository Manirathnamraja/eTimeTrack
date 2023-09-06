using Elmah.ContentSyndication;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                             ProjectId = t.ProjectID,
                             IsClosed = e.IsClosed,
                             IsCostRecovery = e.IsCostRecovery,
                             IsFeeRecovery = e.IsFeeRecovery
                         }).ToList();

            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(new ExpensesTypesViewModel
            {
                ExpensesTypesDetails = result
            }); 
        }

        [HttpPost]
        public ActionResult DeleteConfirmed(int? id)
        {
            ProjectExpenseType expensesTypes = Db.ProjectExpenseTypes.Where(x => x.ExpenseTypeID == id).FirstOrDefault();
            if(expensesTypes != null)
            {
                Db.ProjectExpenseTypes.Attach(expensesTypes);
                Db.ProjectExpenseTypes.Remove(expensesTypes);
                Db.SaveChanges();
            }
            TempData["InfoMessage"] = new InfoMessage(InfoMessageType.Success, "Succesfully deleted Expenses Types");
            return RedirectToAction("ExpensesTypes");
        }

        public ActionResult Create()
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            ProjectExpenseType projectExpense = new ProjectExpenseType();
            projectExpense.ProjectID = selectedProject;

            SetViewbag(projectExpense.ProjectID);
            ViewBag.Source = Source.Create;
            return View("CreateEdit", projectExpense);
        }

        public ActionResult edit(int? id)
        {
            ProjectExpenseType projectExpense = Db.ProjectExpenseTypes.Find(id);
            SetViewbag(projectExpense.ProjectID);
            ViewBag.Source = Source.Existing;
            return View("CreateEdit", projectExpense);
        }

        [HttpPost]
        public ActionResult CreateEdit(ProjectExpenseType projectExpenseType, Source source)
        {
            ProjectExpenseType existing = Db.ProjectExpenseTypes.Find(projectExpenseType.ExpenseTypeID);
            projectExpenseType.LastModifiedBy = UserHelpers.GetCurrentUserId();
            projectExpenseType.LastModifiedDate = DateTime.UtcNow;
            if(existing != null)
            {
                Db.Entry(existing).CurrentValues.SetValues(projectExpenseType);
                Db.Entry(existing).State = EntityState.Modified;
            }
            else
            {
                Db.ProjectExpenseTypes.Add(projectExpenseType);
            }
            Db.SaveChanges();
            TempData["InfoMessage"] = new InfoMessage { MessageContent = $"Project Expense Type {projectExpenseType.ExpenseType}  Saved.", MessageType = InfoMessageType.Success };
            return RedirectToAction("ExpensesTypes");
        }

        private void SetViewbag(int? projectId)
        {
            ViewBag.ProjectID = new SelectList(Db.Projects, "ProjectID", "Name");
            ViewBag.TaskID = new SelectList(GetProjectTaskdetails(projectId), "TaskID", "Name");
            ViewBag.VariationID = new SelectList(GetProjectVariationdetails(projectId), "VariationID", "Description");
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