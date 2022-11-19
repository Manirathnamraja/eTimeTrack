using System.Web.Mvc;
using System.Web.Routing;

namespace eTimeTrack
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "EmployeeTimesheets", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute("NotFound", "{*url}",
                new {controller = "Error", action = "NotFound"});
        }
    }
}
