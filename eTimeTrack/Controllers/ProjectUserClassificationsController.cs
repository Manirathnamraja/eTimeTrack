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
    [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
    public class ProjectUserClassificationsController : BaseController
    {

        //public const string GenericDisciplineText = "Default / Unassigned";
        //public const string GenericDisciplineTextDescription = "Default / Unassigned Project Discipline Description";
        public ActionResult Index()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            //List<ProjectUserClassification> defaultProjectUserClassifications = Db.ProjectUserClassifications.ToList();

            //if(defaultProjectUserClassifications.Count == 0)
            //{
            //    InsertDefaultProjectUserClassifications();
            //}
            
            List<ProjectUserClassification> projectUserClassifications = Db.ProjectUserClassifications.Where(x => x.ProjectID == projectId).ToList();

            List<SelectListItem> selectAECOMUserClassificationItems = GetAECOMUserClassificationSelectItems();

            ViewBag.ProjectId = projectId;
            //ViewBag.ProjectId = ;

            ProjectUserClassificationsIndexViewModel vm = new ProjectUserClassificationsIndexViewModel 
            {
                ProjectUserClassifications = projectUserClassifications,
                AECOMUserClassifications = selectAECOMUserClassificationItems,
            };
            return View(vm);
        }
        public ActionResult CreateProjectUserClassification()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            ProjectUserClassificationCreateViewModel model = new ProjectUserClassificationCreateViewModel
            {
                ProjectID = project.ProjectID,
                ProjectClassificationText = null
            };

            //Project Discipline
            List<SelectListItem> selectAECOMUserClassificationItems = GetAECOMUserClassificationSelectItems();

            SelectList availableAECOMUserClassifications = new SelectList(selectAECOMUserClassificationItems, "Value", "Text", model.AECOMUserClassificationID);
            ViewBag.AvailableAECOMUserClassifications = availableAECOMUserClassifications;

            return View(model);
        }

        [HttpPost]
        public ActionResult CreateProjectUserClassification(ProjectUserClassificationCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            List<ProjectUserClassification> allExistingProjectUserClassifications = Db.ProjectUserClassifications.Where(x => x.ProjectID == model.ProjectID).ToList();

            InfoMessage message;

            bool validNewText = !allExistingProjectUserClassifications.Select(x => x.ProjectClassificationText).Contains(model.ProjectClassificationText);

            if (!validNewText)
            {
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = "Text is already taken. Cannot create new project discipline." };
                ViewBag.InfoMessage = message;
                return View(model);
            }

            //AECOM User Classifications
            var intAECOMUserClassificationID = string.IsNullOrWhiteSpace(model.AECOMUserClassificationID) ? -1 : int.Parse(model.AECOMUserClassificationID);
            bool aECOMUserClassificationIsGeneric = !string.IsNullOrWhiteSpace(model.AECOMUserClassificationID) && Db.AECOMUserClassifications.Any(x => x.AECOMUserClassificationId == intAECOMUserClassificationID);

            ProjectUserClassification ProjectUserClassification = new ProjectUserClassification
            {
                ProjectClassificationText = model.ProjectClassificationText,
                Description = model.Description,
                ProjectID = model.ProjectID,
                //AECOMUserClassificationID = model.AECOMUserClassificationID,
                AECOMUserClassificationID = !aECOMUserClassificationIsGeneric ? null : (!string.IsNullOrWhiteSpace(model.AECOMUserClassificationID) ? intAECOMUserClassificationID : (int?)null),
                ProjectUserClassificationId = (int)model.ProjectUserClassificationId,
            };

            Db.ProjectUserClassifications.Add(ProjectUserClassification);
            Db.SaveChanges();

            message = new InfoMessage
            {
                MessageType = InfoMessageType.Success,
                MessageContent = "Successfully created new project user classifications."
            };

            TempData["message"] = message;
            return RedirectToAction("Index");
        }

        private List<SelectListItem> GetAECOMUserClassificationSelectItems()
        {

            //List<SelectListItem> selectItems = Db.AECOMUserClassifications.Where(x => x.ProjectID == projectId || x.ProjectID == 0).Select(x => new SelectListItem { Value = x.AECOMUserClassificationId.ToString(), Text = x.Classification }).ToList();
            List<SelectListItem> selectItems = Db.AECOMUserClassifications.Select(x => new SelectListItem { Value = x.AECOMUserClassificationId.ToString(), Text = x.Classification }).ToList();
            return selectItems;

        }

        [HttpPost]
        public JsonResult UpdateAECOMUserClassification(int? aecomUserClassificationID,int? projectUserClassificationId)
        {
            ProjectUserClassification projectUserClassification = Db.ProjectUserClassifications.Single(x => x.ProjectUserClassificationId == projectUserClassificationId);

            bool aECOMUserClassificationIsGeneric = !aecomUserClassificationID.HasValue || Db.AECOMUserClassifications.Any(x => x.AECOMUserClassificationId == aecomUserClassificationID);

            projectUserClassification.AECOMUserClassificationID = !aECOMUserClassificationIsGeneric ? null : aecomUserClassificationID;
            Db.SaveChanges();

            return Json(true);
        }
    }
}