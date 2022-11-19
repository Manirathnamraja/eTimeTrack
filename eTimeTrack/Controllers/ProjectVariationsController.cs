using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
    public class ProjectVariationsController : BaseController
    {
        public ActionResult Index()
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            List<ProjectVariation> projectVariations = new List<ProjectVariation>();

            if (selectedProject > 0)
            {
                projectVariations = Db.ProjectVariations.Where(x => x.ProjectID == selectedProject).OrderBy(x => x.VariationNo).ToList();
            }

            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(projectVariations);
        }

        [HttpPost]
        public ActionResult Create(int? projectID)
        {
            if (projectID == null) RedirectToAction("Index");
            return RedirectToAction("CreateEdit", new { projectId = projectID });
        }

        [HttpPost]
        public ActionResult IndexPost(int? VariationId, int? ProjectPartSelector = null, int? ProjectGroupSelector = null)
        {
            return RedirectToAction("Assign", new { id = VariationId, ProjectPartSelector = ProjectPartSelector, ProjectGroupSelector = ProjectGroupSelector });
        }

        public ActionResult CreateEdit(int? id, int? projectId)
        {
            ProjectVariation projectVariation;
            if (id == null)
            {
                if (projectId != null)
                {
                    projectVariation = new ProjectVariation { ProjectID = (int)projectId, IsApproved = true };
                    ViewBag.Source = Source.Create;
                }
                else
                {
                    return InvokeHttp400(HttpContext);
                }
            }
            else
            {
                projectVariation = Db.ProjectVariations.Find(id);
                if (projectVariation == null)
                {
                    return InvokeHttp404(HttpContext);
                }
                ViewBag.Source = Source.Existing;
            }

            return View(projectVariation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEdit(ProjectVariation projectVariation, Source source)
        {
            if (ModelState.IsValid)
            {
                ProjectVariation existing = Db.ProjectVariations.Find(projectVariation.VariationID);

                if (existing != null)
                {
                    Db.Entry(existing).CurrentValues.SetValues(projectVariation);
                    Db.Entry(existing).State = EntityState.Modified;
                }
                else
                {
                    Db.ProjectVariations.Add(projectVariation);
                }

                Db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Source = source;
            return View(projectVariation);
        }

        public ActionResult Assign(int? id, int? ProjectPartSelector = null, int? ProjectGroupSelector = null)
        {
            if (id == null)
            {
                return InvokeHttp400(HttpContext);
            }
            ProjectVariation projectVariation = Db.ProjectVariations.Find(id);

            if (projectVariation == null)
            {
                return InvokeHttp404(HttpContext);
            }

            List<ProjectTask> tasks = Db.ProjectTasks.Where(x => x.ProjectID == projectVariation.Project.ProjectID && (ProjectPartSelector == null || x.ProjectGroup.ProjectPart.PartID == ProjectPartSelector) && (ProjectGroupSelector == null || x.GroupID == ProjectGroupSelector)).OrderBy(x => x.TaskNo).ToList();

            ViewBag.ProjectPartSelector = GetProjectPartsSelect(projectVariation.ProjectID);
            ViewBag.SelectedProjectPart = ProjectPartSelector;
            ViewBag.SelectedProjectGroup = ProjectGroupSelector;

            return View(new ProjectVariationAssignmentModel { ProjectVariation = projectVariation, ProjectTasks = tasks });
        }

        [HttpPost]
        public void AssignToTask(int projectVariationId, int taskId, bool assigned)
        {
            ProjectVariationItem existing = Db.ProjectVariationItems.SingleOrDefault(x => x.TaskID == taskId && x.VariationID == projectVariationId);

            if (existing == null)
            {
                if (assigned)
                {
                    ProjectVariationItem newVariationItem =
                        new ProjectVariationItem { TaskID = taskId, VariationID = projectVariationId, IsClosed = false };
                    Db.ProjectVariationItems.Add(newVariationItem);
                }
            }
            else if (!assigned)
            {
                Db.ProjectVariationItems.Remove(existing);
            }
            Db.SaveChanges();
        }


        [HttpPost]
        public void Approval(int projectVariationId, int taskId, bool approved)
        {
            ProjectVariationItem existing = Db.ProjectVariationItems.SingleOrDefault(x => x.TaskID == taskId && x.VariationID == projectVariationId);
            if (existing == null) return;
            existing.IsApproved = approved;
            Db.SaveChanges();
        }

        [HttpPost]
        public void Close(int projectVariationId, int taskId, bool closed)
        {
            ProjectVariationItem existing = Db.ProjectVariationItems.SingleOrDefault(x => x.TaskID == taskId && x.VariationID == projectVariationId);
            if (existing == null) return;
            existing.IsClosed = closed;
            Db.SaveChanges();
        }

        [HttpPost]
        public ActionResult DeleteConfirmed(int? id)
        {
            ProjectVariation projectVariation = Db.ProjectVariations.Find(id);
            if (projectVariation != null)
            {
                try
                {
                    Db.ProjectVariationItems.RemoveRange(projectVariation.ProjectVariationItems);
                    Db.ProjectVariations.Remove(projectVariation);
                    Db.SaveChanges();
                }
                catch (Exception e)
                {
                    TempData["InfoMessage"] = new InfoMessage { MessageType = InfoMessageType.Warning, MessageContent = "Could not delete project variation. One or more timesheet items are still assigned to it." };
                    return RedirectToAction("Index");
                }
            }
            TempData["InfoMessage"] = new InfoMessage { MessageType = InfoMessageType.Success, MessageContent = "Successfully deleted variation" };
            return RedirectToAction("Index");
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