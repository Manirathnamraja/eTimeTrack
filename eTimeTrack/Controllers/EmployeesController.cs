using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using eTimeTrack.Extensions;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class EmployeesController : BaseEmployeesController
    {
        public ActionResult Index()
        {
            List<Employee> employees = GetAllEmployeesOrdered();
            return View(employees);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return InvokeHttp400(HttpContext);
            }
            Employee employee = Db.Users.Find(id);
            if (employee == null)
            {
                return InvokeHttp404(HttpContext);
            }


            ApplicationUserViewModel applicationUserViewModel = new ApplicationUserViewModel
            {
                Employee = new EmployeeViewModel
                {
                    Id = employee.Id,
                    Email = employee.Email,
                    EmployeeNo = employee.EmployeeNo,
                    UserName = employee.UserName,
                    FirstName = employee.FullNameFirstName,
                    CompanyID = employee.CompanyID,
                    Surname = employee.FullNameSurname,
                    AllowOT = employee.AllowOT,
                    IsActive = employee.IsActive,
                    ManagerID = employee.ManagerID,
                    Lockout = employee.LockoutEndDateUtc != null
                },
                RoleType = RoleType.RoleUser
            };

            if (UserIsInRole(employee.Id, UserHelpers.RoleTimesheetEditor))
                applicationUserViewModel.RoleType = RoleType.RoleTimesheetEditor;
            if (UserIsInRole(employee.Id, UserHelpers.RoleUserAdministrator))
                applicationUserViewModel.RoleType = RoleType.RoleUserAdministrator;
            if (UserIsInRole(employee.Id, UserHelpers.RoleUserPlus))
                applicationUserViewModel.RoleType = RoleType.RoleUserPlus;
            if (UserIsInRole(employee.Id, UserHelpers.RoleAdmin))
                applicationUserViewModel.RoleType = RoleType.RoleAdmin;
            if (UserIsInRole(employee.Id, UserHelpers.RoleSuperUser))
                applicationUserViewModel.RoleType = RoleType.RoleSuperUser;

            ViewBag.InfoMessage = TempData["InfoMessage"];
            PopulateEditViewDropdowns(applicationUserViewModel.Employee);

            return View(applicationUserViewModel);
        }

        private void AddUserToRole(Employee foundUser, string role)
        {
            _account.Value.UserManager.AddToRole(foundUser.Id, role);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationUserViewModel applicationUserViewModel)
        {
            Employee existing = Db.Users.SingleOrDefault(x => x.Id == applicationUserViewModel.Employee.Id);
            if (ModelState.IsValid && existing != null)
            {
                string userName = string.IsNullOrWhiteSpace(existing.Email) ? existing.EmployeeNo : existing.Email;
                if (existing.UserName != userName &&
                    Db.Users.Any(x => x.UserName == userName))
                {
                    Employee existingUser = Db.Users.First(x => x.UserName == userName);
                    ViewBag.InfoMessage = new InfoMessage(InfoMessageType.Failure,
                        $"This username ({userName}) already exists in the system for user with employee number: {existingUser.EmployeeNo} and email {existingUser.Email}. Please confirm employee details or contact an administrator for assistance.");
                    applicationUserViewModel.Employee.Email = existing.Email;
                }
                else if (existing.Email != applicationUserViewModel.Employee.Email &&
                    Db.Users.Any(x => x.Email == applicationUserViewModel.Employee.Email))
                {
                    ViewBag.InfoMessage = new InfoMessage(InfoMessageType.Failure,
                        $"This email address ({applicationUserViewModel.Employee.Email}) already exists in the system. Please confirm employee details.");
                    applicationUserViewModel.Employee.Email = existing.Email;
                }
                else if (existing.EmployeeNo != applicationUserViewModel.Employee.EmployeeNo &&
                         Db.Users.Any(x => x.EmployeeNo == applicationUserViewModel.Employee.EmployeeNo))
                {
                    ViewBag.InfoMessage = new InfoMessage(InfoMessageType.Failure,
                        $"This employee number ({applicationUserViewModel.Employee.EmployeeNo}) already exists in the system. Please confirm employee details.");
                    applicationUserViewModel.Employee.EmployeeNo = existing.EmployeeNo;
                }
                else
                {
                    existing.EmployeeNo = applicationUserViewModel.Employee.EmployeeNo;
                    existing.Email = applicationUserViewModel.Employee.Email;
                    existing.UserName = string.IsNullOrWhiteSpace(existing.Email) ? existing.EmployeeNo : existing.Email;
                    existing.ManagerID = applicationUserViewModel.Employee.ManagerID;
                    existing.CompanyID = applicationUserViewModel.Employee.CompanyID;
                    existing.AllowOT = applicationUserViewModel.Employee.AllowOT;
                    existing.IsActive = applicationUserViewModel.Employee.IsActive;
                    existing.EmailConfirmed = true;
                    existing.Names = applicationUserViewModel.Employee.FullName;

                    // lockout users
                    if (applicationUserViewModel.Employee.Lockout)
                    {
                        existing.LockoutEndDateUtc = DateTime.MaxValue;
                    }
                    else
                    {
                        existing.LockoutEndDateUtc = null;
                    }

                    Db.SaveChanges();

                    // Account-level permissions (not permitted for UserPlus role or for admins reassigning superusers)
                    if (User.IsSuperUser() || (User.IsAdmin() && applicationUserViewModel.RoleType != RoleType.RoleSuperUser))
                    {
                        Employee foundUser = _account.Value.UserManager.FindById(applicationUserViewModel.Employee.Id);
                        foundUser.Email = applicationUserViewModel.Employee.Email;

                        foundUser.Roles.Clear();
                        _account.Value.UserManager.Update(foundUser);

                        switch (applicationUserViewModel.RoleType)
                        {
                            case RoleType.RoleUserAdministrator:
                                AddUserToRole(foundUser, UserHelpers.RoleUserAdministrator);
                                break;

                            case RoleType.RoleTimesheetEditor:
                                AddUserToRole(foundUser, UserHelpers.RoleTimesheetEditor);
                                break;

                            case RoleType.RoleUserPlus:
                                AddUserToRole(foundUser, UserHelpers.RoleUserPlus);
                                break;

                            case RoleType.RoleAdmin:
                                AddUserToRole(foundUser, UserHelpers.RoleAdmin);
                                break;

                            case RoleType.RoleSuperUser:
                                AddUserToRole(foundUser, UserHelpers.RoleSuperUser);
                                break;
                        }
                        _account.Value.UserManager.Update(foundUser);
                        Db.SaveChanges();
                    }

                 ViewBag.InfoMessage = null;
                    return RedirectToAction("Index");
                }
            }
            PopulateEditViewDropdowns(applicationUserViewModel.Employee);

            return View(applicationUserViewModel);
        }

        public async Task<ActionResult> ResetPassword(int? id)
        {
            if (id == null)
            {
                return InvokeHttp400(HttpContext);
            }

            bool success = await ResetPasswordWork((int)id);

            if (success)
            {
                TempData["InfoMessage"] = new InfoMessage
                {
                    MessageType = InfoMessageType.Success,
                    MessageContent = "Successfully sent reset email to user."
                };
            }
            else
            {
                TempData["InfoMessage"] = new InfoMessage
                {
                    MessageType = InfoMessageType.Failure,
                    MessageContent = "Sending reset email to user was unsuccessful. Please contact a system administrator."
                };
            }

            return RedirectToAction("Edit", new { id = id });
        }

        public async Task<bool> ResetPasswordWork(int id)
        {
            Employee user = await _account.Value.UserManager.FindByIdAsync(id);
            if (user == null || !(await _account.Value.UserManager.IsEmailConfirmedAsync(user.Id)))
            {
                return false;
            }
            string code = await _account.Value.UserManager.GeneratePasswordResetTokenAsync(user.Id);

            code = HttpUtility.UrlEncode(code);
            string callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url?.Scheme);
            await _account.Value.UserManager.SendEmailAsync(user.Id, "eTimeTrack Password Reset", "An administrator has reset your eTimeTrack Password. Please reset your password for eTimeTrack by clicking <a href=\"" + callbackUrl + "\">here</a>");

            return true;
        }

        private void PopulateEditViewDropdowns(EmployeeViewModel employee)
        {
            ViewBag.CompanyID = new SelectList(Db.Companies, "Company_Id", "Company_Name", employee.CompanyID);
            ViewBag.ManagerID = new SelectList(Db.Users, "Id", "EmployeeNo", employee.ManagerID);
        }
    }
}