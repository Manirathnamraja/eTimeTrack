using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using System.Data.Entity;
using System.IO;
using System.Web.Hosting;
using System.Threading.Tasks;
using Elmah.ContentSyndication;
using OfficeOpenXml;
using System.Globalization;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class ExpensesUploadController : BaseController
    {
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult Index()
        {
            ExpensesUploadViewModel viewModel = new ExpensesUploadViewModel
            {
                ProjectList = GenerateDropdownUserProjects(),
                CompanyList = GetCompany()
            };
            ViewBag.InfoMessage = TempData["Infomessage"];
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult ImportExpensesTemplates(ExpensesUploadViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return InvokeHttp404(HttpContext);
                }
                model.ProjectList = GenerateDropdownUserProjects();
                var projectName = Db.Projects.Find(model.ProjectId).Name;
                ValidateExcelFileImportBasic(model.file);
                string targetFolder = Server.MapPath("~/Content/Upload");
                string targetPath = Path.Combine(targetFolder, model.file.FileName);
                model.file.SaveAs(targetPath);
                ProjectExpensesUpload projectExpensesUpload = new ProjectExpensesUpload()
                {
                    ProjectId = model.ProjectId,
                    CompanyId = model.CompanyId,
                    TransactionID = model.TransactionID,
                    ExpenseDate = model.ExpenseDate,
                    CostedInWeekEnding = model.CostedInWeekEnding,
                    Cost = model.Cost,
                    HomeOfficeType = model.HomeOfficeType,
                    EmployeeSupplierName = model.EmployeeSupplierName,
                    UOM = model.UOM,
                    ExpenditureComment = model.ExpenditureComment,
                    ProjectComment = model.ProjectComment,
                    InvoiceNumber = model.InvoiceNumber,
                    AddedBy = UserHelpers.GetCurrentUserId(),
                    AddedDate = DateTime.UtcNow
                };

                Db.ProjectExpensesUploads.Add(projectExpensesUpload);
                Db.SaveChanges();
                Employee user = UserHelpers.GetCurrentUser();
                HostingEnvironment.QueueBackgroundWorkItem(ct => ProcessXLSFile(model, user.Id, user.Email, projectName));
                TempData["InfoMessage"] = new InfoMessage { MessageContent = "The upload process will run in the background. You will receive an email notification when complete.", MessageType = InfoMessageType.Success };
                ViewBag.InfoMessage = TempData["Infomessage"];
            }
            catch (Exception ex)
            {
                TempData["InfoMessage"] = new InfoMessage
                {
                    MessageContent = "Error: could not import Expenses Upload data: " + ex.Message,
                    MessageType = InfoMessageType.Failure
                };
                ViewBag.InfoMessage = TempData["InfoMessage"];
            }
            return RedirectToAction("Index");
        }

        private async Task ProcessXLSFile(ExpensesUploadViewModel model, int userId, string email, string projectName)
        {
            int invalidRowsEmpty = 0;
            int insertedRows = 0;
            int rowsCount = 0;
            int duplicateRows = 0;
            List<List<int>> invalidRowsNoLinkRowNumbers = new List<List<int>>();
            ApplicationDbContext context = new ApplicationDbContext();
            try
            {
                #region Column Number methods
                int transactionID = ColumnNumber(model.TransactionID);
                int expenseItemDate = ColumnNumber(model.ExpenseDate);
                int costedInWeekEnding = ColumnNumber(model.CostedInWeekEnding);
                int cost = ColumnNumber(model.Cost);
                int homeOfficeType = ColumnNumber(model.HomeOfficeType);
                int employeeSupplierName = ColumnNumber(model.EmployeeSupplierName);
                int identifier = ColumnNumber(model.UOM);
                int expenditureComment = ColumnNumber(model.ExpenditureComment);
                int invoiceNumber = ColumnNumber(model.InvoiceNumber);
                #endregion

                byte[] fileData;
                using (MemoryStream target = new MemoryStream())
                {
                    model.file.InputStream.CopyTo(target);
                    fileData = target.ToArray();
                }
                List<ProjectExpensesUpload> expensesUpload = new List<ProjectExpensesUpload>();
                List<ProjectExpensesUpload> duplicateexpensesUpload = new List<ProjectExpensesUpload>();

                using (MemoryStream stream = new MemoryStream())
                {
                    stream.Write(fileData, 0, fileData.Length);
                    using (ExcelPackage package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet ws = package.Workbook.Worksheets[1];
                        rowsCount = ws.Dimension.Rows - 1;
                        int colsCount = ws.Dimension.Columns;

                        for (int i = 2; i < int.MaxValue; i++) // skip header row
                        {
                            if (string.IsNullOrWhiteSpace(ws.Cells[i, 1].Text))
                            {
                                if (!string.IsNullOrWhiteSpace(ws.Cells[i + 1, 1].Text) || !string.IsNullOrWhiteSpace(ws.Cells[i + 2, 1].Text))
                                {
                                    continue;
                                }
                                break;
                            }
                            CultureInfo provider = CultureInfo.InvariantCulture;
                            //getting project user classificationId from Text.
                            //var projectUserClassificationText = ws.Cells[i, projectUserClassificationColumn].Text?.Trim();
                            //var projectUserClassificationID = context.ProjectUserClassifications.Where(r => r.ProjectClassificationText == projectUserClassificationText).Select(t => t.ProjectUserClassificationId).FirstOrDefault();
                            ////getting employeeId from employee No.
                            //var userIdText = ws.Cells[i, userIDColumn].Text?.Trim();
                            //var employeeId = context.Users.Where(r => r.EmployeeNo == userIdText).Select(t => t.Id).FirstOrDefault();
                            ////getting startDate and EndDate
                            //string sDate = ws.Cells[i, startDateColumn].Value?.ToString()?.Trim();
                            //double date = string.IsNullOrEmpty(sDate) ? 0 : double.Parse(sDate);
                            //DateTime? startdate = (date == 0) ? (DateTime?)null : Convert.ToDateTime(DateTime.FromOADate(date).ToString("MMMM dd, yyyy"));

                            //string eDate = ws.Cells[i, endDateColumn].Value?.ToString()?.Trim();
                            //double date2 = string.IsNullOrEmpty(eDate) ? 0 : double.Parse(eDate);
                            //DateTime? endDate = (date2 == 0) ? (DateTime?)null : Convert.ToDateTime(DateTime.FromOADate(date2).ToString("MMMM dd, yyyy"));

                            //var IsRatesConfirmedBool = ParseBool(ws.Cells[i, ratesConfirmedColumn].Text?.Trim());
                            var transactionIDdetails = ws.Cells[i, 123].Text?.Trim();

                            bool existexpenses = context.ProjectExpensesUploads.Any(x => x.TransactionID == transactionIDdetails);
                            if (!existexpenses)
                            {
                                ProjectExpensesUpload expenses = new ProjectExpensesUpload
                                {
                                    TransactionID = transactionIDdetails,
                                    ProjectId = model.ProjectId,
                                    CompanyId = model.CompanyId,
                                    ExpenseDate = expenseItemDate != 0 ? ws.Cells[i, expenseItemDate].Value?.ToString()?.Trim() : null,
                                    CostedInWeekEnding = costedInWeekEnding != 0 ? ws.Cells[i, costedInWeekEnding].Value?.ToString()?.Trim() : null,
                                    Cost = cost != 0 ? ws.Cells[i, cost].Value?.ToString()?.Trim() : null,
                                    HomeOfficeType = homeOfficeType != 0 ? ws.Cells[i, homeOfficeType].Value?.ToString()?.Trim() : null,
                                    EmployeeSupplierName = employeeSupplierName != 0 ? ws.Cells[i, employeeSupplierName].Value?.ToString()?.Trim() : null,
                                    UOM = identifier != 0 ? ws.Cells[i, identifier].Value?.ToString()?.Trim() : null,
                                    ExpenditureComment = expenditureComment != 0 ? ws.Cells[i, expenditureComment].Value?.ToString()?.Trim() : null,
                                    InvoiceNumber = invoiceNumber != 0 ? ws.Cells[i, invoiceNumber].Value?.ToString()?.Trim() : null,
                                    AddedBy = UserHelpers.GetCurrentUserId(),
                                    AddedDate = DateTime.UtcNow
                                };
                                expensesUpload.Add(expenses);
                            }
                            else
                            {
                                duplicateRows++;
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                TempData["InfoMessage"] = new InfoMessage
                {
                    MessageContent = "Error: could not import user rates data: " + e.Message,
                    MessageType = InfoMessageType.Failure
                };
            }
        }

        public static int ColumnNumber(string colAddress)
        {
            if (colAddress == null)
            {
                return 0;
            }
            string colAddressUpper = colAddress.ToUpper();
            int[] digits = new int[colAddressUpper.Length];
            for (int i = 0; i < colAddressUpper.Length; ++i)
            {
                digits[i] = Convert.ToInt32(colAddressUpper[i]) - 64;
            }
            int mul = 1; int res = 0;
            for (int pos = digits.Length - 1; pos >= 0; --pos)
            {
                res += digits[pos] * mul;
                mul *= 26;
            }
            return res;
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