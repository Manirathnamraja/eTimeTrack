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
    public class ProjectGroupsController : BaseController
    {
        public ActionResult CreateEdit(int? id, int? projectId, int? partId)
        {
            ProjectGroup projectGroup;
            if (id == null)
            {
                if (projectId != null && partId != null)
                {
                    projectGroup = new ProjectGroup { ProjectID = (int)projectId, PartID = (int)partId };
                    ViewBag.Source = Source.Create;
                }
                else
                {
                    return InvokeHttp400(HttpContext);
                }
            }
            else
            {
                projectGroup = Db.ProjectGroups.Find(id);
                if (projectGroup == null)
                {
                    return InvokeHttp404(HttpContext);
                }
                ViewBag.Source = Source.Existing;
            }

            SetViewbag(projectGroup.ProjectID);

            return View(projectGroup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEdit(ProjectGroup projectGroup, Source source)
        {
            ViewBag.Source = source;
            if (ModelState.IsValid)
            {
                // check for existing group in the system
                bool existingGroup = Db.ProjectGroups.Any(x => x.PartID == projectGroup.PartID && x.GroupNo == projectGroup.GroupNo && x.GroupID != projectGroup.GroupID);
                if (existingGroup)
                {
                    ProjectPart part = Db.ProjectParts.Find(projectGroup.PartID);
                    ViewBag.InfoMessage = new InfoMessage { MessageType = InfoMessageType.Warning, MessageContent = $"Group No {projectGroup.GroupNo} already exists under part {part?.PartNo}. Please choose a different one." };
                    SetViewbag(projectGroup.ProjectID);
                    projectGroup.ProjectPart = part;
                    return View(projectGroup);
                }

                ProjectGroup existing = Db.ProjectGroups.Find(projectGroup.GroupID);

                if (existing != null)
                {
                    Db.Entry(existing).CurrentValues.SetValues(projectGroup);
                    Db.Entry(existing).State = EntityState.Modified;
                }
                else
                {
                    Db.ProjectGroups.Add(projectGroup);
                }

                Db.SaveChanges();
                TempData["InfoMessage"] = new InfoMessage { MessageContent = $"Project Group {projectGroup.GroupNo} saved.", MessageType = InfoMessageType.Success };
                return RedirectToAction("Index", "ProjectComponent", new { type = ProjectComponentType.ProjectGroup });
            }

            SetViewbag(projectGroup.ProjectID);

            return View(projectGroup);
        }
        private void SetViewbag(int? projectId)
        {
            ViewBag.PartID = new SelectList(Db.ProjectParts.Where(x => x.ProjectID == projectId), "PartID", "Name");
            ViewBag.ProjectID = new SelectList(Db.Projects, "ProjectID", "Name");
            ViewBag.PM = new SelectList(GetEmployeeDetails(projectId), "Id", "Names");
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