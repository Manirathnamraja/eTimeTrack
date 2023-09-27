using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using eTimeTrack.Models;

namespace eTimeTrack.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private static SelectList companyList;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
            companyList = new SelectList(Db.Companies.OrderBy(x => x.Company_Name), "Company_Id", "Company_Name");
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            Session["SelectedProject"] = null;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                //Allow the user to login with either employee number or email
                Employee myUser = db.Users.FirstOrDefault(u => u.EmployeeNo == model.Username || u.Email == model.Username);
                if (myUser != null)
                    model.Username = myUser.UserName;
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            SignInStatus result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    if (IsMobileDevice())
                    {
                        return RedirectToAction("NewTimesheet", "Mobile");
                    }
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        [Authorize(Roles = UserHelpers.RoleSuperUser)]
        public async Task<ActionResult> PopulateAllAccountsWithRandomPasswords(int projectId)
        {
            int successfulCount = 0;
            List<string> failedEmails = new List<string>();
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                string bodyText = "eTimeTrack users,";
                bodyText += "<br /><br />";
                bodyText += "On Wednesday 30th August 2017 we will be moving to a web enabled version of eTimeTrack for the NL2 Project.";
                bodyText += "<br /><br />";
                bodyText += "In order to access this new timesheet functionality, use the link provided below to access the website from where timesheets can be recorded. At first login, you will need to provide your home office email address and create a new password. Password requirements are as follows:";
                bodyText += "<br /><br />";
                bodyText += "<ul>";
                bodyText += "<li>at least 6 characters overall</li>";
                bodyText += "<li>at least 1 lowercase letter</li>";
                bodyText += "<li>at least 1 uppercase letter</li>";
                bodyText += "<li>at least 1 number</li>";
                bodyText += "</ul>";
                bodyText += "<br /><br />";
                bodyText += "Once you have created a password, you will be able to login and create your timesheet for NL2 from any computer with internet access.";
                bodyText += "<br /><br />";
                bodyText += "We trust that you find the new timesheet functionality intuitive and easy to use. Feedback on the interface will be appreciated and should be addressed to support@anzgeo.com.";
                bodyText += "<br /><br />";
                bodyText += "<b>As always, your primary responsibility is to complete your ORACLE timesheet using your AECOM interface of choice.</b> The new version of eTimeTrack does not change the requirement of having to complete two timesheets. If you are unable to complete the eTimeTrack timesheet, we will follow up with you next week.";
                bodyText += "<br /><br />";
                bodyText += "If you have any questions regarding completion of your timesheet, please contact Helen Drummond.";
                bodyText += "<br /><br />";

                List<Employee> users = db.Users.Where(u => (u.PasswordHash == null || u.PasswordHash == string.Empty) && u.Email != null && u.Email != string.Empty).ToList();

                users = users.Where(x => x.Projects.Any(y => y.ProjectId == projectId)).ToList();

                foreach (Employee employee in users)
                {
                    try
                    {
                        string code = await UserManager.GeneratePasswordResetTokenAsync(employee.Id);
                        code = HttpUtility.UrlEncode(code);
                        string callbackUrl = Url.Action("ResetPassword", "Account", new { userId = employee.Id, code = code }, protocol: Request.Url.Scheme);

                        string userSpecificText = bodyText + "<a href=\"" + callbackUrl + "\">Generate Password and Login</a>";

                        bool success = EmailHelper.SendEmail(employee.Email, "eTimeTrack Password Generation", userSpecificText);
                        if (success)
                            successfulCount++;
                        else
                        {
                            failedEmails.Add(employee.Email);
                        }
                    }
                    catch (Exception e)
                    {
                        failedEmails.Add(employee.Email);
                        continue;
                    }
                }

                bodyText = "eTimeTrack users,";
                bodyText += "<br /><br />";
                bodyText += "On Wednesday 30th August 2017 we will be moving to a web enabled version of eTimeTrack for the NL2 Project.";
                bodyText += "<br /><br />";
                bodyText += "In order to access this new timesheet functionality, use the link provided below to access the website from where timesheets can be recorded. You already have a username and password set up. If you have forgotten your username or password, please follow the link to reset it.";
                bodyText += "<br /><br />";
                bodyText += "We trust that you find the new timesheet functionality intuitive and easy to use. Feedback on the interface will be appreciated and should be addressed to support@anzgeo.com.";
                bodyText += "<br /><br />";
                bodyText += "<b>As always, your primary responsibility is to complete your ORACLE timesheet using your AECOM interface of choice.</b> The new version of eTimeTrack does not change the requirement of having to complete two timesheets. If you are unable to complete the eTimeTrack timesheet, we will follow up with you next week.";
                bodyText += "<br /><br />";
                bodyText += "If you have any questions regarding completion of your timesheet, please contact Helen Drummond.";
                bodyText += "<br /><br />";

                users = db.Users.Where(u => u.PasswordHash != null && u.PasswordHash != string.Empty && u.Email != null && u.Email != string.Empty).ToList();

                users = users.Where(x => x.Projects.Any(y => y.ProjectId == projectId)).ToList();

                foreach (Employee employee in users)
                {
                    try
                    {
                        string code = await UserManager.GeneratePasswordResetTokenAsync(employee.Id);
                        code = HttpUtility.UrlEncode(code);
                        string callbackUrl = Url.Action("ResetPassword", "Account", new { userId = employee.Id, code = code }, protocol: Request.Url.Scheme);

                        string userSpecificText = bodyText + "<a href=\"" + callbackUrl + "\">Reset Password and Login</a>";

                        bool success = EmailHelper.SendEmail(employee.Email, "eTimeTrack New Project", userSpecificText);
                        if (success)
                            successfulCount++;
                        else
                        {
                            failedEmails.Add(employee.Email);
                        }
                    }
                    catch (Exception e)
                    {
                        failedEmails.Add(employee.Email);
                        continue;
                    }
                }

                EmailHelper.SendEmail("richard.hammond@aecom.com", "eTimeTrack Emails Sent", "Have successfully sent " + successfulCount + " emails to users and there were " + failedEmails.Count + " unsuccessful: " + string.Join(" ", failedEmails));
            }

            return View(successfulCount);
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
        public ActionResult Register()
        {
            EmployeeViewModel model = new EmployeeViewModel { IsActive = true };

            Company aecom = Db.Companies.FirstOrDefault(x => x.Company_Name.StartsWith("AECOM"));
            if (aecom != null)
            {
                model.CompanyID = aecom.Company_Id;
            }

            SetRegisterViewBag();

            return View(model);
        }

        private void SetRegisterViewBag()
        {
            ViewBag.CompanyList = companyList;
            ViewBag.ManagerID = new SelectList(Db.Users, "Id", "EmployeeNo");
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(EmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (Db.Users.Any(x => x.Email == model.Email))
                {
                    ViewBag.InfoMessage = new InfoMessage(InfoMessageType.Failure,
                        "Email already exists in the system, please try another.");
                }
                else if (Db.Users.Any(x => x.EmployeeNo == model.EmployeeNo))
                {
                    ViewBag.InfoMessage = new InfoMessage(InfoMessageType.Failure,
                        "Employee Number already exists in the system, please try another.");
                }
                else
                {
                    try
                    {
                        Employee user = new Employee
                        {
                            CompanyID = model.CompanyID,
                            UserName = model.Email,
                            Email = model.Email,
                            EmployeeNo = model.EmployeeNo,
                            EmailConfirmed = true,
                            LockoutEnabled = true,
                            LastModifiedBy = UserHelpers.GetCurrentUserId(),
                            LastModifiedDate = DateTime.UtcNow,
                            AllowOT = model.AllowOT,
                            IsActive = model.IsActive,
                            ManagerID = model.ManagerID,
                            Names = model.FullName
                        };
                        IdentityResult result = await UserManager.CreateAsync(user);
                        if (result.Succeeded)
                        {
                            user = await UserManager.FindByEmailAsync(user.Email);

                            UserManager.AddToRole(user.Id, UserHelpers.RoleUser);

                            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                            code = HttpUtility.UrlEncode(code);
                            string callbackUrl = Url.Action("ResetPassword", "Account",
                                new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                            string bodyText =
                                "The eTimeTrack administration staff have created an account for you. Please click the link below to setup your password and start using your account!";
                            bodyText += "<br/><br/><a href=\"" + callbackUrl +
                                        "\">Generate Password and Login</a><br /><br />";

                            await UserManager.SendEmailAsync(user.Id, "eTimeTrack Account Registration", bodyText);

                            return RedirectToAction("Assign", "EmployeeProjects", new { id = user.Id });
                        }
                        AddErrors(result);
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Trace.TraceInformation("Property: {0} Error: {1}",
                                    validationError.PropertyName,
                                    validationError.ErrorMessage);
                            }
                        }
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            SetRegisterViewBag();
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(int userId, string code)
        {
            if (userId == default(int) || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                code = HttpUtility.UrlEncode(code);
                string callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "eTimeTrack Password Reset", "Please reset your password for eTimeTrack by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            Employee user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            string code = HttpUtility.UrlDecode(model.Code);
            IdentityResult result = await UserManager.ResetPasswordAsync(user.Id, code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == default(int))
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new Employee { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "EmployeeTimesheets");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "EmployeeTimesheets");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}