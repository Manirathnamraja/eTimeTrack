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
    public class ProjectOfficesController : BaseController
    {

        public const string GenericOfficeText = "Default / Unassigned";
        public const string GenericOfficeTextDescription = "Default / Unassigned Office Description";
        public ActionResult Index()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            List<ProjectOffice> defaultOffices = Db.ProjectOffices.ToList();

            if(defaultOffices.Count == 0)
            {
                InsertDefaultOffices();
            }
            
            List<ProjectOffice> offices = Db.ProjectOffices.Where(x => x.ProjectID == projectId || x.OfficeName.StartsWith("Default")).ToList();


            OfficesIndexViewModel vm = new OfficesIndexViewModel 
            {
                Offices = offices 
            };
            return View(vm);
        }

        public void InsertDefaultOffices()
        {
            try
            {
                int projectId = (int?)Session["SelectedProject"] ?? 0;
                ProjectOffice Office = new ProjectOffice
                {
                    OfficeName = GenericOfficeText,
                    LastModifiedBy = UserHelpers.GetCurrentUserId(),
                    LastModifiedDate = DateTime.Now,
                    Description = GenericOfficeTextDescription,
                    ProjectID = 0
                };

                Db.ProjectOffices.Add(Office);
                Db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
        }

        public ActionResult CreateOffice()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            OfficeCreateViewModel model = new OfficeCreateViewModel
            {
                ProjectID = project.ProjectID,
                OfficeName = null
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateOffice(OfficeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            List<ProjectOffice> allExistingOffices = Db.ProjectOffices.Where(x => x.ProjectID == model.ProjectID).ToList();

            InfoMessage message;

            bool validNewText = !allExistingOffices.Select(x => x.OfficeName).Contains(model.OfficeName);

            if (!validNewText)
            {
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = "Text is already taken. Cannot create new office." };
                ViewBag.InfoMessage = message;
                return View(model);
            } 

            ProjectOffice Office = new ProjectOffice
            {
                OfficeName = model.OfficeName,
                Description = model.Description,
                ProjectID = model.ProjectID,
                OfficeId = (int)model.OfficeId,
            };

            Db.ProjectOffices.Add(Office);
            Db.SaveChanges();

            message = new InfoMessage
            {
                MessageType = InfoMessageType.Success,
                MessageContent = "Successfully created new Offices."
            };

            TempData["message"] = message;
            return RedirectToAction("Index");
        }            
    }
}