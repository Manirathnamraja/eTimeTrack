using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;

namespace eTimeTrack.Controllers
{

    public enum Source
    {
        Create,
        Existing
    }

    public enum ProjectComponentType
    {
        Project = 0,
        [Display(Name = "Project Part")]
        ProjectPart = 1,
        [Display(Name = "Project Group")]
        ProjectGroup = 2,
        [Display(Name = "Project Task")]
        ProjectTask = 3
    }

    [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
    public class ProjectComponentController : BaseController
    {
        public ActionResult Index(ProjectComponentType? type, int? ProjectPartSelector = null, int? ProjectGroupSelector = null)
        {
            if (type == null)
            {
                return InvokeHttp400(HttpContext);
            }
            // select type
            List<ProjectComponent> projectHierarchies = new List<ProjectComponent>();

            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            switch (type)
            {
                case ProjectComponentType.ProjectPart:
                    List<ProjectComponent> projectParts = selectedProject > 0 ? Db.ProjectParts.Where(x => x.ProjectID == selectedProject).OrderBy(x => x.Project.Name).ThenBy(x => x.PartNo).Select(x => new ProjectComponent { Item = x, CanDelete = !x.ProjectGroups.Any() }).ToList() : new List<ProjectComponent>();
                    projectHierarchies.AddRange(projectParts);
                    break;
                case ProjectComponentType.ProjectGroup:
                    List<ProjectComponent> projectGroups = selectedProject > 0 ? Db.ProjectGroups.Where(x => x.ProjectID == selectedProject && (ProjectPartSelector == null || x.PartID == ProjectPartSelector)).OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectPart.PartNo).ThenBy(x => x.GroupNo).Select(x => new ProjectComponent { Item = x, CanDelete = !x.Tasks.Any() }).ToList() : new List<ProjectComponent>();

                    projectHierarchies.AddRange(projectGroups);
                    break;
                case ProjectComponentType.ProjectTask:
                    List<ProjectComponent> projectTasks = selectedProject > 0 ? Db.ProjectTasks.Where(x => x.ProjectID == selectedProject && (ProjectPartSelector == null || x.ProjectGroup.ProjectPart.PartID == ProjectPartSelector) && (ProjectGroupSelector == null || x.GroupID == ProjectGroupSelector)).OrderBy(x => x.Project.Name).ThenBy(x => x.ProjectGroup.ProjectPart.PartNo).ThenBy(x => x.ProjectGroup.GroupNo).ThenBy(x => x.TaskNo).Select(x => new ProjectComponent { Item = x, CanDelete = !x.EmployeeTimesheetItems.Any() }).ToList() : new List<ProjectComponent>();
                    projectHierarchies.AddRange(projectTasks);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            ViewBag.ProjectSelector = GenerateDropdownUserProjects();
            ViewBag.SelectedProjectPart = ProjectPartSelector;
            ViewBag.SelectedProjectGroup = ProjectGroupSelector;

            ViewBag.InfoMessage = TempData["InfoMessage"];

            ProjectComponentViewModel vm = new ProjectComponentViewModel { Type = (ProjectComponentType)type, ProjectComponents = projectHierarchies.ToList() };
            return View(vm);
        }

        [HttpPost]
        public ActionResult IndexPost(ProjectComponentType type, int? ProjectPartSelector = null,
            int? ProjectGroupSelector = null)
        {
            return RedirectToAction("Index",
                new
                {
                    type = type,
                    ProjectPartSelector = ProjectPartSelector,
                    ProjectGroupSelector = ProjectGroupSelector
                });
        }

        [HttpPost]
        public ActionResult DeleteConfirmed(ProjectComponentType type, int id)
        {
            switch (type)
            {
                case ProjectComponentType.ProjectPart:
                    ProjectPart projectPart = new ProjectPart {PartID = id};
                    Db.ProjectParts.Attach(projectPart);
                    Db.ProjectParts.Remove(projectPart);
                    break;
                case ProjectComponentType.ProjectGroup:
                    ProjectGroup projectGroup = new ProjectGroup { GroupID = id };
                    Db.ProjectGroups.Attach(projectGroup);
                    Db.ProjectGroups.Remove(projectGroup);
                    break;
                case ProjectComponentType.ProjectTask:
                    ProjectTask projectTask = new ProjectTask { TaskID = id };
                    Db.ProjectTasks.Attach(projectTask);
                    Db.ProjectTasks.Remove(projectTask);
                    Db.ProjectVariationItems.RemoveRange(Db.ProjectVariationItems.Where(x => x.TaskID == id));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            Db.SaveChanges();

            TempData["InfoMessage"] = new InfoMessage(InfoMessageType.Success, "Succesfully deleted item");
            return RedirectToAction(nameof(Index), new {type = type});
        }

        public ActionResult Create(ProjectComponentType type, int? ProjectSelector, int? ProjectPartSelector, int? ProjectGroupSelector)
        {
            bool valid = CheckValidCreateSelections(type, ProjectSelector, ProjectPartSelector, ProjectGroupSelector);

            if (!valid)
            {
                TempData["InfoMessage"] = new InfoMessage(InfoMessageType.Failure, "Please ensure that the project component fields are selected.");
                return RedirectToAction("Index", new { type = type });
            }

            if (type == ProjectComponentType.ProjectPart)
            {
                return RedirectToAction("CreateEdit", "ProjectParts", new { projectId = ProjectSelector });
            }
            if (type == ProjectComponentType.ProjectGroup)
            {
                return RedirectToAction("CreateEdit", "ProjectGroups", new { projectId = ProjectSelector, partId = ProjectPartSelector });
            }
            if (type == ProjectComponentType.ProjectTask)
            {
                return RedirectToAction("CreateEdit", "ProjectTasks", new { projectId = ProjectSelector, partId = ProjectPartSelector, groupId = ProjectGroupSelector });
            }

            return RedirectToAction("Index", new { type = type });
        }

        private bool CheckValidCreateSelections(ProjectComponentType type, int? project, int? projectPart, int? projectGroup)
        {
            if (type == ProjectComponentType.ProjectPart && project == null) return false;
            if (type == ProjectComponentType.ProjectGroup && (project == null || projectPart == null)) return false;
            if (type == ProjectComponentType.ProjectTask && (project == null || projectPart == null || projectGroup == null)) return false;
            return true;
        }

        public ActionResult Edit(ProjectComponentType type, int? id)
        {
            if (type == ProjectComponentType.ProjectPart)
            {
                return RedirectToAction("CreateEdit", "ProjectParts", new { id = id });
            }
            else if (type == ProjectComponentType.ProjectGroup)
            {
                return RedirectToAction("CreateEdit", "ProjectGroups", new { id = id });
            }
            else if (type == ProjectComponentType.ProjectTask)
            {
                return RedirectToAction("CreateEdit", "ProjectTasks", new { id = id });
            }

            return RedirectToAction("Index", new { type = type });
        }

        [HttpPost]
        public JsonResult SetActiveStatus(int id, ProjectComponentType componentType, bool active)
        {
            IProjectComponent component = GetProjectComponent(id, componentType);

            if (component == null)
                return Json(new { Success = false });

            component.IsClosed = !active;
            Db.SaveChanges();
            return Json(new { Success = true });
        }

        private IProjectComponent GetProjectComponent(int id, ProjectComponentType componentType)
        {
            IProjectComponent component = null;
            switch (componentType)
            {
                case ProjectComponentType.ProjectPart:
                    component = Db.ProjectParts.Find(id);
                    break;
                case ProjectComponentType.ProjectGroup:
                    component = Db.ProjectGroups.Find(id);
                    break;
                case ProjectComponentType.ProjectTask:
                    component = Db.ProjectTasks.Find(id);
                    break;
            }
            return component;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}