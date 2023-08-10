using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class SearchWBSController : BaseController
    {
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult Index()
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            if (selectedProject == 0) { return InvokeHttp404(HttpContext); }
            var results = from p in Db.ProjectVariationItems
                          join t in Db.ProjectTasks on p.TaskID equals t.TaskID
                          join v in Db.ProjectVariations on p.VariationID equals v.VariationID
                          where t.ProjectID == selectedProject
                          select new SearchWBSViewModel
                          {
                              VarItemIsClosed = p.IsClosed,
                              VariationIsClosed = v.IsClosed,
                              TaskIsClosed = t.IsClosed,
                              TaskName = t.Name,
                              TaskNo = t.TaskNo,
                              VariationName = v.Description,
                              VariationNo = v.VariationNo
                          };
            var res = results.ToList();
            return View(new ShowSearchWPSViewModel
            {
                searchWPS = res
            });
        }
        
    }
}