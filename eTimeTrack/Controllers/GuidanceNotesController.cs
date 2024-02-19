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

            return View(new ProjectGuidanceNotes
            {
                ProjectId = ProjectId
            });
        }
    }
}