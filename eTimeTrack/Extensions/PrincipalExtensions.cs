using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using eTimeTrack.Helpers;

namespace eTimeTrack.Extensions
{
    public static class PrincipalExtensions
    {
        public static bool IsInAnyAdminRole(this IPrincipal user)
        {
            return user.IsInRole(UserHelpers.RoleSuperUser) || user.IsInRole(UserHelpers.RoleAdmin) || user.IsInRole(UserHelpers.RoleUserPlus) || user.IsInRole(UserHelpers.RoleTimesheetEditor) || user.IsInRole(UserHelpers.RoleUserAdministrator);
        }

        public static bool IsSuperUser(this IPrincipal user)
        {
            return user.IsInRole(UserHelpers.RoleSuperUser);
        }

        public static bool IsUser(this IPrincipal user)
        {
            return user.IsInRole(UserHelpers.RoleUser);
        }

        public static bool IsAdmin(this IPrincipal user)
        {
            return user.IsInRole(UserHelpers.RoleAdmin);
        }

        public static bool IsUserAdministrator(this IPrincipal user)
        {
            return user.IsInRole(UserHelpers.RoleUserAdministrator);
        }

        public static bool IsAdminOrAbove(this IPrincipal user)
        {
            return user.IsInRole(UserHelpers.RoleSuperUser) || user.IsInRole(UserHelpers.RoleAdmin);
        }

        public static bool IsUserPlusOrAbove(this IPrincipal user)
        {
            return user.IsInRole(UserHelpers.RoleSuperUser) || user.IsInRole(UserHelpers.RoleAdmin) || user.IsInRole(UserHelpers.RoleUserPlus);
        }

        public static bool IsTimesheetEditorOrAbove(this IPrincipal user)
        {
            return user.IsInRole(UserHelpers.RoleSuperUser) || user.IsInRole(UserHelpers.RoleAdmin) || user.IsInRole(UserHelpers.RoleUserPlus) || user.IsInRole(UserHelpers.RoleTimesheetEditor);
        }

        public static bool IsUserAdministratorOrAboveExcludeTimesheetEditor(this IPrincipal user)
        {
            return user.IsInRole(UserHelpers.RoleSuperUser) || user.IsInRole(UserHelpers.RoleAdmin) || user.IsInRole(UserHelpers.RoleUserPlus) || user.IsInRole(UserHelpers.RoleUserAdministrator);
        }
    }
}