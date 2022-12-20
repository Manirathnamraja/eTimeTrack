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
    public class AECOMUserClassificationsController : BaseController
    {
        public ActionResult Index()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            //List<AECOMUserClassification> defaultaecomUserClassifications = Db.AECOMUserClassifications.ToList();

            //if(defaultaecomUserClassifications.Count == 0)
            //{
            //    InsertDefaultaecomUserClassifications();
            //}
            
            //List<AECOMUserClassification> aecomUserClassifications = Db.AECOMUserClassifications.Where(x => x.ProjectID == projectId).ToList();
            List<AECOMUserClassification> aecomUserClassifications = Db.AECOMUserClassifications.ToList();


            AECOMUserClassificationsIndexViewModel vm = new AECOMUserClassificationsIndexViewModel
            {
                AECOMUserClassifications = aecomUserClassifications 
            };
            return View(vm);
        }
        public ActionResult CreateAECOMUserClassification()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            AECOMUserClassificationCreateViewModel model = new AECOMUserClassificationCreateViewModel
            {
                //ProjectID = project.ProjectID,
                Classification = null
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateAECOMUserClassification(AECOMUserClassificationCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            List<AECOMUserClassification> allExistingaecomUserClassifications = Db.AECOMUserClassifications.ToList();

            InfoMessage message;

            bool validNewText = !allExistingaecomUserClassifications.Select(x => x.Classification).Contains(model.Classification);

            if (!validNewText)
            {
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = "Text is already taken. Cannot create new AECOMUserClassification." };
                ViewBag.InfoMessage = message;
                return View(model);
            } 

            AECOMUserClassification AECOMUserClassification = new AECOMUserClassification
            {
                Classification = model.Classification,
              //  Description = model.Description,
                //ProjectID = model.ProjectID,
                AECOMUserClassificationId = (int)model.AECOMUserClassificationId,
            };

            Db.AECOMUserClassifications.Add(AECOMUserClassification);
            Db.SaveChanges();

            message = new InfoMessage
            {
                MessageType = InfoMessageType.Success,
                MessageContent = "Successfully created new AECOM User Classifications."
            };

            TempData["message"] = message;
            return RedirectToAction("Index");
        }            
    }
}