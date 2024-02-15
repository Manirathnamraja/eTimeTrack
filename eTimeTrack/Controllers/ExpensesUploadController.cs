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
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.Text.RegularExpressions;
using static Spire.Pdf.General.Render.Decode.Jpeg2000.j2k.codestream.HeaderInfo;

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
            };
            ViewBag.CompanyId = new SelectList(Getcompanydetails(), "Company_Id", "Company_Name");
            ViewBag.InfoMessage = TempData["Infomessage"];
            return View(viewModel);
        }

        [HttpPost]
        public JsonResult GetCompanyList(int? projectid)
        {
            if (projectid != null)
            {
                List<Company> company = (from c in Db.Companies
                                         join p in Db.ProjectCompanies on c.Company_Id equals p.CompanyId
                                         where p.ProjectId == projectid
                                         select c).OrderBy(x => x.Company_Name).ToList();

                return Json(company.Select(x => new
                {
                    CompanyId = x.Company_Id,
                    Name = x.Company_Name
                }));
            }
            return null;
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
                            var transactionIDdetails = ws.Cells[i, transactionID].Text?.Trim();
                            string exDate = ws.Cells[i, expenseItemDate].Value?.ToString()?.Trim();
                            var ischeckexdate = CheckDate(exDate);
                            DateTime expenDate = !ischeckexdate ? DateTime.FromOADate(Convert.ToDouble(exDate)) : Convert.ToDateTime(exDate);

                            string costDate = ws.Cells[i, costedInWeekEnding].Value?.ToString()?.Trim();
                            var ischeckcostdate = CheckDate(costDate);
                            DateTime cdate = !ischeckcostdate ? DateTime.FromOADate(Convert.ToDouble(costDate)) : Convert.ToDateTime(costDate);
                            var idevalues = string.Empty;
                            var idet = identifier != 0 ? ws.Cells[i, identifier].Value?.ToString()?.Trim() : null;
                            if (!string.IsNullOrEmpty(model.IdentifierValues) && !string.IsNullOrEmpty(idet))
                            {
                                if (model.IdentifierValues.Contains(";"))
                                {
                                    string[] values = model.IdentifierValues.Split(';');
                                    foreach (var v in values)
                                    {
                                        if (idet.ToLower().Equals(v.ToLower()))
                                        {
                                            idevalues += idet;
                                        }
                                    }
                                }
                                else
                                {
                                    if (model.IdentifierValues.ToLower().Equals(idet.ToLower()))
                                    {
                                        idevalues += idet;
                                    }
                                }
                            }
                            int stdtypeid = 0; int exptypeid = 0;
                            var homeoffics = homeOfficeType != 0 ? ws.Cells[i, homeOfficeType].Value?.ToString()?.Trim() : null;
                            if (!string.IsNullOrEmpty(homeoffics))
                            {
                                stdtypeid = context.ProjectExpensesStdDetails.Where(x => x.StdType.ToLower().Equals(homeoffics.ToLower())).Select(x => x.StdTypeID).FirstOrDefault();
                            }

                            if(stdtypeid != 0)
                            {
                                exptypeid = context.ProjectExpensesMappings.Where(x => x.StdExpTypeID == stdtypeid && x.ProjectID == model.ProjectId).Select(x => x.ProjectTypeID).FirstOrDefault();
                            }

                            var expensestypes = context.ProjectExpenseTypes.Where(x => x.ProjectID == model.ProjectId).FirstOrDefault();

                            bool existexpenses = context.ProjectExpensesUploads.Any(x => x.TransactionID == transactionIDdetails);
                            if (!existexpenses && !string.IsNullOrEmpty(idevalues))
                            {
                                ProjectExpensesUpload expenses = new ProjectExpensesUpload
                                {
                                    TransactionID = transactionIDdetails,
                                    ProjectId = model.ProjectId,
                                    CompanyId = model.CompanyId,
                                    ExpenseDate = expenDate.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                    CostedInWeekEnding = cdate.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                    Cost = cost != 0 ? ws.Cells[i, cost].Value?.ToString()?.Trim() : null,
                                    HomeOfficeType = homeoffics,
                                    EmployeeSupplierName = employeeSupplierName != 0 ? ws.Cells[i, employeeSupplierName].Value?.ToString()?.Trim() : null,
                                    UOM = idevalues,
                                    ExpenditureComment = expenditureComment != 0 ? ws.Cells[i, expenditureComment].Value?.ToString()?.Trim() : null,
                                    InvoiceNumber = invoiceNumber != 0 ? ws.Cells[i, invoiceNumber].Value?.ToString()?.Trim() : null,
                                    AddedBy = userId,
                                    AddedDate = DateTime.UtcNow,
                                    ProjectExpTypeID = exptypeid,
                                    TaskID = expensestypes != null ? expensestypes.TaskID : 0,
                                    VariationID = expensestypes != null ? expensestypes.VariationID : 0,
                                    IsCostRecovery = expensestypes != null ? expensestypes.IsCostRecovery : false,
                                    IsFeeRecovery = expensestypes != null ? expensestypes.IsFeeRecovery : false,
                                    IsUpload = true
                                };
                                expensesUpload.Add(expenses);
                                context.ProjectExpensesUploads.Add(expenses);
                                await context.SaveChangesAsync();
                                insertedRows++;
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
                EmailHelper.SendEmail(email, $"eTimeTrack Expenses uploads failed for {projectName}", "Error: could not upload Expenses data: " + e.Message + ". Please contact an administrator for assistance.");
                TempData["InfoMessage"] = new InfoMessage
                {
                    MessageContent = "Error: could not import user rates data: " + e.Message,
                    MessageType = InfoMessageType.Failure
                };
            }
            string emailText = $"<p> Expenses upload completed for project: <em style=\"color:darkblue\"> {projectName} </em>. </p><ul><li>Total Rows in file: {rowsCount}</li><li style=\"color:darkred\">Invalid Rows: {invalidRowsEmpty}</li><li style=\"color:darkgreen\">Inserted Rows: {insertedRows}</li><li style=\"color:orangered\">Duplicate Rows: {duplicateRows}</li></ul>";

            EmailHelper.SendEmail(email, $"eTimeTrack Expenses uploads succeeded for {projectName}", emailText);
        }

        private bool CheckDate(string date)
        {
            try
            {
                DateTime dt = DateTime.Parse(date);
                return true;
            }
            catch
            {
                return false;
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
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            List<Company> company = (from c in Db.Companies
                                    join p in Db.ProjectCompanies on c.Company_Id equals p.CompanyId
                                    where p.ProjectId == projectId
                                    select c).OrderBy(x => x.Company_Name).ToList();
            return company;
        }
    }
}