﻿using eTimeTrack.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class ExpensesController : BaseController
    {

        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult ExpensesTypes()
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            if (selectedProject == 0) { return InvokeHttp404(HttpContext); }
            return View();
        }
    }
}