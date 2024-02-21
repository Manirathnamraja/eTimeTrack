using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;

namespace eTimeTrack.Controllers
{
    [Authorize]
    public class GuidanceNotesController : BaseController
    {
        public ActionResult Index()
        {
            int ProjectId = (int?)Session?["SelectedProject"] ?? 0;
            var results = Db.ProjectGuidanceNotes.Where(x => x.ProjectId == ProjectId).FirstOrDefault();
            if(results != null)
            {
                results.ProjectId = ProjectId;
                ViewBag.InfoMessage = TempData["InfoMessage"];
                return View(results);
            }
            else
            {
                ViewBag.InfoMessage = TempData["InfoMessage"];
                return View(new ProjectGuidanceNotes
                {
                    ProjectId = ProjectId
                });
            }
        }

        [HttpPost]
        public ActionResult Create(ProjectGuidanceNotes notes)
        {
            var existing = Db.ProjectGuidanceNotes.Find(notes.GuidanceNoteId);
            notes.LastModifiedBy = UserHelpers.GetCurrentUserId();
            notes.LastModifiedDate = DateTime.UtcNow;
            if (existing != null)
            {
                Db.Entry(existing).CurrentValues.SetValues(notes);
                Db.Entry(existing).State = EntityState.Modified;
            }
            else
            {
                Db.ProjectGuidanceNotes.Add(notes);
            }
            Db.SaveChanges();
            TempData["InfoMessage"] = new InfoMessage(InfoMessageType.Success, "Succesfully Saved Project Guidance Notes");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult GetNotes()
        {
            int ProjectId = (int?)Session?["SelectedProject"] ?? 0;
            var results = Db.ProjectGuidanceNotes.Where(x => x.ProjectId == ProjectId).ToList();
            return View(new GetNotesViewModel
            {
                GuidanceNotes = results
            });
        }
    }
}