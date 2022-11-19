using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextAdminOrAbove)]
    public class CompaniesController : BaseController
    {
        public ActionResult Index()
        {
            return View(Db.Companies.OrderBy(x => x.Company_Code).ToList());
        }

        [Authorize(Roles = UserHelpers.RoleSuperUser)]
        public ActionResult CreateEdit(int? id)
        {
            Company company;

            if (id == null)
            {
                company = new Company();
            }
            else
            {
                company = Db.Companies.Find(id);
                if (company == null)
                {
                    return InvokeHttp404(HttpContext);
                }
            }
            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserHelpers.RoleSuperUser)]
        public ActionResult CreateEdit(Company company)
        {
            if (!ModelState.IsValid) return View(company);
            Company existing = Db.Companies.Find(company.Company_Id);

            if (existing != null)
            {
                Db.Entry(existing).CurrentValues.SetValues(company);
                Db.Entry(existing).State = EntityState.Modified;
            }
            else
            {
                Db.Companies.Add(company);
            }

            Db.SaveChanges();

            if (existing != null)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Assign", new { id = company.Company_Id });
        }

        public ActionResult Assign(int? id)
        {
            if (id == null)
            {
                return InvokeHttp400(HttpContext);
            }

            Company company = Db.Companies.Find(id);

            List<Project> availableProjects = GetProjectsAssignedToUser();

            List<ProjectCompany> assignedProjects = Db.ProjectCompanies.Where(x => x.CompanyId == company.Company_Id).ToList();

            var model = new GenericAssignmentModel<Company, Project, ProjectCompany> { AssignmentRecipient = company, AvailableList = availableProjects, AssignedList = assignedProjects };

            return View(model);
        }

        [HttpPost]
        public JsonResult AssignToProject(int? companyId, int? projectId, bool assigned)
        {
            if (companyId == null || projectId == null)
            {
                return Json(false);
            }

            ProjectCompany existing = Db.ProjectCompanies.SingleOrDefault(x => x.CompanyId == companyId && x.ProjectId == projectId);

            if (existing == null)
            {
                if (assigned)
                {
                    ProjectCompany companyProject =
                        new ProjectCompany { CompanyId = (int)companyId, ProjectId = (int)projectId };
                    Db.ProjectCompanies.Add(companyProject);
                }
            }
            else if (!assigned)
            {
                Db.ProjectCompanies.Remove(existing);
            }
            Db.SaveChanges();

            return Json(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
