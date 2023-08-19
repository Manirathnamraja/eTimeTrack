using System.Collections.Generic;
using System.Data;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
    public class ProjectPartsController : BaseController
    {
        public ActionResult CreateEdit(int? id, int? projectId)
        {
            ProjectPartsViewModel projectPart = new ProjectPartsViewModel();
            if (id == null)
            {
                if (projectId != null)
                {
                    projectPart.part.ProjectID = (int)projectId;
                    ViewBag.Source = Source.Create;
                }
                else
                {
                    return InvokeHttp400(HttpContext);
                }
            }
            else
            {
                projectPart.part = Db.ProjectParts.Find(id);
                if (projectPart.part == null)
                {
                    return InvokeHttp404(HttpContext);
                }
                ViewBag.Source = Source.Existing;
            }

            projectPart.employees = GetEmployee(projectPart.part.ProjectID);

            SetViewbag();

            return View(projectPart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEdit(ProjectPartsViewModel projectPart, Source source)
        {
            ViewBag.Source = source;
            if (ModelState.IsValid)
            {
                // check for existing part in the system
                bool existingPart = Db.ProjectParts.Any(x => x.ProjectID == projectPart.part.ProjectID && x.PartNo == projectPart.part.PartNo && x.PartID != projectPart.part.PartID);
                if (existingPart)
                {
                    Project project = Db.Projects.Find(projectPart.part.ProjectID);
                    ViewBag.InfoMessage = new InfoMessage { MessageType = InfoMessageType.Warning, MessageContent = $"Part No {projectPart.part.PartNo} already exists under project {project?.ProjectNo}. Please choose a different one."};
                    SetViewbag();
                    projectPart.part.Project = project;
                    return View(projectPart);
                }

                projectPart.part  = Db.ProjectParts.Find(projectPart.part.PartID);

                if (projectPart.part != null)
                {
                    Db.Entry(projectPart.part).CurrentValues.SetValues(projectPart.part);
                    Db.Entry(projectPart.part).State = EntityState.Modified;
                }
                else
                {
                    Db.ProjectParts.Add(projectPart.part);
                }

                Db.SaveChanges();
                TempData["InfoMessage"] = new InfoMessage { MessageContent = $"Project Part {projectPart.part.PartNo} saved.", MessageType = InfoMessageType.Success };
                return RedirectToAction("Index", "ProjectComponent", new { type = ProjectComponentType.ProjectPart });
            }

            SetViewbag();
            return View(projectPart);
        }
        private void SetViewbag()
        {
            ViewBag.ProjectID = new SelectList(Db.Projects, "ProjectID", "Name");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db.Dispose();
            }
            base.Dispose(disposing);
        }

        private SelectList GetEmployee(int projectId) { 
            return new SelectList(GetEmployeeDetails(projectId), "Id", "Names", 1);
        }

        private List<Employee> GetEmployeeDetails(int projectid) { 
            List<Employee> emp = (from p in Db.EmployeeProjects
                                  join e in Db.Users on p.EmployeeId equals e.Id
                                  where p.ProjectId == projectid
                                  select e).ToList();
            return emp;
        }
    }
}
