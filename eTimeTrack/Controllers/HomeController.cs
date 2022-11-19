using System;
using System.Web.Mvc;

namespace eTimeTrack.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "EmployeeTimesheets");
        }

        [AllowAnonymous]
        public ActionResult ErrorTest(bool throwInController = true)
        {
            if (throwInController)
                throw new Exception("Error Testing, please disregard");

            return View();
        }
    }
}