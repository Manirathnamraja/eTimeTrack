﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using System;
using System.Web.UI.WebControls;
using EntityState = System.Data.Entity.EntityState;
using System.Web;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Xml;
using OfficeOpenXml;
using System.Globalization;
using System.Web.Hosting;
using System.Threading.Tasks;
using eTimeTrack.Extensions;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class UserEndDatesController : BaseController
    {
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult Index()
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            if (selectedProject == 0) { return InvokeHttp404(HttpContext); }

            UserEndDatesViewModel model = new UserEndDatesViewModel
            {
                ProjectId = selectedProject,
                Project = GetProject(selectedProject)

            };
            SelectList select = GetEnddates(selectedProject);
            model.DateSelect = select;

            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(model);
        }

        private SelectList GetProject(int projectId)
        {
            return new SelectList(GetProjectdetails(projectId), "ProjectID", "DisplayName");
        }

        private List<Project> GetProjectdetails(int? projectId)
        {
            return Db.Projects.Where(x => x.ProjectID == projectId).OrderBy(x => x.ProjectNo).ToList();
        }
        private SelectList GetEnddates(int projectId)
        {
            List<UserRate> projectDatePeriods = Db.UserRates.Where(x => x.ProjectId == projectId).ToList();

            IEnumerable<SelectListItem> selectItems = projectDatePeriods.Select(x => new SelectListItem()
            {
                Value = x.UserRateId.ToString(),
                Text = x.EndDate.ToString()
            });
            SelectList existingPeriods = new SelectList(selectItems, "Value", "Text");

            var texts = selectItems.Select(x => x.Value).ToList();

            return existingPeriods;
        }
    }
}