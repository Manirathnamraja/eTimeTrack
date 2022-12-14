using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using eTimeTrack.Extensions;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using Elmah;
using Spire.Doc;
using Spire.Xls;

namespace eTimeTrack.Controllers
{
    public class BaseController : Controller
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private const string CsvContentType = "text/csv";

        protected readonly ApplicationDbContext Db = new ApplicationDbContext();

        protected List<Project> GetProjectsAssignedToUser(bool includeArchived = false)
        {
            // super users can see everything
            if (User.IsSuperUser())
            {
                return Db.Projects.Where(x => includeArchived || !x.IsArchived).OrderByDescending(x => x.ProjectNo).ToList();
            }

            int id = UserHelpers.GetCurrentUserId();
            List<Project> userProjects = Db.EmployeeProjects.Where(ep => ep.EmployeeId == id && !ep.Project.IsClosed && (includeArchived || !ep.Project.IsArchived) && !ep.Employee.Company.IsClosed && Db.ProjectCompanies.Any(x => x.ProjectId == ep.ProjectId && x.CompanyId == ep.Employee.CompanyID)).Select(x => x.Project).OrderBy(x => x.ProjectNo).ToList();

            return userProjects;
        }

        protected List<int> OpenUserProjectPeriods()
        {
            List<int> openUserProjects = GetProjectsAssignedToUser().Select(x => x.ProjectID).ToList();

            List<int> openUserProjectPeriods = Db.ProjectTimesheetPeriods.Where(x => openUserProjects.Contains(x.ProjectID))
                .Select(x => x.TimesheetPeriodID)
                .Distinct()
                .ToList();

            return openUserProjectPeriods;
        }

        protected bool IsMobileDevice()
        {
            int width = Request.Browser.ScreenPixelsWidth;
            int height = Request.Browser.ScreenPixelsHeight;
            return Request.Browser.IsMobileDevice && width < 720 && height < 1280;
        }

        protected TimesheetPeriod GetCurrentTimesheetPeriod()
        {
            DateTime now = DateTime.UtcNow.Date.AddHours(10); // TODO
            TimesheetPeriod existing = Db.TimesheetPeriods.FirstOrDefault(x => x.StartDate <= now && x.EndDate >= now);
            return existing;
        }

        protected EmployeeTimesheet GetCurrentExistingTimesheet()
        {
            int id = UserHelpers.GetCurrentUserId();
            TimesheetPeriod currentPeriod = GetCurrentTimesheetPeriod();
            EmployeeTimesheet existing =
                Db.EmployeeTimesheets.FirstOrDefault(x => x.EmployeeID == id &&
                                                          x.TimesheetPeriodID == currentPeriod.TimesheetPeriodID);
            return existing;
        }

        private List<Employee> GetUserAvailableUsers()
        {
            List<Employee> availableUsers;
            if (User.IsSuperUser())
            {
                availableUsers = Db.Users.ToList();
            }
            else
            {
                List<int> assignedProjects = GetProjectsAssignedToUser().Select(x => x.ProjectID).ToList();
                availableUsers = Db.Users.Where(x => x.Projects.Select(p => p.ProjectId).Intersect(assignedProjects).Any()).ToList();
            }
            return availableUsers.OrderBy(x => x.Email).ThenBy(x => x.EmployeeNo).ToList();
        }

        protected SelectList GenerateDropdownFromUserList(List<Employee> users)
        {
            return new SelectList(users.OrderBy(x => x.UserName).Select(x => new { Value = x.Id, Text = x.UserName }), "Value", "Text");
        }

        protected SelectList GenerateDropdownUsers()
        {
            return GenerateDropdownFromUserList(GetUserAvailableUsers());
        }
        protected SelectList GenerateDropdownUsersAdmins(int? existingId = null)
        {
            RoleType[] roleTypes = { RoleType.RoleUserPlus, RoleType.RoleAdmin, RoleType.RoleSuperUser };
            List<Employee> users = Db.Users.Where(u => u.Id == existingId || (u.IsActive && u.LockoutDateTimeUtc == null && roleTypes.Select(rt => (int)rt).Intersect(u.Roles.Select(role => role.RoleId)).Any())).ToList();
            return GenerateDropdownFromUserList(users);
        }

        protected SelectList GenerateDropdownUserProjects()
        {
            return new SelectList(GetProjectsAssignedToUser(), "ProjectID", "DisplayName");
        }

