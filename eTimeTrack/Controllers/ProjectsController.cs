using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using OfficeOpenXml;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserAdministratorOrAboveExcludeTimesheetEditor)]
    public class ProjectsController : BaseController
    {
        [Authorize(Roles = UserHelpers.RoleSuperUser)]
        public ActionResult Index()
        {
            List<Project> projects = GetProjectsAssignedToUser(true);
            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(projects);
        }

        [Authorize(Roles = UserHelpers.RoleSuperUser)]
        public ActionResult CreateEdit(int? id = null)
        {
            if (id == null)
            {
                ViewBag.Source = Source.Create;
                ViewBag.LeadAdminList = GenerateDropdownUsersAdmins();
                return View();
            }

            Project project = Db.Projects.Find(id);
            if (project == null) return InvokeHttp404(HttpContext);
            ViewBag.Source = Source.Existing;
            ViewBag.LeadAdminList = GenerateDropdownUsersAdmins(project.ManagerID);
            return View(project);
        }

        [Authorize(Roles = UserHelpers.RoleSuperUser)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEdit(Project project)
        {
            if (ModelState.IsValid)
            {
                Project existing = Db.Projects.Find(project.ProjectID);

                if (existing != null)
                {
                    bool currentlyArchived = existing.IsArchived;

                    Db.Entry(existing).CurrentValues.SetValues(project);
                    Db.Entry(existing).State = EntityState.Modified;

                    // lockout users that were only assigned to this project if it is being archived
                    if (project.IsArchived && !currentlyArchived)
                    {
                        DateTime now = DateTime.UtcNow;
                        IQueryable<Employee> usersWithThisProjectAsOnlyProject = Db.Users.Where(x => x.Projects.Count == 1 && x.Projects.Any(y => y.ProjectId == project.ProjectID));
                        foreach (Employee employee in usersWithThisProjectAsOnlyProject)
                        {
                            employee.LockoutDateTimeUtc = now;
                            employee.LockoutEndDateUtc = DateTime.MaxValue;
                            employee.LastModifiedBy = UserHelpers.GetCurrentUserId();
                            employee.LastModifiedDate = now;
                        }
                        Db.SaveChanges();
                    }
                }
                else
                {
                    Db.Projects.Add(project);
                    Company aecom = Db.Companies.FirstOrDefault(x => x.Company_Name.StartsWith("AECOM"));
                    aecom?.Projects.Add(new ProjectCompany { Project = project, Company = aecom });

                    TempData["InfoMessage"] = new InfoMessage { MessageContent = "Project successfully created. Company 'AECOM' has been automatically assigned to project. Please assign other companies if needed.", MessageType = InfoMessageType.Success };
                }

                Db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.LeadAdminList = GenerateDropdownUsersAdmins(project.ManagerID);
            return View(project);
        }

        public ActionResult Assign()
        {
            int id = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(id) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            List<Employee> employees = GetAllEmployeesOrdered();
            List<Employee> admins = employees.Where(x => x.Roles.Any(r => r.RoleId == (int)RoleType.RoleAdmin || r.RoleId == (int)RoleType.RoleSuperUser || r.RoleId == (int)RoleType.RoleUserPlus || r.RoleId == (int)RoleType.RoleTimesheetEditor || r.RoleId == (int)RoleType.RoleUserAdministrator || r.RoleId == (int)RoleType.RoleTimesheetApproval)).ToList();

            List<ProjectEmployeesViewModel> employeesVm = new List<ProjectEmployeesViewModel>();
            employees.ForEach(x => employeesVm.Add(new ProjectEmployeesViewModel { Employee = x, IsAdmin = admins.FirstOrDefault(y => y.Id == x.Id) != null, RoleName = GetRoleDetails(x.Roles.Select(y => y.RoleId).FirstOrDefault()) }));


            List<EmployeeProject> assignedProjects = Db.EmployeeProjects.Where(x => x.ProjectId == project.ProjectID).ToList();

            var model = new GenericAssignmentModel<Project, ProjectEmployeesViewModel, EmployeeProject> { AssignmentRecipient = project, AvailableList = employeesVm, AssignedList = assignedProjects };

            return View(model);
        }

        private string GetRoleDetails(int roleid)
        {
            var result = string.Empty;
            switch (roleid)
            {
                case 1:
                    result = string.Empty;
                    break;
                case 2:
                    result = "Administrator";
                    break;
                case 3:
                    result = "SuperUser";
                    break;
                case 4:
                    result = "UserPlus";
                    break;
                case 5:
                    result = "TimesheetEditor";
                    break;
                case 6:
                    result = "UserAdministrator";
                    break;
                case 7:
                    result = "TimesheetApproval";
                    break;
                
            }
            return result;
        }

        [HttpPost]
        public JsonResult AssignUsers(int? projectId, int? userId, bool assigned)
        {
            if (projectId == null || userId == null)
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
        public ActionResult ImportProjectElements()
        {
            ProjectStructureImportViewModel viewModel = new ProjectStructureImportViewModel { ProjectList = GenerateDropdownUserProjects() };
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public ActionResult ImportProjectElements(ProjectStructureImportViewModel model)
        {
            try
            {
                ValidateExcelFileImportBasic(model.File);

                Project project = Db.Projects.Find(model.ProjectId);
                if (project == null)
                {
                    return InvokeHttp400(HttpContext);
                }

                ProjectStructureImportResults results;
                using (ExcelPackage package = new ExcelPackage(model.File.InputStream))
                {
                    // get the first worksheet in the workbook
                    results = ExcelProjectImport.GenerateProjectModelFromExcel(project, package.Workbook.Worksheets[1], Db, model.ConcatenateCodes, model.ConcatenateCharacter);
                }
                TempData["InfoMessage"] = new InfoMessage { MessageContent = $"<p>Upload complete. Added:</p><ul><li>Project Parts: {results.PartsAdded}</li><li>Project Groups: {results.GroupsAdded}</li><li>Project Tasks: {results.TasksAdded}</li></ul>", MessageType = InfoMessageType.Success };
            }
            catch (Exception e)
            {
                TempData["InfoMessage"] = new InfoMessage
                {
                    MessageContent = "Error: could not import project data: " + e.Message,
                    MessageType = InfoMessageType.Failure
                };

            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
        public FileResult DownloadStructureTemplate()
        {
            FileInfo dir = new FileInfo(Server.MapPath("~/Content/Templates/Project Structure Import Template.xlsx"));
            return File(dir.FullName, System.Net.Mime.MediaTypeNames.Application.Octet, "Project Structure Import Template.xlsx");
        }

        [Authorize(Roles = UserHelpers.AuthTextAdminOrAbove)]
        public ActionResult TimeCodeConfig()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Include(x => x.ProjectTimeCodeConfig).SingleOrDefault(x => x.ProjectID == projectId);
            if (project == null)
            {
                return InvokeHttp400(HttpContext);
            }

            ProjectTimeCodeConfig config = Db.ProjectTimeCodeConfigs.Include(x => x.Project).SingleOrDefault(x => x.ProjectID == projectId);

            if (config == null)
            {
                config = new ProjectTimeCodeConfig { Project = project, ProjectID = project.ProjectID };
            }

            TimeCodeConfigVm vm = new TimeCodeConfigVm { ProjectTimeCodeConfig = config };
            return View(vm);
        }

        [Authorize(Roles = UserHelpers.AuthTextAdminOrAbove)]
        [HttpPost]
        public ActionResult TimeCodeConfig(TimeCodeConfigVm vm)
        {
            Project project = Db.Projects.Find(vm.ProjectTimeCodeConfig.ProjectID);
            ProjectTimeCodeConfig config = Db.ProjectTimeCodeConfigs.Find(vm.ProjectTimeCodeConfig.ProjectTimeCodeConfigId);
            if (config == null)
            {
                config = vm.ProjectTimeCodeConfig;
                Db.ProjectTimeCodeConfigs.Add(config);
            }
            else
            {
                Db.Entry(config).CurrentValues.SetValues(vm.ProjectTimeCodeConfig);
            }

            config.Project = project;
            project.ProjectTimeCodeConfig = config;

            Db.SaveChanges();

            project.ProjectTimeCodeConfigId = config.ProjectTimeCodeConfigId;

            Db.SaveChanges();

            TempData["InfoMessage"] = new InfoMessage { MessageContent = "Successfully updated project time code config", MessageType = InfoMessageType.Success };

            return RedirectToAction("Index", "Manage");
        }
    }

}
