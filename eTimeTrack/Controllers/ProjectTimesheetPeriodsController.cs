using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
    public class ProjectTimesheetPeriodsController : BaseController
    {
        public ActionResult Assign()
        {
            int id = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(id) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            List<TimesheetPeriod> periods = GetDbTimesheetPeriods();
            List<ProjectTimesheetPeriodViewModel> vm = new List<ProjectTimesheetPeriodViewModel>();
            List<ProjectTimesheetPeriod> assignedProjects = Db.ProjectTimesheetPeriods.Where(x => x.ProjectID == project.ProjectID).ToList();

            GenericAssignmentModel<Project, TimesheetPeriod, ProjectTimesheetPeriod> model = new GenericAssignmentModel<Project, TimesheetPeriod, ProjectTimesheetPeriod> { AssignmentRecipient = project, AvailableList = periods, AssignedList = assignedProjects };

            return View(model);
        }

        [HttpPost]
        public JsonResult AssignPeriods(int? projectId, int? periodId, bool assigned)
        {
            bool success = true;

            if (projectId == null || periodId == null)
            {
                return Json(false);
            }

            ProjectTimesheetPeriod existing = Db.ProjectTimesheetPeriods.SingleOrDefault(x => x.TimesheetPeriodID == periodId && x.ProjectID == projectId);

            if (existing == null)
            {
                if (assigned)
                {
                    ProjectTimesheetPeriod projectPeriod = new ProjectTimesheetPeriod { TimesheetPeriodID = (int)periodId, ProjectID = (int)projectId };
                    Db.ProjectTimesheetPeriods.Add(projectPeriod);
                }
            }
            else if (!assigned)
            {
                Db.ProjectTimesheetPeriods.Remove(existing);
            }
            else
            {
                success = false;
            }

            Db.SaveChanges();
            return Json(success);
        }
    }
}
