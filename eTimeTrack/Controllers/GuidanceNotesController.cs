using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
    public class GuidanceNotesController : BaseController
    {
        public ActionResult Index()
        {
            int ProjectId = (int?)Session?["SelectedProject"] ?? 0;

            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(new ProjectGuidanceNotes
            {
                ProjectId = ProjectId
            });
        }

        [HttpPost]
        public ActionResult Create(ProjectGuidanceNotes notes)
        {
            notes.LastModifiedBy = UserHelpers.GetCurrentUserId();
            notes.LastModifiedDate = DateTime.UtcNow;
            Db.ProjectGuidanceNotes.Add(notes);
            Db.SaveChanges();

            TempData["InfoMessage"] = new InfoMessage(InfoMessageType.Success, "Succesfully Saved Project Guidance Notes");
            return RedirectToAction("Index");
        }
    }
}