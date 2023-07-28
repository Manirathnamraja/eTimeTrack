using System.Collections.Generic;
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
                Project = GetProjectPartsSelect(selectedProject)

            };
            SelectList select = GetProjectTimesheetPeriodsSelect(selectedProject);
            model.DateSelect = select;

            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(model);
        }

        private SelectList GetProjectPartsSelect(int projectId)
        {
            return new SelectList(GetProjectParts(projectId), "PartID", "DisplayName");
        }

        private List<Project> GetProjectParts(int? projectId)
        {
            return Db.Projects.Where(x => x.ProjectID == projectId).OrderBy(x => x.ProjectNo).ToList();
        }
       
    }
}