using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class ExpensesAllocationsController : BaseController
    {
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult Allocation(bool hideCompleted = true)
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            if (selectedProject == 0) { return InvokeHttp404(HttpContext); }

            ExpensesAllocationsViewModel viewModel = new ExpensesAllocationsViewModel();
            var results = (from u in Db.ProjectExpensesUploads
                          join c in Db.Companies on u.CompanyId equals c.Company_Id
                          where u.ProjectId == selectedProject && u.IsUpload == true
                          select new ProjectExpensesUploadDetails
                          {
                              CompanyName = c.Company_Name,
                              TransactionID = u.TransactionID,
                              ExpensesTypes = u.HomeOfficeType,
                              EmployeeSupplierName = u.EmployeeSupplierName,
                              ExpenseDate = u.ExpenseDate,
                              Cost = u.Cost,
                              ExpenditureComment = u.ExpenditureComment,
                              ProjectComment = u.ProjectComment,
                              ExpenseUploadID = u.ExpenseUploadID,
                              Completed = u.Completed
                          }).ToList();

            if (hideCompleted)
            {
                if (results.Count > 0)
                {
                    results = results.Where(x => x.Completed == false).ToList();
                }
            }
            else
            {
                results = results.ToList();
            }
            viewModel.ProjectExpensesUploadDetails = results;
            viewModel.HideCompleted = hideCompleted;
            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(viewModel);
        }

        public ActionResult edit(int? id)
        {
            ProjectExpensesUpload projectExpense = Db.ProjectExpensesUploads.Find(id);
            SetViewbag(projectExpense.ProjectId);
            ViewBag.Source = Source.Existing;
            ViewBag.CompanyName = Db.Companies.Where(x => x.Company_Id == projectExpense.CompanyId).Select(x => x.Company_Name).FirstOrDefault();
            return View("Edit", projectExpense);
        }

        [HttpPost]
        public ActionResult CreateEdit(ProjectExpensesUpload projectExpensesUpload)
        {
            ProjectExpensesUpload existing = Db.ProjectExpensesUploads.Find(projectExpensesUpload.ExpenseUploadID);
            if (existing != null)
            {
                existing.ProjectExpTypeID = projectExpensesUpload.ProjectExpTypeID;
                existing.TaskID = projectExpensesUpload.TaskID;
                existing.VariationID = projectExpensesUpload.VariationID;
                existing.ProjectComment = projectExpensesUpload.ProjectComment;
                existing.IsCostRecovery = projectExpensesUpload.IsCostRecovery;
                existing.IsFeeRecovery = projectExpensesUpload.IsFeeRecovery;
                existing.Completed = projectExpensesUpload.Completed;
                existing.AddedBy = UserHelpers.GetCurrentUserId();
                existing.AddedDate = DateTime.UtcNow;
                existing.Traveller = projectExpensesUpload.Traveller;
                Db.ProjectExpensesUploads.AddOrUpdate(existing);
                Db.Entry(existing).State = EntityState.Modified;
            }
            Db.SaveChanges();
            TempData["InfoMessage"] = new InfoMessage { MessageContent = $"Project Expenses Allocation Updated.", MessageType = InfoMessageType.Success };
            return RedirectToAction("Allocation");
        }

        private void SetViewbag(int projectId)
        {
            ViewBag.ProjectID = new SelectList(Db.Projects, "ProjectID", "Name");
            ViewBag.TaskID = new SelectList(GetProjectTaskdetails(projectId), "TaskID", "Name");
            ViewBag.VariationID = new SelectList(GetProjectVariationdetails(projectId), "VariationID", "Description");
            ViewBag.ProjectExpTypeID = new SelectList(GetProjectExpensesTypesdetails(projectId), "ExpenseTypeID", "ExpenseType");
            ViewBag.Traveller = new SelectList(GetProjectUserdetails(projectId), "Id", "Names");
        }
        private List<Employee> GetProjectUserdetails(int projectId)
        {
            var result = from p in Db.EmployeeProjects
                         join e in Db.Users on p.EmployeeId equals e.Id
                         where p.ProjectId == projectId
                         select e;

            var data = result.ToList();
            return data;
        }

        private List<ProjectTask> GetProjectTaskdetails(int projectId)
        {
            return Db.ProjectTasks.Where(x => x.ProjectID == projectId).ToList();
        }

        private List<ProjectVariation> GetProjectVariationdetails(int projectId)
        {
            return Db.ProjectVariations.Where(x => x.ProjectID == projectId).ToList();
        }
        private List<ProjectExpenseType> GetProjectExpensesTypesdetails(int projectId)
        {
            return Db.ProjectExpenseTypes.Where(x => x.ProjectID == projectId).ToList();
        }
    }
}