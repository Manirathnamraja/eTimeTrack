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
                model.ProjectUserClassifications = selectItems;
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

        private List<SelectListItem> GetProjectUserClassificationSelectItems(int projectId)
        {

            List<SelectListItem> selectItems = Db.ProjectUserClassifications.Where(x => x.ProjectID == projectId).Select(x => new SelectListItem { Value = x.ProjectUserClassificationId.ToString(), Text = x.ProjectClassificationText }).ToList();
            return selectItems;
        }
    }
}