using System.Security.Principal;
using Microsoft.AspNet.Identity;
using System.Web;
using eTimeTrack.Models;

namespace eTimeTrack.Helpers
{
    public static class UserHelpers
    {
        public const string RoleUser = "User";
        public const string RoleUserAdministrator = "UserAdministrator";
        public const string RoleTimesheetEditor = "TimesheetEditor";
        public const string RoleUserPlus = "UserPlus";
        public const string RoleAdmin = "Admin";
        public const string RoleSuperUser = "SuperUser";
        public const int Invalid = 0;

        public const string AuthTextAdminOrAbove = "SuperUser, Admin";
        public const string AuthTextUserPlusOrAbove = "SuperUser, Admin, UserPlus";
        public const string AuthTextTimesheetEditorOrAbove = "SuperUser, Admin, UserPlus, TimesheetEditor";
        public const string AuthTextUserAdministratorOrAboveExcludeTimesheetEditor = "SuperUser, Admin, UserPlus, UserAdministrator";
        public const string AuthTextAnyAdminRole = "SuperUser, Admin, UserPlus, TimesheetEditor, UserAdministrator";

        // Get as ApplicationUser based on an Id
        public static Employee GetUser(int userId)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            Employee e = db.Users.Find(userId);
            return e;
        }

        // Get the logged in user as an ApplicationUser
        public static Employee GetCurrentUser()
        {
            if (HttpContext.Current != null && HttpContext.Current.User != null)
            {
                int currentUser = HttpContext.Current.User.Identity.GetUserId<int>();
                return GetUser(currentUser);
            }
            return null;
        }

        // Get the logged in user Id as string
        public static int GetCurrentUserId()
        {
            Employee user = GetCurrentUser();
            if (user == null) return Invalid;
            return user.Id;
        }
    }

    public enum RoleType
    {
        RoleUser = 1,
        RoleAdmin = 2,
        RoleSuperUser = 3,
        RoleUserPlus = 4,
        RoleTimesheetEditor = 5,
        RoleUserAdministrator = 6
    }
}