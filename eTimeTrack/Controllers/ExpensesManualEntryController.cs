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
    public class ExpensesManualEntryController : BaseController
    {
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult Index()
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            if (selectedProject == 0) { return InvokeHttp404(HttpContext); }
            ExpensesUploadViewModel viewModel = new ExpensesUploadViewModel
            {
                CompanyList = GetCompany()
            };
            ViewBag.InfoMessage = TempData["Infomessage"];
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult SaveData(ProjectExpensesUpload model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return InvokeHttp404(HttpContext);
                }
                ProjectExpensesUpload projectExpensesUpload = new ProjectExpensesUpload()
                {
                    ProjectId = model.ProjectId,
                    CompanyId = model.CompanyId,
                    TransactionID = model.TransactionID,
                    ExpenseDate =  Convert.ToDateTime(model.ExpenseDate).ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    CostedInWeekEnding = Convert.ToDateTime(model.CostedInWeekEnding).ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    Cost = model.Cost,
                    HomeOfficeType = model.HomeOfficeType,
                    EmployeeSupplierName = model.EmployeeSupplierName,
                    ExpenditureComment = model.ExpenditureComment,
                    ProjectComment = model.ProjectComment,
                    InvoiceNumber = model.InvoiceNumber,
                    AddedBy = UserHelpers.GetCurrentUserId(),
                    AddedDate = DateTime.UtcNow,
                    IsUpload = true
                };
                Db.ProjectExpensesUploads.Add(projectExpensesUpload);
                Db.SaveChanges();
                TempData["InfoMessage"] = new InfoMessage { MessageContent = "Expenses Manual Entry data Successfully Saved.", MessageType = InfoMessageType.Success };
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        private SelectList GetCompany()
        {
            return new SelectList(Getcompanydetails(), "Company_Id", "Company_Name", 1);
        }
        private List<Company> Getcompanydetails()
        {
            List<Company> company = Db.Companies.Join(Db.ProjectCompanies, c => c.Company_Id, p => p.CompanyId, (c, p) => c).Distinct().ToList();
            return company;
        }
    }
}