using System.Web.Mvc;

namespace eTimeTrack.Controllers
{
    public class ErrorController : BaseController
    {
        public ViewResult NotFound(string url = "")
        {
            //Ensure the status code of the site is 404 to make sure browser and search engines act accordingly
            Response.StatusCode = 404;

            // If you're running under IIS 7 in Integrated mode set use this line to override
            // IIS errors:
            Response.TrySkipIisCustomErrors = true;

            return View("NotFound", (object)null);
        }

        public ViewResult BadRequest(string url = "")
        {
            //Ensure the status code of the site is 400 to make sure browser and search engines act accordingly
            Response.StatusCode = 400;

            // If you're running under IIS 7 in Integrated mode set use this line to override
            // IIS errors:
            Response.TrySkipIisCustomErrors = true;

            return View("BadRequest", (object)null);
        }
    }
}