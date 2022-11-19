using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace eTimeTrack.Controllers
{
    public abstract class BaseEmployeesController : BaseController
    {
        protected readonly Lazy<AccountController> _account;

        protected BaseEmployeesController()
        {
            _account = new Lazy<AccountController>(() => new AccountController(HttpContext.GetOwinContext().Get<ApplicationUserManager>(), HttpContext.GetOwinContext().Get<ApplicationSignInManager>()));
        }

        protected bool UserIsInRole(int userId, string roleName)
        {
            return _account.Value.UserManager.IsInRole(userId, roleName);
        }

        protected bool UserIsInRoles(int userId, IEnumerable<string> roleNames)
        {
            foreach (string roleName in roleNames)
            {
                bool found = _account.Value.UserManager.IsInRole(userId, roleName);
                if (found) return true;
            }
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}