        protected SelectList GenerateDropdownReconciliationTemplates()
        {
            List<ReconciliationTemplate> templates = Db.ReconciliationTemplates.Include(x => x.Company).OrderBy(x => x.Name).ToList();

            List<Tuple<int, string>> templatesAdjusted = templates.Select(x => new Tuple<int, string>(x.Id, $"{x.Company.Company_Name}: {x.Name}")).ToList();

            return new SelectList(templatesAdjusted, "Item1", "Item2");
        }

        protected SelectList GenerateDropdownUserPeriodOpenProjects(TimesheetPeriod period)
        {
            return new SelectList(GetOpenProjectsAssignedToUser(period), "ProjectID", "DisplayName");
        }

        protected List<Project> GetOpenProjectsAssignedToUser(TimesheetPeriod period)
        {
            List<Project> userProjects = GetProjectsAssignedToUser();

            List<Project> openPeriodProjects = new List<Project>();

            if (User.IsSuperUser())
            {
                openPeriodProjects = userProjects;
            }
            else
            {
                foreach (Project project in userProjects)
                {
                    ProjectTimesheetPeriod openPeriod = Db.ProjectTimesheetPeriods.SingleOrDefault(x => x.ProjectID == project.ProjectID && x.TimesheetPeriodID == period.TimesheetPeriodID);

                    if (openPeriod != null)
                    {
                        openPeriodProjects.Add(project);
                    }
                }
            }
            return openPeriodProjects;
        }

        protected SelectList GetProjectPartsSelect(int projectId, bool openOnly = false)
        {
            return new SelectList(GetProjectParts(projectId, openOnly), "PartID", "DisplayName");
        }

        protected List<ProjectPart> GetProjectParts(int? projectId, bool openOnly = false)
        {
            return Db.ProjectParts.Where(x => x.ProjectID == projectId && (openOnly || !x.IsClosed)).OrderBy(x => x.PartNo).ToList();
        }

        protected List<Project> GetAssignedProjects(int id)
        {
            Employee employee = Db.Users.SingleOrDefault(x => x.Id == id);
            if (employee == null) return new List<Project>();

            List<ProjectCompany> projectCompanies = Db.ProjectCompanies.Where(x => x.CompanyId == employee.CompanyID && !x.Company.IsClosed).ToList();

            List<EmployeeProject> employeeProjects = Db.EmployeeProjects.Where(ep => ep.EmployeeId == id && !ep.Project.IsClosed && !ep.Project.IsArchived).ToList();

            return employeeProjects.Where(x => projectCompanies.Select(y => y.ProjectId).Contains(x.ProjectId)).Select(x => x.Project).ToList();
        }

        protected List<EmployeeProject> GetAllProjectEmployeesOrdered(int? projectId, int? projectUserTypeId = null, int? projectDisciplineId = null)
        {
            IQueryable<ProjectUserType> projectUserTypes = Db.ProjectUserTypes.Where(x => x.ProjectID == projectId);

            IQueryable<ProjectDiscipline> projectDisciplines = Db.ProjectDisciplines.Where(x => x.ProjectID == projectId);
            int? projectGenericUserTypeId = null;
            int? projectGenericDisciplineId = null;

            if (projectUserTypeId.HasValue)
            {
                projectGenericUserTypeId = projectUserTypes.SingleOrDefault(x => x.UserTypeID == null)?.ProjectUserTypeID;
            }
            if (projectDisciplineId.HasValue)
            {
                projectGenericDisciplineId = projectDisciplines.SingleOrDefault(x=>x.Text.StartsWith("Default"))?.ProjectDisciplineId;
            }

            bool isGeneric = projectUserTypeId == null || projectUserTypeId == projectGenericUserTypeId;
            bool isGenericDiscipline = projectDisciplineId == null || projectDisciplineId == projectGenericDisciplineId;
            return Db.EmployeeProjects.Where(x =>
                x.ProjectId == projectId
                && (
                    projectUserTypeId == null || ((isGeneric ? (x.ProjectUserTypeID == null || x.ProjectUserTypeID == projectGenericUserTypeId) : x.ProjectUserTypeID == projectUserTypeId
                    )))
                && (
                    projectDisciplineId == null || ((isGenericDiscipline ? (x.ProjectDisciplineID == null || x.ProjectDisciplineID == projectGenericDisciplineId) : x.ProjectDisciplineID == projectDisciplineId
                    )))
                ).OrderBy(x => x.Employee.Names).ThenBy(x => x.Employee.EmployeeNo).ToList();
        }

