using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using Spire.Pdf.General.Render.Font.OpenTypeFile;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Web.UI.WebControls;
using System.Threading.Tasks;
using System.Web.Script.Services;
using EntityState = System.Data.Entity.EntityState;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class UserRatesController : BaseEmployeesController
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

            return View();
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

        public ActionResult Create(int? projectId,int? employeeId)
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

        public ActionResult Edit(int userRateId,int? employeeId)
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

                userRate.LastModifiedBy = UserHelpers.GetCurrentUserId();
                userRate.LastModifiedDate = DateTime.Now;
            }
            else
            {
                userRate.LastModifiedBy = UserHelpers.GetCurrentUserId();
                userRate.LastModifiedDate = DateTime.Now;
                userRate.IsDeleted = false;
                Db.UserRates.Add(userRate);
            }

            //UserRate userRate2 = new UserRate
            //{
            //    EmployeeId = userRate.EmployeeId,
            //    ProjectId = projectId,
            //    ProjectUserClassificationID = Convert.ToInt32(userRate.ProjectUserClassificationID),
            //    StartDate = userRate.StartDate,
            //    EndDate = userRate.EndDate,
            //    IsRatesConfirmed = userRate.IsRatesConfirmed,
            //    LastModifiedBy = UserHelpers.GetCurrentUserId(),
            //    LastModifiedDate = DateTime.Now,
            //    //Fee & Cost Rates
            //    NTFeeRate = userRate?.NTFeeRate,
            //    NTCostRate = userRate?.NTCostRate,
            //    OT1FeeRate = userRate?.OT1FeeRate,
            //    OT1CostRate = userRate?.OT1CostRate,
            //    OT2FeeRate = userRate?.OT2FeeRate,
            //    OT2CostRate = userRate?.OT2CostRate,
            //    OT3FeeRate = userRate?.OT3FeeRate,
            //    OT3CostRate = userRate?.OT3CostRate,
            //    OT4FeeRate = userRate?.OT4FeeRate,
            //    OT4CostRate = userRate?.OT4CostRate,
            //    OT5FeeRate = userRate?.OT5FeeRate,
            //    OT5CostRate = userRate?.OT5CostRate,
            //    OT6FeeRate = userRate?.OT6FeeRate,
            //    OT6CostRate = userRate?.OT6CostRate,
            //    OT7FeeRate = userRate?.OT7FeeRate,
            //    OT7CostRate = userRate?.OT7CostRate,
            //};

           // Db.UserRates.Add(userRate2);
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
        public JsonResult InsertSkills(List<UserRate> userRates)
        {

            if (userRates == null)
            {
                userRates = new List<UserRate>();
            }
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return Json(false);
            }

            foreach (UserRate s in userRates)
            {
                s.ProjectId = projectId;
                s.LastModifiedBy = UserHelpers.GetCurrentUserId();
                s.LastModifiedDate = DateTime.Now;
                Db.UserRates.Add(s);
            }
            int insertedRecords = Db.SaveChanges();
            //    int insertedRecords = 0;
            return Json(insertedRecords);

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
                userRate.LastModifiedBy = UserHelpers.GetCurrentUserId();
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
                    LastModifiedBy = UserHelpers.GetCurrentUserId(),
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
        public JsonResult UpdateProjectUserClassification(int? employeeId, int? projectId, int? projectUserClassificationId,int? userRateId)
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