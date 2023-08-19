using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
    public class ProjectTasksController : BaseController
    {
        public ActionResult CreateEdit(int? id, int? projectId, int? partId, int? groupId)
        {
            ProjectTask projectTask;
            if (id == null)
            {
                if (projectId != null && groupId != null)
                {
                    projectTask = new ProjectTask { ProjectID = (int)projectId, GroupID = (int)groupId };
                    ViewBag.Source = Source.Create;
                }
                else
                {
                    return InvokeHttp400(HttpContext);
                }
            }
            else
            {
                projectTask = Db.ProjectTasks.Find(id);
                if (projectTask == null)
                {
                    return InvokeHttp404(HttpContext);
                }
                ViewBag.Source = Source.Existing;
            }

            SetViewbag(projectTask.ProjectID, projectTask.ProjectGroup?.PartID ?? partId);

            return View(projectTask);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEdit(ProjectTask projectTask, int? ProjectPartSelector, Source source)
        {
            ViewBag.Source = source;
            if (ModelState.IsValid)
            {
                // check for existing task in the system
                bool existingTask = Db.ProjectTasks.Any(x => x.GroupID == projectTask.GroupID && x.TaskNo == projectTask.TaskNo && x.TaskID != projectTask.TaskID);
                ProjectGroup group = Db.ProjectGroups.Find(projectTask.GroupID);
                if (existingTask)
                {
                    ViewBag.InfoMessage = new InfoMessage { MessageType = InfoMessageType.Warning, MessageContent = $"Task No {projectTask.TaskNo} already exists under group {group?.GroupNo}. Please choose a different one." };
                    SetViewbag(projectTask.ProjectID, ProjectPartSelector);
                    projectTask.ProjectGroup = group;
                    return View(projectTask);
                }

                if (group != null)
                {
                    projectTask.ProjectID = group.ProjectID;
                }

                ProjectTask existing = Db.ProjectTasks.Find(projectTask.TaskID);

                if (existing != null)
                {
                    Db.Entry(existing).CurrentValues.SetValues(projectTask);
                    Db.Entry(existing).State = EntityState.Modified;
                }
                else
                {
                    Db.ProjectTasks.Add(projectTask);
                }

                Db.SaveChanges();
                TempData["InfoMessage"] = new InfoMessage { MessageContent = $"Project Task {projectTask.TaskNo} saved.", MessageType = InfoMessageType.Success };
                return RedirectToAction("Index", "ProjectComponent", new { type = ProjectComponentType.ProjectTask });
            }

            SetViewbag(projectTask.ProjectID, ProjectPartSelector);
            return View(projectTask);
        }

        private void SetViewbag(int? projectId, int? partId)
        {
            ViewBag.GroupID = new SelectList(Db.ProjectGroups.Where(x => x.PartID == partId), "GroupID", "DisplayName");
            ViewBag.PartID = new SelectList(Db.ProjectParts.Where(x => x.ProjectID == projectId), "PartID", "DisplayName", partId);
            ViewBag.ProjectID = new SelectList(Db.Projects, "ProjectID", "DisplayName");
            ViewBag.PMUser = new SelectList(GetEmployeeDetails(projectId), "Id", "Names", 1);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db.Dispose();
            }
            base.Dispose(disposing);
        }

        private List<Employee> GetEmployeeDetails(int? projectid)
        {
            List<Employee> emp = (from p in Db.EmployeeProjects
                                  join e in Db.Users on p.EmployeeId equals e.Id
                                  where p.ProjectId == projectid
                                  select e).ToList();
            return emp;
        }
    }
}
