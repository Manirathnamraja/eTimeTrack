using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
    public class ProjectPartsController : BaseController
    {
        public ActionResult CreateEdit(int? id, int? projectId)
        {
            ProjectPart projectPart;
            if (id == null)
            {
                if (projectId != null)
                {
                    projectPart = new ProjectPart { ProjectID = (int)projectId };
                    ViewBag.Source = Source.Create;
                }
                else
                {
                    return InvokeHttp400(HttpContext);
                }
            }
            else
            {
                projectPart = Db.ProjectParts.Find(id);
                if (projectPart == null)
                {
                    return InvokeHttp404(HttpContext);
                }
                ViewBag.Source = Source.Existing;
            }

            SetViewbag(projectPart.ProjectID);

            return View(projectPart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEdit(ProjectPart projectPart, Source source)
        {
            ViewBag.Source = source;
            if (ModelState.IsValid)
            {
                // check for existing part in the system
                bool existingPart = Db.ProjectParts.Any(x => x.ProjectID == projectPart.ProjectID && x.PartNo == projectPart.PartNo && x.PartID != projectPart.PartID);
                if (existingPart)
                {
                    Project project = Db.Projects.Find(projectPart.ProjectID);
                    ViewBag.InfoMessage = new InfoMessage { MessageType = InfoMessageType.Warning, MessageContent = $"Part No {projectPart.PartNo} already exists under project {project?.ProjectNo}. Please choose a different one." };
                    SetViewbag(projectPart.ProjectID);
                    projectPart.Project = project;
                    return View(projectPart);
                }

                ProjectPart existing = Db.ProjectParts.Find(projectPart.PartID);

                if (existing != null)
                {
                    Db.Entry(existing).CurrentValues.SetValues(projectPart);
                    Db.Entry(existing).State = EntityState.Modified;
                }
                else
                {
                    Db.ProjectParts.Add(projectPart);
                }

                Db.SaveChanges();
                TempData["InfoMessage"] = new InfoMessage { MessageContent = $"Project Part {projectPart.PartNo} saved.", MessageType = InfoMessageType.Success };
                return RedirectToAction("Index", "ProjectComponent", new { type = ProjectComponentType.ProjectPart });
            }

            SetViewbag(projectPart.ProjectID);
            return View(projectPart);
        }
        private void SetViewbag(int? projectId)
        {
            ViewBag.ProjectID = new SelectList(Db.Projects, "ProjectID", "Name");
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
