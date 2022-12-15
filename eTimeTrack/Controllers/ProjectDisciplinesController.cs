using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Migrations;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.RoleSuperUser)]
    public class ProjectDisciplinesController : BaseController
    {

        public const string GenericDisciplineText = "Default / Unassigned";
        public const string GenericDisciplineTextDescription = "Default / Unassigned Project Discipline Description";
        public ActionResult Index()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            List<ProjectDiscipline> defaultProjectDisciplines = Db.ProjectDisciplines.ToList();

            if(defaultProjectDisciplines.Count == 0)
            {
                InsertDefaultProjectDisciplines();
            }
            
            List<ProjectDiscipline> projectDisciplines = Db.ProjectDisciplines.Where(x => x.ProjectID == projectId || x.Text.StartsWith("Default")).ToList();


            ProjectDisciplinesIndexViewModel vm = new ProjectDisciplinesIndexViewModel 
            {
                ProjectDisciplines = projectDisciplines 
            };
            return View(vm);
        }

        public void InsertDefaultProjectDisciplines()
        {
            try
            {
                int projectId = (int?)Session["SelectedProject"] ?? 0;
                ProjectDiscipline ProjectDiscipline = new ProjectDiscipline
                {
                    Text = GenericDisciplineText,
                    LastModifiedBy = UserHelpers.GetCurrentUserId(),
                    LastModifiedDate = DateTime.Now,
                    Description = GenericDisciplineTextDescription,
                    ProjectID = 0
                };

                Db.ProjectDisciplines.Add(ProjectDiscipline);
                Db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
        }

        public ActionResult CreateProjectDiscipline()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            ProjectDisciplineCreateViewModel model = new ProjectDisciplineCreateViewModel
            {
                ProjectID = project.ProjectID,
                Text = null
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateProjectDiscipline(ProjectDisciplineCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            List<ProjectDiscipline> allExistingProjectDisciplines = Db.ProjectDisciplines.Where(x => x.ProjectID == model.ProjectID).ToList();

            InfoMessage message;

            bool validNewText = !allExistingProjectDisciplines.Select(x => x.Text).Contains(model.Text);

            if (!validNewText)
            {
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = "Text is already taken. Cannot create new project discipline." };
                ViewBag.InfoMessage = message;
                return View(model);
            } 

            ProjectDiscipline ProjectDiscipline = new ProjectDiscipline
            {
                Text = model.Text,
                Description = model.Description,
                ProjectID = model.ProjectID,
                ProjectDisciplineId = (int)model.ProjectDisciplineId,
            };

            Db.ProjectDisciplines.Add(ProjectDiscipline);
            Db.SaveChanges();

            message = new InfoMessage
            {
                MessageType = InfoMessageType.Success,
                MessageContent = "Successfully created new project disciplines."
            };

            TempData["message"] = message;
            return RedirectToAction("Index");
        }            
    }
}