using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class EmployeeProjectsController : BaseEmployeesController
    {
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult Index(int? projectUserTypeId,int? projectDisciplineId)
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            List<EmployeeProject> employees = GetAllProjectEmployeesOrdered(projectId, projectUserTypeId, projectDisciplineId).Where(x => !x.Employee.LockoutEndDateUtc.HasValue && x.Employee.IsActive).ToList();
            
            List<SelectListItem> selectItems = GetProjectUserTypeSelectItems(projectId);

            List<SelectListItem> selectProjectDisciplineItems = GetProjectDisciplineSelectItems(projectId);

            ViewBag.ProjectId = projectId;

            ProjectEmployeeIndexUserVm vm = new ProjectEmployeeIndexUserVm
            {
                EmployeeProjects = employees,
                ProjectUserTypeIdFilter = projectUserTypeId,
                ProjectUserTypes = selectItems,
                //Project Disciplines
                ProjectDisciplineIdFilter = projectDisciplineId,
                 ProjectDisciplines = selectProjectDisciplineItems,
            };
            //NOTE : If projectDisciplineId = NULL, need to update ProjectDisciplineId to 1 in SQL table

            return View(vm);
        }

        public ActionResult Assign(int? id)
        {
            if (id == null)
            {
                return InvokeHttp400(HttpContext);
            }

            Employee employee = Db.Users.Find(id);
            if (employee == null)
            {
                return InvokeHttp404(HttpContext);
            }

            List<Project> availableProjects = GetProjectsAssignedToUser();

            List<EmployeeProject> assignedProjects = Db.EmployeeProjects.Where(x => x.EmployeeId == employee.Id).ToList();

            var model = new GenericAssignmentModel<Employee, Project, EmployeeProject> { AssignmentRecipient = employee, AvailableList = availableProjects, AssignedList = assignedProjects, IsAdmin = UserIsInRole(employee.Id, UserHelpers.RoleAdmin) || UserIsInRole(employee.Id, UserHelpers.RoleSuperUser) };

            return View(model);
        }

        [HttpPost]
        public JsonResult AssignToProject(int? userId, int? projectId, bool assigned)
        {
            if (userId == null || projectId == null)
            {
                return Json(false);
            }

            EmployeeProject existing = Db.EmployeeProjects.SingleOrDefault(x => x.EmployeeId == userId && x.ProjectId == projectId);

            if (existing == null)
            {
                if (assigned)
                {
                    EmployeeProject employeeProject = new EmployeeProject { EmployeeId = (int)userId, ProjectId = (int)projectId };
                    Db.EmployeeProjects.Add(employeeProject);
                }
            }
            else if (!assigned)
            {
                Db.EmployeeProjects.Remove(existing);
            }

            Db.SaveChanges();

            return Json(true);
        }

        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult Details(int? employeeId)
        {
            if (employeeId == null)
            {
                return InvokeHttp400(HttpContext);
            }

            Employee employee = Db.Users.Find(employeeId);
            if (employee == null)
            {
                return InvokeHttp404(HttpContext);
            }

            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            EmployeeProject employeeProject = Db.EmployeeProjects.SingleOrDefault(x => x.EmployeeId == employeeId && x.ProjectId == project.ProjectID);

            EmployeeProjectDetailsViewModel model = new EmployeeProjectDetailsViewModel
            {
                EmployeeID = employee.Id,
                EmployeeNo = employee.EmployeeNo,
                EmailAddress = employee.Email,
                Names = employee.Names,
                ProjectUserTypeID = employeeProject?.ProjectUserTypeID?.ToString(),
                ProjectRole = employeeProject?.ProjectRole,
                ProjectDisciplineID = employeeProject?.ProjectDisciplineID?.ToString()
            };

            List<SelectListItem> selectItems = GetProjectUserTypeSelectItems(projectId);

            SelectList availableProjectUserTypes = new SelectList(selectItems, "Value", "Text", model.ProjectUserTypeID);

            ViewBag.AvailableProjectUserTypes = availableProjectUserTypes;

            //Project Discipline
            List<SelectListItem> selectProjectDisciplineItems = GetProjectDisciplineSelectItems(projectId);

            SelectList availableProjectDisciplines = new SelectList(selectProjectDisciplineItems, "Value", "Text", model.ProjectDisciplineID);

            ViewBag.AvailableProjectDisciplines = availableProjectDisciplines;

            return PartialView(model);
        }

        private List<SelectListItem> GetProjectUserTypeSelectItems(int projectId)
        {
            int? genericProjectUserTypeId = Db.ProjectUserTypes.SingleOrDefault(x => x.ProjectID == projectId && x.UserTypeID == null)?.ProjectUserTypeID;

            List<SelectListItem> selectItems = Db.ProjectUserTypes.Where(x => x.ProjectID == projectId).Include(x => x.UserType).OrderBy(x => x.UserTypeID).Select(x => new SelectListItem { Value = x.ProjectUserTypeID.ToString(), Text = (x.ProjectUserTypeID == genericProjectUserTypeId ? "<GENERIC> " : string.Empty) + ((x.AliasCode ?? x.UserType.Code) + ": " + (x.AliasType ?? x.UserType.Type)) }).ToList();

            SelectListItem genericItem = selectItems.FirstOrDefault(x => x.Text.StartsWith("<GENERIC> "));
            if (genericItem != null)
            {
                genericItem.Text = genericItem.Text.Replace("<GENERIC> ", string.Empty);
                if (genericItem.Text == ": ")
                {
                    genericItem.Text = UserTypesController.GenericUserTypeText;
                }
                selectItems.Remove(genericItem);
                selectItems.Insert(0, genericItem);
            }

            return selectItems;
        }

        private List<SelectListItem> GetProjectDisciplineSelectItems(int projectId)
        {
         
            List<SelectListItem> selectItems = Db.ProjectDisciplines.Where(x => x.ProjectID == projectId || x.ProjectID == 0).Select(x => new SelectListItem { Value = x.ProjectDisciplineId.ToString(), Text = x.Text }).ToList();
            return selectItems;

        }

        [HttpPost]
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public JsonResult Details(EmployeeProjectDetailsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(false);
            }

            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return Json(false);
            }

            EmployeeProject employeeProject = Db.EmployeeProjects.SingleOrDefault(x => x.EmployeeId == model.EmployeeID && x.ProjectId == project.ProjectID);

            var intProjectUserTypeID = string.IsNullOrWhiteSpace(model.ProjectUserTypeID) ? -1 : int.Parse(model.ProjectUserTypeID);
            bool projectUserTypeIsGeneric = !string.IsNullOrWhiteSpace(model.ProjectUserTypeID) && Db.ProjectUserTypes.Any(x => x.ProjectUserTypeID == intProjectUserTypeID && x.UserTypeID == null);
            //Project Disciplines
            var intProjectDisciplineID = string.IsNullOrWhiteSpace(model.ProjectDisciplineID) ? -1 : int.Parse(model.ProjectDisciplineID);
            bool projectDisciplineIsGeneric = !string.IsNullOrWhiteSpace(model.ProjectDisciplineID) && Db.ProjectDisciplines.Any(x => x.ProjectDisciplineId == intProjectDisciplineID);

            InfoMessage message;
            if (employeeProject != null)
            {
                employeeProject.ProjectUserTypeID = projectUserTypeIsGeneric ? null : (!string.IsNullOrWhiteSpace(model.ProjectUserTypeID) ? intProjectUserTypeID : (int?)null);

                employeeProject.ProjectRole = model.ProjectRole;

                employeeProject.ProjectDisciplineID = !projectDisciplineIsGeneric ? null : (!string.IsNullOrWhiteSpace(model.ProjectDisciplineID) ? intProjectDisciplineID : (int?)null);

                Db.SaveChanges();
                message = new InfoMessage
                {
                    MessageType = InfoMessageType.Success,
                    MessageContent = "Successfully assigned project user type to employee."
                };
            }
            else
            {
                message = new InfoMessage
                {
                    MessageType = InfoMessageType.Failure,
                    MessageContent = "Could not assign project user type to employee."
                };
            }

            TempData["message"] = message;
            return Json(true);
        }

        [HttpPost]
        public JsonResult UpdateUserProjectUserType(int? employeeId, int? projectId, int? projectUserTypeId)
        {
            EmployeeProject employeeProject = Db.EmployeeProjects.Single(x => x.EmployeeId == employeeId && x.ProjectId == projectId);

            bool projectUserTypeIsGeneric = !projectUserTypeId.HasValue || Db.ProjectUserTypes.Any(x => x.ProjectUserTypeID == projectUserTypeId && x.UserTypeID == null);

            employeeProject.ProjectUserTypeID = projectUserTypeIsGeneric ? null : projectUserTypeId;
            Db.SaveChanges();

            return Json(true);
        }

        [HttpPost]
        public JsonResult UpdateUserProjectDiscipline(int? employeeId, int? projectId, int? projectDisciplineId)
        {
            EmployeeProject employeeProject = Db.EmployeeProjects.Single(x => x.EmployeeId == employeeId && x.ProjectId == projectId);

            bool projectDisciplineIsGeneric = !projectDisciplineId.HasValue || Db.ProjectDisciplines.Any(x => x.ProjectDisciplineId == projectDisciplineId);

            employeeProject.ProjectDisciplineID = !projectDisciplineIsGeneric ? null : projectDisciplineId;
            Db.SaveChanges();

            return Json(true);
        }
    }
}