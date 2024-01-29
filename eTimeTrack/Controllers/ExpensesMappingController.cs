using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                           where m.ProjectID == selectedProject
                           select new ExpensesMappingDetails
                           {
                               ExpenseType = m.ProjectMapTable,
                               StdExpTypeID = m.StdExpTypeID,
                               ProjectID = m.ProjectID,
                               CompanyID = m.CompanyID,
                               LastModifiedDate = m.LastModifiedDate,
                               MapID = m.MapID
                           }).OrderByDescending(x => x.LastModifiedDate).ToList();

            List<SelectListItem> SelectStdExpense = GetStdExpenseTypesSelectItems();
            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(new ExpensesMappingViewModel
            {
                ExpensesMappingDetails = results,
                StdExpenseTypes = SelectStdExpense
            });
        }

        public ActionResult Create()
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            ProjectExpensesMapping mapping = new ProjectExpensesMapping();
            mapping.ProjectID = selectedProject;

            SetViewbag(mapping.ProjectID);
            ViewBag.Source = Source.Create;
            return View("Create", mapping);
        }

        public ActionResult edit(int? id)
        {
            ProjectExpensesMapping projectExpense = Db.ProjectExpensesMappings.Find(id);
            SetViewbag(projectExpense.ProjectID);
            ViewBag.Source = Source.Existing;
            return View("Create", projectExpense);
        }

        [HttpPost]
        public ActionResult DeleteConfirmed(int? id)
        {
            ProjectExpensesMapping mapping = Db.ProjectExpensesMappings.Where(x => x.MapID == id).FirstOrDefault();
            if (mapping != null)
            {
                Db.ProjectExpensesMappings.Attach(mapping);
                Db.ProjectExpensesMappings.Remove(mapping);
                Db.SaveChanges();
            }
            TempData["InfoMessage"] = new InfoMessage(InfoMessageType.Success, "Succesfully deleted Expenses Mapping");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CreateEdit(ProjectExpensesMapping projectExpensesMapping, Source source)
        {
            ProjectExpensesMapping existing = Db.ProjectExpensesMappings.Find(projectExpensesMapping.MapID);
            projectExpensesMapping.LastModifiedBy = UserHelpers.GetCurrentUserId();
            projectExpensesMapping.LastModifiedDate = DateTime.UtcNow;
            if (existing != null)
            {
                Db.Entry(existing).CurrentValues.SetValues(projectExpensesMapping);
                Db.Entry(existing).State = EntityState.Modified;
            }
            else
            {
                Db.ProjectExpensesMappings.Add(projectExpensesMapping);
            }
            Db.SaveChanges();
            TempData["InfoMessage"] = new InfoMessage { MessageContent = $"Project Expense Type { projectExpensesMapping.ProjectMapTable }  Saved.", MessageType = InfoMessageType.Success };
            return RedirectToAction("Index");
        }

        private void SetViewbag(int projectId)
        {
            ViewBag.ProjectID = new SelectList(Db.Projects, "ProjectID", "Name");
            ViewBag.ProjectTypeID = new SelectList(GetExpensesTypesdetails(projectId), "ExpenseTypeID", "ExpenseType");
            ViewBag.CompanyID = new SelectList(Db.Companies, "Company_Id", "Company_Name");
            ViewBag.StdExpTypeID = new SelectList(Db.ProjectExpensesStdDetails, "StdTypeID", "StdType");
        }

        private List<SelectListItem> GetStdExpenseTypesSelectItems()
        {
            List<SelectListItem> selectItems = Db.ProjectExpensesStdDetails.Select(x => new SelectListItem { Value = x.StdTypeID.ToString(), Text = x.StdType }).ToList();
            return selectItems;
        }
        
        private List<ProjectExpenseType> GetExpensesTypesdetails(int projectId)
        {
            List<ProjectExpenseType> expenses = Db.ProjectExpenseTypes.Where(x => x.ProjectID == projectId).ToList();
            return expenses;
        }
    }
}