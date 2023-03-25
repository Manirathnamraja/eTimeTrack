using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using System;
using System.Web.UI.WebControls;
using EntityState = System.Data.Entity.EntityState;
using System.Web;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Xml;
using OfficeOpenXml;
using System.Globalization;
using System.Web.Hosting;
using System.Threading.Tasks;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class UserRatesController : BaseController
    {

        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult Index()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }
            List<UserRate> userRates = GetAllUserRatesOrdered(projectId).Where(x => !x.Employee.LockoutEndDateUtc.HasValue && x.Employee.IsActive).ToList();

            List<EmployeeProject> employeeProjects = GetAllProjectEmployeesOrdered(projectId).Where(x => !x.Employee.LockoutEndDateUtc.HasValue && x.Employee.IsActive).ToList();

            ViewBag.ProjectId = projectId;

            UserRateIndexUserVm vm = new UserRateIndexUserVm
            {
                EmployeeProjects = employeeProjects,

            };
            return View(vm);
        }


        public ActionResult ImportRatesTemplates()
        {
            UserRatesUploadCreateViewModel viewModel = new UserRatesUploadCreateViewModel { ProjectList = GenerateDropdownUserProjects() };
            return View(viewModel);
        }        

        [HttpPost]
        public ActionResult ImportRatesTemplates(UserRatesUploadCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return InvokeHttp404(HttpContext);
                }
                model.ProjectList = GenerateDropdownUserProjects();
                var projectName = Db.Projects.Find(model.ProjectID).Name;

                //int projectId = (int?)Session["SelectedProject"] ?? 0;
                //Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();


                //if (project == null)
                //{
                //    return InvokeHttp404(HttpContext);
                //}
                ValidateExcelFileImportBasic(model.file);

                string targetFolder = Server.MapPath("~/Content/Upload");
                string targetPath = Path.Combine(targetFolder, model.file.FileName);
                model.file.SaveAs(targetPath);

                //string path = Server.MapPath("~/Content/Upload/" + model.file.FileName);
                //  string path = Server.MapPath(Path.Combine("~/Content/Upload", model.file.FileName));
                //   Directory.CreateDirectory(path);
                //    model.file.SaveAs(path);

                //   UserRatesUpload userRateUpload = Db.UserRatesUploads.SingleOrDefault(x => x.ProjectId == project.ProjectID);

              //  InfoMessage message;
                UserRatesUpload userRatesUpload = new UserRatesUpload()
                {
                    ProjectId = model.ProjectID,
                    ProjectUserClassificationIDColumn = model.ProjectUserClassification,
                    UserIDColumn = model.UserID,
                    StartDateColumn = model.StartDate,
                    EndDateColumn = model.EndDate,
                    IsRatesConfirmedColumn = model.IsRatesConfirmed,
                    //Fee Rates and Cost Rates
                    NTFeeRateColumn = model.NTFeeRate,
                    NTCostRateColumn = model.NTCostRate,
                    OT1FeeRateColumn = model.OT1FeeRate,
                    OT1CostRateColumn = model.OT1CostRate,
                    OT2FeeRateColumn = model.OT2FeeRate,
                    OT2CostRateColumn = model.OT2CostRate,
                    OT3FeeRateColumn = model.OT3FeeRate,
                    OT3CostRateColumn = model.OT3CostRate,
                    OT4FeeRateColumn = model.OT4FeeRate,
                    OT4CostRateColumn = model.OT4CostRate,
                    OT5FeeRateColumn = model.OT5FeeRate,
                    OT5CostRateColumn = model.OT5CostRate,
                    OT6FeeRateColumn = model.OT6FeeRate,
                    OT6CostRateColumn = model.OT6CostRate,
                    OT7FeeRateColumn = model.OT7FeeRate,
                    OT7CostRateColumn = model.OT7CostRate,
                    FilePath = model.file.FileName,
                    AddedBy = UserHelpers.GetCurrentUserId(),
                    AddedDate = DateTime.Now,
                };

                Db.UserRatesUploads.Add(userRatesUpload);
                Db.SaveChanges();

                Employee user = UserHelpers.GetCurrentUser();
              //  ProcessXLSFile(model, user.Email, project);
                HostingEnvironment.QueueBackgroundWorkItem(ct => ProcessXLSFile(model,user.Id, user.Email, projectName));

                TempData["InfoMessage"] = new InfoMessage { MessageContent = "The upload process will run in the background. You will receive an email notification when complete.", MessageType = InfoMessageType.Success };

                //TempData["Infomessage"] = new InfoMessage
                //{
                //    MessageType = InfoMessageType.Success,
                //    MessageContent = "Successfully uploaded User Rates."
                //};
                ViewBag.InfoMessage = TempData["Infomessage"];


            }
            catch (Exception ex)
            {
                // throw ex;
                TempData["InfoMessage"] = new InfoMessage
                {
                    MessageContent = "Error: could not import user rates data: " + ex.Message,
                    MessageType = InfoMessageType.Failure
                };
                ViewBag.InfoMessage = TempData["InfoMessage"];
            }
            return View(model);
        }

        private async Task ProcessXLSFile(UserRatesUploadCreateViewModel model,int userId, string email, string projectName)
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
                int userIDColumn = ColumnNumber(model.UserID);
                int startDateColumn = ColumnNumber(model.StartDate);
                int endDateColumn = ColumnNumber(model.EndDate);
                int projectUserClassificationColumn = ColumnNumber(model.ProjectUserClassification);
                int ratesConfirmedColumn = ColumnNumber(model.IsRatesConfirmed);
                int ntFeeRateColumn = ColumnNumber(model.NTFeeRate);
                int ot1FeeRateColumn = ColumnNumber(model.OT1FeeRate);
                int ot2FeeRateColumn = ColumnNumber(model.OT2FeeRate);
                int ot3FeeRateColumn = ColumnNumber(model.OT3FeeRate);
                int ot4FeeRateColumn = ColumnNumber(model.OT4FeeRate);
                int ot5FeeRateColumn = ColumnNumber(model.OT5FeeRate);
                int ot6FeeRateColumn = ColumnNumber(model.OT6FeeRate);
                int ot7FeeRateColumn = ColumnNumber(model.OT7FeeRate);
                int ntCostRateColumn = ColumnNumber(model.NTCostRate);
                int ot1CostRateColumn = ColumnNumber(model.OT1CostRate);
                int ot2CostRateColumn = ColumnNumber(model.OT2CostRate);
                int ot3CostRateColumn = ColumnNumber(model.OT3CostRate);
                int ot4CostRateColumn = ColumnNumber(model.OT4CostRate);
                int ot5CostRateColumn = ColumnNumber(model.OT5CostRate);
                int ot6CostRateColumn = ColumnNumber(model.OT6CostRate);
                int ot7CostRateColumn = ColumnNumber(model.OT7CostRate);
                #endregion

                byte[] fileData;
                using (MemoryStream target = new MemoryStream())
                {
                    model.file.InputStream.CopyTo(target);
                    fileData = target.ToArray();
                }

                List<UserRate> userRates = new List<UserRate>();
                List<UserRate> duplicateuserRates = new List<UserRate>();

                //get data from excel
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
                            var projectUserClassificationText = ws.Cells[i, projectUserClassificationColumn].Text?.Trim();
                            var projectUserClassificationID = context.ProjectUserClassifications.Where(r => r.ProjectClassificationText == projectUserClassificationText).Select(t => t.ProjectUserClassificationId).FirstOrDefault();
                            //getting employeeId from employee No.
                            var userIdText = ws.Cells[i, userIDColumn].Text?.Trim();
                            var employeeId = context.Users.Where(r => r.EmployeeNo == userIdText).Select(t => t.Id).FirstOrDefault();
                            //getting startDate and EndDate
                            string sDate = ws.Cells[i, startDateColumn].Value?.ToString()?.Trim();
                            double date = string.IsNullOrEmpty(sDate) ? 0 : double.Parse(sDate);
                            DateTime? startdate = (date == 0) ? (DateTime?)null : Convert.ToDateTime(DateTime.FromOADate(date).ToString("MMMM dd, yyyy"));

                            string eDate = ws.Cells[i, endDateColumn].Value?.ToString()?.Trim();
                            double date2 = string.IsNullOrEmpty(eDate) ? 0 : double.Parse(eDate);
                            DateTime? endDate = (date2 == 0) ? (DateTime?)null : Convert.ToDateTime(DateTime.FromOADate(date2).ToString("MMMM dd, yyyy"));

                            var IsRatesConfirmedBool = ParseBool(ws.Cells[i, ratesConfirmedColumn].Text?.Trim());

                            bool existUserRates = context.UserRates.Any(x => x.EmployeeId == employeeId && x.StartDate == startdate);
                            if (!existUserRates)
                            {
                                UserRate userRate = new UserRate
                                {
                                    EmployeeId = employeeId,
                                    StartDate = startdate,
                                    EndDate = endDate,
                                    ProjectUserClassificationID = projectUserClassificationID,
                                    IsRatesConfirmed = IsRatesConfirmedBool == "true" ? true : false,

                                    NTFeeRate = ntFeeRateColumn != 0 ? ws.Cells[i, ntFeeRateColumn].Value?.ToString()?.Trim() : null,
                                    OT1FeeRate = ot1FeeRateColumn != 0 ? ws.Cells[i, ot1FeeRateColumn].Value?.ToString()?.Trim() : null,
                                    OT2FeeRate = ot2FeeRateColumn != 0 ? ws.Cells[i, ot2FeeRateColumn].Value?.ToString()?.Trim() : null,
                                    OT3FeeRate = ot3FeeRateColumn != 0 ? ws.Cells[i, ot3FeeRateColumn].Value?.ToString()?.Trim() : null,
                                    OT4FeeRate = ot4FeeRateColumn != 0 ? ws.Cells[i, ot4FeeRateColumn].Value?.ToString()?.Trim() : null,
                                    OT5FeeRate = ot5FeeRateColumn != 0 ? ws.Cells[i, ot5FeeRateColumn].Value?.ToString()?.Trim() : null,
                                    OT6FeeRate = ot6FeeRateColumn != 0 ? ws.Cells[i, ot6FeeRateColumn].Value?.ToString()?.Trim() : null,
                                    OT7FeeRate = ot7FeeRateColumn != 0 ? ws.Cells[i, ot7FeeRateColumn].Value?.ToString()?.Trim() : null,
                                    NTCostRate = ntCostRateColumn != 0 ? ws.Cells[i, ntCostRateColumn].Value?.ToString()?.Trim() : null,
                                    OT1CostRate = ot1CostRateColumn != 0 ? ws.Cells[i, ot1CostRateColumn].Value?.ToString()?.Trim() : null,
                                    OT2CostRate = ot2CostRateColumn != 0 ? ws.Cells[i, ot2CostRateColumn].Value?.ToString()?.Trim() : null,
                                    OT3CostRate = ot3CostRateColumn != 0 ? ws.Cells[i, ot3CostRateColumn].Value?.ToString()?.Trim() : null,
                                    OT4CostRate = ot4CostRateColumn != 0 ? ws.Cells[i, ot4CostRateColumn].Value?.ToString()?.Trim() : null,
                                    OT5CostRate = ot5CostRateColumn != 0 ? ws.Cells[i, ot5CostRateColumn].Value?.ToString()?.Trim() : null,
                                    OT6CostRate = ot6CostRateColumn != 0 ? ws.Cells[i, ot6CostRateColumn].Value?.ToString()?.Trim() : null,
                                    OT7CostRate = ot7CostRateColumn != 0 ? ws.Cells[i, ot7CostRateColumn].Value?.ToString()?.Trim() : null,

                                    ProjectId = model.ProjectID,
                                    LastModifiedBy = UserHelpers.GetCurrentUserId() + "-IMP",
                                    LastModifiedDate = DateTime.Now,
                                    IsDeleted = false,
                                };
                                userRates.Add(userRate);

                                //validations
                                if (string.IsNullOrEmpty(userRate.EmployeeId.ToString()) ||
                                    userRate.EmployeeId == 0 ||
                                    string.IsNullOrWhiteSpace(userRate.EndDate.ToString()) ||
                                    string.IsNullOrWhiteSpace(userRate.StartDate.ToString()) ||
                                    userRate.ProjectUserClassificationID == 0 ||
                                    string.IsNullOrEmpty(IsRatesConfirmedBool)
                                    )
                                {
                                    invalidRowsEmpty++;
                                }


                                else
                                {
                                    var rate = userRates.Where(x => x.EmployeeId == employeeId);
                                    foreach (var data in rate)
                                    {
                                        if (!duplicateuserRates.Exists(x => x.EmployeeId == data.EmployeeId && x.EndDate >= data.StartDate))
                                        {
                                            duplicateuserRates.Add(data);

                                            context.UserRates.Add(data);
                                            //context.SaveChangesWithChangelog(userId);
                                            await context.SaveChangesAsync();
                                            insertedRows++;
                                        }
                                    }
                                }
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
                EmailHelper.SendEmail(email, $"eTimeTrack User Rates uploads failed for {projectName}", "Error: could not upload user rates data: " + e.Message + ". Please contact an administrator for assistance.");
                TempData["InfoMessage"] = new InfoMessage
                {
                    MessageContent = "Error: could not import user rates data: " + e.Message,
                    MessageType = InfoMessageType.Failure
                };

              //  return false;
            }

            string emailText = $"<p> User Rates upload completed for project: <em style=\"color:darkblue\"> {projectName} </em>. </p><ul><li>Total Rows in file: {rowsCount}</li><li style=\"color:darkred\">Invalid Rows: {invalidRowsEmpty}</li><li style=\"color:darkgreen\">Inserted Rows: {insertedRows}</li><li style=\"color:orangered\">Duplicate Rows: {duplicateRows}</li></ul>";

             EmailHelper.SendEmail(email, $"eTimeTrack User Rates uploads succeeded for {projectName}", emailText);

          //  return true;

        }

        public string ParseBool(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                switch (input.ToLower())
                {
                    case "y":
                    case "yes":
                        return "true";
                    case "n":
                    case "no":
                        return "false";

                }
            }
            return string.Empty;
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


        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult Details(int? employeeId)
        {
            if (employeeId == null)
            {
                return InvokeHttp400(HttpContext);
            }

            Employee employee = Db.Users.Find(employeeId);
            if (employee == null)
            {
                return InvokeHttp404(HttpContext);
            }

            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            UserRate userRate = Db.UserRates.SingleOrDefault(x => x.EmployeeId == employeeId && x.ProjectId == project.ProjectID);

            UserRateDetailsViewModel model = new UserRateDetailsViewModel();

            List<SelectListItem> selectItems = GetProjectUserClassificationSelectItems(projectId);

            SelectList availableProjectUserClassifications = new SelectList(selectItems, "Value", "Text", model.ProjectUserClassificationID);

            ViewBag.AvailableProjectUserClassifications = availableProjectUserClassifications;

            if (userRate != null)
            {
                model.EmployeeID = employee.Id;
                model.EmployeeNo = employee.EmployeeNo;
                model.EmailAddress = employee.Email;
                model.Names = employee.Names;
                model.StartDate = userRate.StartDate;
                model.EndDate = userRate.EndDate;
                model.ProjectUserClassificationID = userRate?.ProjectUserClassificationID?.ToString();
                model.ProjectID = projectId;

                model.IsRatesConfirmed = userRate.IsRatesConfirmed;
                //Fee Rates and Cost Rates
                model.NTFeeRate = userRate?.NTFeeRate;
                model.NTCostRate = userRate?.NTCostRate;
                model.OT1FeeRate = userRate?.OT1FeeRate;
                model.OT1CostRate = userRate?.OT1CostRate;
                model.OT2FeeRate = userRate?.OT2FeeRate;
                model.OT2CostRate = userRate?.OT2CostRate;
                model.OT3FeeRate = userRate?.OT3FeeRate;
                model.OT3CostRate = userRate?.OT3CostRate;
                model.OT4FeeRate = userRate?.OT4FeeRate;
                model.OT4CostRate = userRate?.OT4CostRate;
                model.OT5FeeRate = userRate?.OT5FeeRate;
                model.OT5CostRate = userRate?.OT5CostRate;
                model.OT6FeeRate = userRate?.OT6FeeRate;
                model.OT6CostRate = userRate?.OT6CostRate;
                model.OT7FeeRate = userRate?.OT7FeeRate;
                model.OT7CostRate = userRate?.OT7CostRate;

            }

            else
            {
                model.EmployeeID = employee.Id;
                model.EmployeeNo = employee.EmployeeNo;
                model.EmailAddress = employee.Email;
                model.Names = employee.Names;
                model.ProjectID = projectId;
                model.IsRatesConfirmed = false;
                model.StartDate = null;
                model.EndDate = null;
                //Fee Rates and Cost Rates
                model.NTFeeRate = userRate?.NTFeeRate;
                model.NTCostRate = userRate?.NTCostRate;
                model.OT1FeeRate = userRate?.OT1FeeRate;
                model.OT1CostRate = userRate?.OT1CostRate;
                model.OT2FeeRate = userRate?.OT2FeeRate;
                model.OT2CostRate = userRate?.OT2CostRate;
                model.OT3FeeRate = userRate?.OT3FeeRate;
                model.OT3CostRate = userRate?.OT3CostRate;
                model.OT4FeeRate = userRate?.OT4FeeRate;
                model.OT4CostRate = userRate?.OT4CostRate;
                model.OT5FeeRate = userRate?.OT5FeeRate;
                model.OT5CostRate = userRate?.OT5CostRate;
                model.OT6FeeRate = userRate?.OT6FeeRate;
                model.OT6CostRate = userRate?.OT6CostRate;
                model.OT7FeeRate = userRate?.OT7FeeRate;
                model.OT7CostRate = userRate?.OT7CostRate;
            }

            return PartialView(model);
        }

        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult EditUserRates(int? employeeId)
        {
            if (employeeId == null)
            {
                return InvokeHttp400(HttpContext);
            }

            Employee employee = Db.Users.Find(employeeId);
            if (employee == null)
            {
                return InvokeHttp404(HttpContext);
            }

            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            UserRate userRate = Db.UserRates.SingleOrDefault(x => x.EmployeeId == employeeId && x.ProjectId == project.ProjectID);

            UserRateDetailsViewModel model = new UserRateDetailsViewModel();

            List<SelectListItem> selectItems = GetProjectUserClassificationSelectItems(projectId);

            SelectList availableProjectUserClassifications = new SelectList(selectItems, "Value", "Text", model.ProjectUserClassificationID);

            ViewBag.AvailableProjectUserClassifications = availableProjectUserClassifications;

            if (userRate != null)
            {
                model.EmployeeID = employee.Id;
                model.EmployeeNo = employee.EmployeeNo;
                model.EmailAddress = employee.Email;
                model.Names = employee.Names;
                model.StartDate = userRate.StartDate;
                model.EndDate = userRate.EndDate;


                var selected = availableProjectUserClassifications.Where(x => x.Value == "selectedValue").First();
                selected.Selected = true;
                model.ProjectUserClassificationSelectedValue = selected.Text;

                model.ProjectUserClassificationID = userRate?.ProjectUserClassificationID?.ToString();
                model.ProjectID = projectId;
                model.ProjectUserClassifications = selectItems;

                model.IsRatesConfirmed = userRate.IsRatesConfirmed;
                //Fee Rates and Cost Rates
                model.NTFeeRate = userRate?.NTFeeRate;
                model.NTCostRate = userRate?.NTCostRate;
                model.OT1FeeRate = userRate?.OT1FeeRate;
                model.OT1CostRate = userRate?.OT1CostRate;
                model.OT2FeeRate = userRate?.OT2FeeRate;
                model.OT2CostRate = userRate?.OT2CostRate;
                model.OT3FeeRate = userRate?.OT3FeeRate;
                model.OT3CostRate = userRate?.OT3CostRate;
                model.OT4FeeRate = userRate?.OT4FeeRate;
                model.OT4CostRate = userRate?.OT4CostRate;
                model.OT5FeeRate = userRate?.OT5FeeRate;
                model.OT5CostRate = userRate?.OT5CostRate;
                model.OT6FeeRate = userRate?.OT6FeeRate;
                model.OT6CostRate = userRate?.OT6CostRate;
                model.OT7FeeRate = userRate?.OT7FeeRate;
                model.OT7CostRate = userRate?.OT7CostRate;

            }

            else
            {
                model.EmployeeID = employee.Id;
                model.EmployeeNo = employee.EmployeeNo;
                model.EmailAddress = employee.Email;
                model.Names = employee.Names;
                model.ProjectUserClassifications = selectItems;
                model.ProjectID = projectId;
                model.ProjectUserClassifications = selectItems;
                model.IsRatesConfirmed = false;
                model.StartDate = null;
                model.EndDate = null;
                //Fee Rates and Cost Rates
                model.NTFeeRate = userRate?.NTFeeRate;
                model.NTCostRate = userRate?.NTCostRate;
                model.OT1FeeRate = userRate?.OT1FeeRate;
                model.OT1CostRate = userRate?.OT1CostRate;
                model.OT2FeeRate = userRate?.OT2FeeRate;
                model.OT2CostRate = userRate?.OT2CostRate;
                model.OT3FeeRate = userRate?.OT3FeeRate;
                model.OT3CostRate = userRate?.OT3CostRate;
                model.OT4FeeRate = userRate?.OT4FeeRate;
                model.OT4CostRate = userRate?.OT4CostRate;
                model.OT5FeeRate = userRate?.OT5FeeRate;
                model.OT5CostRate = userRate?.OT5CostRate;
                model.OT6FeeRate = userRate?.OT6FeeRate;
                model.OT6CostRate = userRate?.OT6CostRate;
                model.OT7FeeRate = userRate?.OT7FeeRate;
                model.OT7CostRate = userRate?.OT7CostRate;
            }

            return View(model);
        }

        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult EditUserRates2(int? employeeId)
        {
            if (employeeId == null)
            {
                return InvokeHttp400(HttpContext);
            }

            Employee employee = Db.Users.Find(employeeId);
            if (employee == null)
            {
                return InvokeHttp404(HttpContext);
            }

            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            List<UserRate> userRates = Db.UserRates.Where(x => x.EmployeeId == employeeId && x.ProjectId == project.ProjectID).ToList();
            //UserRate userRate = Db.UserRates.SingleOrDefault(x => x.EmployeeId == employeeId && x.ProjectId == project.ProjectID);

            // UserRateDetailsViewModel model = new UserRateDetailsViewModel();

            List<SelectListItem> selectItems = GetProjectUserClassificationSelectItems(projectId);

            //SelectList availableProjectUserClassifications = new SelectList(selectItems, "Value", "Text", model.ProjectUserClassificationID);
            SelectList availableProjectUserClassifications = new SelectList(selectItems, "Value", "Text");

            ViewBag.AvailableProjectUserClassifications = availableProjectUserClassifications;
            ViewBag.EmployeeId = employeeId;
            ViewBag.ProjectId = projectId;

            //if (userRates.Count != 0)
            //{
            //    foreach (var item in userRates)
            //    {
            //        model.EmployeeID = employee.Id;
            //        model.EmployeeNo = employee.EmployeeNo;
            //        model.EmailAddress = employee.Email;
            //        model.Names = employee.Names;
            //        model.StartDate = item.StartDate;
            //        model.EndDate = item.EndDate;
            //        model.ProjectUserClassificationID = item?.ProjectUserClassificationID?.ToString();
            //        model.ProjectID = projectId;
            //        model.IsRatesConfirmed = item.IsRatesConfirmed;
            //        //Fee Rates and Cost Rates
            //        model.NTFeeRate = item?.NTFeeRate;
            //        model.NTCostRate = item?.NTCostRate;
            //        model.OT1FeeRate = item?.OT1FeeRate;
            //        model.OT1CostRate = item?.OT1CostRate;
            //        model.OT2FeeRate = item?.OT2FeeRate;
            //        model.OT2CostRate = item?.OT2CostRate;
            //        model.OT3FeeRate = item?.OT3FeeRate;
            //        model.OT3CostRate = item?.OT3CostRate;
            //        model.OT4FeeRate = item?.OT4FeeRate;
            //        model.OT4CostRate = item?.OT4CostRate;
            //        model.OT5FeeRate = item?.OT5FeeRate;
            //        model.OT5CostRate = item?.OT5CostRate;
            //        model.OT6FeeRate = item?.OT6FeeRate;
            //        model.OT6CostRate = item?.OT6CostRate;
            //        model.OT7FeeRate = item?.OT7FeeRate;
            //        model.OT7CostRate = item?.OT7CostRate;
            //    }

            //}

            //else
            //{
            //    foreach (var item in userRates)
            //    {
            //        model.EmployeeID = employee.Id;
            //        model.EmployeeNo = employee.EmployeeNo;
            //        model.EmailAddress = employee.Email;
            //        model.Names = employee.Names;
            //        model.ProjectUserClassifications = selectItems;
            //        model.ProjectID = projectId;
            //        model.IsRatesConfirmed = false;
            //        model.StartDate = null;
            //        model.EndDate = null;
            //        //Fee Rates and Cost Rates
            //        model.NTFeeRate = item?.NTFeeRate;
            //        model.NTCostRate = item?.NTCostRate;
            //        model.OT1FeeRate = item?.OT1FeeRate;
            //        model.OT1CostRate = item?.OT1CostRate;
            //        model.OT2FeeRate = item?.OT2FeeRate;
            //        model.OT2CostRate = item?.OT2CostRate;
            //        model.OT3FeeRate = item?.OT3FeeRate;
            //        model.OT3CostRate = item?.OT3CostRate;
            //        model.OT4FeeRate = item?.OT4FeeRate;
            //        model.OT4CostRate = item?.OT4CostRate;
            //        model.OT5FeeRate = item?.OT5FeeRate;
            //        model.OT5CostRate = item?.OT5CostRate;
            //        model.OT6FeeRate = item?.OT6FeeRate;
            //        model.OT6CostRate = item?.OT6CostRate;
            //        model.OT7FeeRate = item?.OT7FeeRate;
            //        model.OT7CostRate = item?.OT7CostRate;
            //    }
            //}

            return View(userRates);
        }

        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult EditUserRates3(int? employeeId)
        {
            if (employeeId == null)
            {
                return InvokeHttp400(HttpContext);
            }

            Employee employee = Db.Users.Find(employeeId);
            if (employee == null)
            {
                return InvokeHttp404(HttpContext);
            }

            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            List<UserRate> userRates = Db.UserRates.Where(x => x.EmployeeId == employeeId && x.ProjectId == project.ProjectID && x.IsDeleted == false).ToList();


            List<SelectListItem> selectItems = GetProjectUserClassificationSelectItems(projectId);

            SelectList availableProjectUserClassifications = new SelectList(selectItems, "Value", "Text");

            ViewBag.AvailableProjectUserClassifications = availableProjectUserClassifications;
            ViewBag.EmployeeId = employeeId;
            ViewBag.ProjectId = projectId;
            ViewBag.EmployeeNo = employee.EmployeeNo;
            ViewBag.EmployeeName = employee.Names;
            // ViewBag.UserRateId = userRateId;
            return View(userRates);
        }

        public ActionResult Create(int? projectId, int? employeeId)
        {
            Employee employee = Db.Users.Find(employeeId);
            if (employee == null)
            {
                return InvokeHttp404(HttpContext);
            }
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();
            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }
            UserRate userRate = Db.UserRates.Find(projectId);

            List<SelectListItem> selectItems = GetProjectUserClassificationSelectItems(projectId);

            SelectList availableProjectUserClassifications = new SelectList(selectItems, "Value", "Text");

            ViewBag.AvailableProjectUserClassifications = availableProjectUserClassifications;
            ViewBag.EmployeeId = employeeId;
            //ViewBag.EmployeeId = employeeId;
            ViewBag.ProjectId = projectId;
            ViewBag.EmployeeNo = employee.EmployeeNo;
            ViewBag.EmployeeName = employee.Names;
            ViewBag.userRateId = userRate?.UserRateId;
            return View(userRate);
        }

        public ActionResult Edit(int userRateId, int? employeeId)
        {
            UserRate userRate = Db.UserRates.Find(userRateId);

            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();
            Employee employee = Db.Users.Find(employeeId);
            if (employee == null)
            {
                return InvokeHttp404(HttpContext);
            }
            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            List<SelectListItem> selectItems = GetProjectUserClassificationSelectItems(projectId);

            SelectList availableProjectUserClassifications = new SelectList(selectItems, "Value", "Text");

            ViewBag.AvailableProjectUserClassifications = availableProjectUserClassifications;
            ViewBag.EmployeeId = employeeId;
            ViewBag.EmployeeNo = employee.EmployeeNo;
            ViewBag.EmployeeName = employee.Names;
            ViewBag.ProjectId = projectId;
            ViewBag.UserRateId = userRateId;

            //return View("Create", userRate);
            return View("Create", userRate);
        }

        [HttpPost]
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public JsonResult Save(UserRate userRate)
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return Json(false);
            }

            UserRate existing = Db.UserRates.Find(userRate.UserRateId);

            if (existing != null)
            {
                Db.Entry(existing).CurrentValues.SetValues(userRate);
                Db.Entry(existing).State = EntityState.Modified;

                // Db.Entry(existing).Property(u => u.EmployeeId).IsModified = false;

                userRate.LastModifiedBy = UserHelpers.GetCurrentUserId().ToString();
                userRate.LastModifiedDate = DateTime.Now;
            }
            else
            {
                userRate.LastModifiedBy = UserHelpers.GetCurrentUserId().ToString();
                userRate.LastModifiedDate = DateTime.Now;
                userRate.IsDeleted = false;
                Db.UserRates.Add(userRate);
            }

            Db.SaveChanges();

            //    int insertedRecords = Db.SaveChanges();
            int insertedRecords = 0;
            return Json(insertedRecords);
        }

        public JsonResult Remove(int userRateId)
        {
            UserRate userRate = Db.UserRates.Find(userRateId);

            //Db.UserRates.Remove(userRate);
            userRate.IsDeleted = true;
            Db.SaveChanges();
            //return Json(userRate);
            return Json(new { Success = true });
        }

        [HttpPost]
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public JsonResult Details(UserRateDetailsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(false);
            }
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return Json(false);
            }

            UserRate userRate = Db.UserRates.SingleOrDefault(x => x.EmployeeId == model.EmployeeID && x.ProjectId == project.ProjectID);

            InfoMessage message;
            if (userRate != null)
            {
                userRate.EmployeeId = model.EmployeeID;
                userRate.ProjectId = projectId;
                userRate.ProjectUserClassificationID = Convert.ToInt32(model.ProjectUserClassificationID);
                userRate.StartDate = model.StartDate;
                userRate.EndDate = model.EndDate;
                userRate.IsRatesConfirmed = model.IsRatesConfirmed;
                //Fee Rates and Cost Rates
                userRate.NTFeeRate = model.NTFeeRate;
                userRate.NTCostRate = model.NTCostRate;
                userRate.OT1FeeRate = model.OT1FeeRate;
                userRate.OT1CostRate = model.OT1CostRate;
                userRate.OT2FeeRate = model.OT2FeeRate;
                userRate.OT2CostRate = model.OT2CostRate;
                userRate.OT3FeeRate = model.OT3FeeRate;
                userRate.OT3CostRate = model.OT3CostRate;
                userRate.OT4FeeRate = model.OT4FeeRate;
                userRate.OT4CostRate = model.OT4CostRate;
                userRate.OT5FeeRate = model.OT5FeeRate;
                userRate.OT5CostRate = model.OT5CostRate;
                userRate.OT6FeeRate = model.OT6FeeRate;
                userRate.OT6CostRate = model.OT6CostRate;
                userRate.OT7FeeRate = model.OT7FeeRate;
                userRate.OT7CostRate = model.OT7CostRate;
                userRate.LastModifiedBy = UserHelpers.GetCurrentUserId().ToString();
                userRate.LastModifiedDate = DateTime.Now;


                Db.SaveChanges();
                message = new InfoMessage
                {
                    MessageType = InfoMessageType.Success,
                    MessageContent = "Successfully updated User Rates."
                };
            }
            else
            {
                UserRate userRate2 = new UserRate
                {
                    EmployeeId = model.EmployeeID,
                    ProjectId = projectId,
                    ProjectUserClassificationID = Convert.ToInt32(model.ProjectUserClassificationID),
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    IsRatesConfirmed = model.IsRatesConfirmed,
                    //Fee Rates and Cost Rates
                    NTFeeRate = model.NTFeeRate,
                    NTCostRate = model.NTCostRate,
                    OT1FeeRate = model.OT1FeeRate,
                    OT1CostRate = model.OT1CostRate,
                    OT2FeeRate = model.OT2FeeRate,
                    OT2CostRate = model.OT2CostRate,
                    OT3FeeRate = model.OT3FeeRate,
                    OT3CostRate = model.OT3CostRate,
                    OT4FeeRate = model.OT4FeeRate,
                    OT4CostRate = model.OT4CostRate,
                    OT5FeeRate = model.OT5FeeRate,
                    OT5CostRate = model.OT5CostRate,
                    OT6FeeRate = model.OT6FeeRate,
                    OT6CostRate = model.OT6CostRate,
                    OT7FeeRate = model.OT7FeeRate,
                    OT7CostRate = model.OT7CostRate,
                    LastModifiedBy = UserHelpers.GetCurrentUserId().ToString(),
                    LastModifiedDate = DateTime.Now,
                };

                Db.UserRates.Add(userRate2);
                Db.SaveChanges();

                message = new InfoMessage
                {
                    MessageType = InfoMessageType.Success,
                    MessageContent = "Successfully Added User Rates."
                };
            }

            TempData["message"] = message;
            return Json(true);
        }

        [HttpPost]
        public JsonResult UpdateProjectUserClassification(int? employeeId, int? projectId, int? projectUserClassificationId, int? userRateId)
        {
            UserRate userRate = Db.UserRates.Single(x => x.EmployeeId == employeeId && x.ProjectId == projectId && x.UserRateId == userRateId);

            bool projectClassificationIsGeneric = !projectUserClassificationId.HasValue || Db.ProjectDisciplines.Any(x => x.ProjectDisciplineId == projectUserClassificationId);

            userRate.ProjectUserClassificationID = !projectClassificationIsGeneric ? null : projectUserClassificationId;
            Db.SaveChanges();

            return Json(true);
        }

        private List<SelectListItem> GetProjectUserClassificationSelectItems(int? projectId)
        {

            List<SelectListItem> selectItems = Db.ProjectUserClassifications.Where(x => x.ProjectID == projectId).Select(x => new SelectListItem { Value = x.ProjectUserClassificationId.ToString(), Text = x.ProjectClassificationText }).ToList();
            return selectItems;
        }

        [HttpPost]
        public void PeriodClosure(int userRateId, bool state)
        {
            UserRate userRate = Db.UserRates.SingleOrDefault(x => x.UserRateId == userRateId);
            if (userRate != null)
            {
                userRate.IsRatesConfirmed = state;
                Db.SaveChanges();
            }
        }
    }
}