        protected List<Employee> GetAllEmployeesOrdered(int? projectId = null)
        {
            return Db.Users.Include(x => x.Company).Where(x => projectId == null || x.Projects.Any(y => y.ProjectId == projectId)).OrderBy(x => x.Email).ThenBy(x => x.EmployeeNo).AsNoTracking().ToList();
        }

        protected SelectList GetProjectTimesheetPeriodsSelect(int projectId)
        {
            List<TimesheetPeriod> projectDatePeriods = Db.TimesheetPeriods.Where(x => x.EmployeeTimesheet.SelectMany(y => y.TimesheetItems).Any(z => z.ProjectTask.ProjectID == projectId)).Distinct().OrderByDescending(x => x.EndDate).ToList();

            IEnumerable<SelectListItem> selectItems = projectDatePeriods.Select(x => new SelectListItem()
            {
                Value = x.TimesheetPeriodID.ToString(),
                Text = x.EndDate.ToDateStringGeneral()
            });
            SelectList existingPeriods = new SelectList(selectItems, "Value", "Text");

            var texts = selectItems.Select(x => x.Text).ToList();

            return existingPeriods;
        }

        protected List<TimesheetPeriod> GetDbTimesheetPeriods(bool reverse = true)
        {
            if (reverse)
                return Db.TimesheetPeriods.OrderByDescending(x => x.EndDate).ToList();

            return Db.TimesheetPeriods.OrderBy(x => x.EndDate).ToList();
        }

        //https://stackoverflow.com/a/2577095
        protected override void HandleUnknownAction(string actionName)
        {
            // If controller is ErrorController dont 'nest' exceptions
            if (GetType() != typeof(ErrorController))
                InvokeHttp404(HttpContext);
        }

        protected static void ValidateExcelFileImportBasic(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                throw new Exception("Empty file, please try upload again");
            }

            string fileName = file.FileName;
            string fileExtension = Path.GetExtension(fileName);
            if (file.ContentType != ExcelContentType || fileExtension != ".xlsx")
            {
                throw new Exception("Invalid file format - needs to be an excel file (.xlsx extension required)");
            }
        }

        protected static void ValidateCsvFileImportBasic(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                throw new Exception("Empty file, please try upload again");
            }

            string fileName = file.FileName;
            string fileExtension = Path.GetExtension(fileName);
            if (file.ContentType != CsvContentType || fileExtension != ".csv")
            {
                throw new Exception("Invalid file format - needs to be an csv file (.csv extension required)");
            }
        }

        public ActionResult InvokeHttp404(HttpContextBase httpContext)
        {
            ErrorSignal.FromCurrentContext().Raise(new HttpException(404, "Could not find page: " + httpContext.Request.Url?.OriginalString));

            return InvokeError(httpContext, "NotFound");
        }

        public ActionResult InvokeHttp400(HttpContextBase httpContext)
        {
            ErrorSignal.FromCurrentContext().Raise(new HttpException(400, "Bad request on page: " + httpContext.Request.Url?.OriginalString));

            return InvokeError(httpContext, "BadRequest");
        }

        private static ActionResult InvokeError(HttpContextBase httpContext, string actionName)
        {
            IController errorController = new ErrorController();
            RouteData errorRoute = new RouteData();
            errorRoute.Values.Add("controller", "Error");
            errorRoute.Values.Add("action", actionName);
            errorRoute.Values.Add("url", httpContext.Request.Url?.OriginalString);
            errorController.Execute(new RequestContext(httpContext, errorRoute));

            return new EmptyResult();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db.Dispose();
            }
            base.Dispose(disposing);
        }

        protected static FileInfo WritePdfExcel(FileSystemInfo path)
        {
            Workbook wb = new Workbook();
            wb.LoadFromFile(path.FullName);
            string pdfFileName = path.FullName.Replace(".xlsx", ".pdf");
            wb.SaveToFile(pdfFileName, Spire.Xls.FileFormat.PDF);
            return new FileInfo(pdfFileName);
        }

        protected static FileInfo WritePdfWord(FileSystemInfo path)
        {
            Document doc = new Document();
            doc.LoadFromFile(path.FullName);
            string pdfFileName = path.FullName.Replace(".docx", ".pdf");
            doc.SaveToFile(pdfFileName, Spire.Doc.FileFormat.PDF);
            return new FileInfo(pdfFileName);
        }

        protected FileInfo GetGuidFilePath(string extension)
        {
            string folder = Server.MapPath("~/App_Data/Downloads");
            Guid fileNameId = Guid.NewGuid();
            return new FileInfo(Path.Combine(folder, fileNameId + "." + extension));
        }
    }
}