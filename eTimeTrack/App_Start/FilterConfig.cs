using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using eTimeTrack.Extensions;
using eTimeTrack.Helpers;
using Elmah;
using Microsoft.AspNet.Identity;

namespace eTimeTrack
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ElmahHandledErrorLoggerFilter());
            filters.Add(new HandleErrorAttribute());
        }
    }

    //This class is to log errors that are caught by other pages so we get error messages
    public class ElmahHandledErrorLoggerFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            // Log only handled exceptions, because all other will be caught by ELMAH anyway.
            if (context.ExceptionHandled)
                ErrorSignal.FromCurrentContext().Raise(context.Exception);
        }
    }

    public class AdminRedirectFilter : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            IPrincipal user = HttpContext.Current.User;

            if (user.IsInAnyAdminRole())
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary{{ "controller", "Manage" },
                            { "action", "Index" }

                    });
            }
            base.OnActionExecuting(filterContext);
        }


    }
}
