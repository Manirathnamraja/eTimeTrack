using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextAnyAdminRole)]
    public class ProjectSelectorController : BaseController
    {
        public ActionResult Index()
        {
            if (Session["SelectedProject"] == null)
            {
                List<Project> userProjects = GetProjectsAssignedToUser();
                Session["SelectedProject"] = userProjects.Any() ? userProjects.First().ProjectID : (int?)null;
                Session["SelectedProjectName"] = userProjects.Any() ? userProjects.First().DisplayName : null;
            }

            ProjectSelectorViewModel model = new ProjectSelectorViewModel { Projects = GenerateDropdownUserProjects(), SelectedProjectId = (int?)Session["SelectedProject"] };

            return View(model);
        }

        public JsonResult UpdateProject(int? projectId)
        {
            Project project = Db.Projects.Find(projectId);

            if (project == null)
            {
                return Json(false);
            }

            Session["SelectedProject"] = project.ProjectID;
            Session["SelectedProjectName"] = project.DisplayName;
            return Json(true);
        }
    }
}