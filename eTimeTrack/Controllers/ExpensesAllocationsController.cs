﻿using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using System;
using System.Collections.Generic;
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
                          join e in Db.ProjectExpensesStdDetails on u.ProjectExpTypeID equals e.StdTypeID
                          where u.ProjectId == selectedProject
                          select new ProjectExpensesUploadDetails
                          {
                              CompanyName = c.Company_Name,
                              TransactionID = u.TransactionID,
                              ExpensesTypes = e.StdType,
                              EmployeeSupplierName = u.EmployeeSupplierName,
                              ExpenseDate = u.ExpenseDate,
                              Cost = u.Cost,
                              ExpenditureComment = u.ExpenditureComment,
                              ExpenseUploadID = u.ExpenseUploadID
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
                existing.ExpenditureComment = projectExpensesUpload.ExpenditureComment;
                existing.IsCostRecovery = projectExpensesUpload.IsCostRecovery;
                existing.IsFeeRecovery = projectExpensesUpload.IsFeeRecovery;
                existing.Completed = projectExpensesUpload.Completed;
                existing.AddedBy = UserHelpers.GetCurrentUserId();
                existing.AddedDate = DateTime.UtcNow;
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