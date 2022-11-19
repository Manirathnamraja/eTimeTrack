using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using eTimeTrack.Models;
using OfficeOpenXml;

namespace eTimeTrack.ProjectImportPreparation.Cli
{
    public static class ChecksAndHelpers
    {
        public static void CheckTimesheetPeriods(List<TimesheetPeriod> timesheetPeriods, List<string> errors)
        {
            IEnumerable<IGrouping<DateTime, TimesheetPeriod>> startGroups = timesheetPeriods.GroupBy(x => x.StartDate);
            foreach (IGrouping<DateTime, TimesheetPeriod> grp in startGroups.Where(x => x.Count() > 1))
            {
                errors.Add(string.Format("There are {0} timesheet periods with the start date {1}", grp.Count(), grp.Key.ToString("yyyy-MM-dd")));
            }

            IEnumerable<IGrouping<DateTime, TimesheetPeriod>> endGroups = timesheetPeriods.GroupBy(x => x.EndDate);
            foreach (IGrouping<DateTime, TimesheetPeriod> grp in endGroups.Where(x => x.Count() > 1))
            {
                errors.Add(string.Format("There are {0} timesheet periods with the end date {1}", grp.Count(), grp.Key.ToString("yyyy-MM-dd")));
            }
        }

        public static void CheckLinkedEmployees(List<Employee> employees, List<EmployeeTimesheetItem> timesheetItems, List<EmployeeTimesheet> timesheets, List<ProjectGroup> groups, List<ProjectPart> parts, List<Project> projects, List<ProjectTask> tasks, List<ProjectVariationItem> variationItems, List<ProjectVariation> variations, List<TimesheetPeriod> timesheetPeriods, List<string> errors)
        {
            employees.ForEach(
                x =>
                {
                    if (x.LastModifiedBy.HasValue && !employees.Any(y => y.Id == x.LastModifiedBy.Value))
                        errors.Add("Employee doesn't exist in employees.LastModifiedBy: " + x.LastModifiedBy);
                });
            employees.ForEach(
                x =>
                {
                    if (x.ManagerID.HasValue && !employees.Any(y => y.Id == x.ManagerID.Value))
                        errors.Add("Employee doesn't exist in employees.ManagerID: " + x.ManagerID);
                });
            timesheetItems.ForEach(
                x =>
                {
                    if (x.LastModifiedBy.HasValue && !employees.Any(y => y.Id == x.LastModifiedBy.Value))
                        errors.Add("Employee doesn't exist in timesheetItems.LastModifiedBy: " + x.LastModifiedBy);
                });
            timesheets.ForEach(
                x =>
                {
                    if (x.LastModifiedBy.HasValue && !employees.Any(y => y.Id == x.LastModifiedBy.Value))
                        errors.Add("Employee doesn't exist in timesheets.LastModifiedBy: " + x.LastModifiedBy);
                });
            groups.ForEach(
                x =>
                {
                    if (x.LastModifiedBy.HasValue && !employees.Any(y => y.Id == x.LastModifiedBy.Value))
                        errors.Add("Employee doesn't exist in groups.LastModifiedBy: " + x.LastModifiedBy);
                });
            parts.ForEach(
                x =>
                {
                    if (x.LastModifiedBy.HasValue && !employees.Any(y => y.Id == x.LastModifiedBy.Value))
                        errors.Add("Employee doesn't exist in parts.LastModifiedBy: " + x.LastModifiedBy);
                });
            projects.ForEach(
                x =>
                {
                    if (x.LastModifiedBy.HasValue && !employees.Any(y => y.Id == x.LastModifiedBy.Value))
                        errors.Add("Employee doesn't exist in projects.LastModifiedBy: " + x.LastModifiedBy);
                });
            tasks.ForEach(
                x =>
                {
                    if (x.LastModifiedBy.HasValue && !employees.Any(y => y.Id == x.LastModifiedBy.Value))
                        errors.Add("Employee doesn't exist in tasks.LastModifiedBy: " + x.LastModifiedBy);
                });
            variationItems.ForEach(
                x =>
                {
                    if (x.LastModifiedBy.HasValue && !employees.Any(y => y.Id == x.LastModifiedBy.Value))
                        errors.Add("Employee doesn't exist in variationItems.LastModifiedBy: " + x.LastModifiedBy);
                });
            variations.ForEach(
                x =>
                {
                    if (x.LastModifiedBy.HasValue && !employees.Any(y => y.Id == x.LastModifiedBy.Value))
                        errors.Add("Employee doesn't exist in variations.LastModifiedBy: " + x.LastModifiedBy);
                });
            timesheetPeriods.ForEach(
                x =>
                {
                    if (x.LastModifiedBy.HasValue && !employees.Any(y => y.Id == x.LastModifiedBy.Value))
                        errors.Add("Employee doesn't exist in timesheetPeriods.LastModifiedBy: " + x.LastModifiedBy);
                });
            timesheets.ForEach(
                x =>
                {
                    if (!employees.Any(y => y.Id == x.EmployeeID))
                        errors.Add("Employee doesn't exist in timesheets.employeeID: " + x.EmployeeID);
                });
            timesheets.ForEach(
                x =>
                {
                    if (x.ApprovedByID.HasValue && !employees.Any(y => y.Id == x.ApprovedByID.Value))
                        errors.Add("Employee doesn't exist in timesheets.ApprovedByID: " + x.ApprovedByID);
                });
            groups.ForEach(
                x =>
                {
                    if (x.PM.HasValue && !employees.Any(y => y.Id == x.PM.Value))
                        errors.Add("Employee doesn't exist in groups.PM: " + x.PM);
                });
            parts.ForEach(
                x =>
                {
                    if (x.PM.HasValue && !employees.Any(y => y.Id == x.PM.Value))
                        errors.Add("Employee doesn't exist in parts.PM: " + x.PM);
                });
            tasks.ForEach(
                x =>
                {
                    if (x.PM.HasValue && !employees.Any(y => y.Id == x.PM.Value))
                        errors.Add("Employee doesn't exist in tasks.PM: " + x.PM);
                });
            projects.ForEach(
                x =>
                {
                    if (x.DirectorID.HasValue && !employees.Any(y => y.Id == x.DirectorID.Value))
                        errors.Add("Employee doesn't exist in projects.DirectorID: " + x.DirectorID);
                });
            projects.ForEach(
                x =>
                {
                    if (x.ManagerID.HasValue && !employees.Any(y => y.Id == x.ManagerID.Value))
                        errors.Add("Employee doesn't exist in projects.ManagerID: " + x.ManagerID);
                });
            projects.ForEach(
                x =>
                {
                    if (x.ClientContactID.HasValue && !employees.Any(y => y.Id == x.ClientContactID.Value))
                        errors.Add("Employee doesn't exist in projects.ClientContactID: " + x.ClientContactID);
                });
        }

        public static void ResetMerges(params IEnumerable<IMergeable>[] toReset)
        {
            foreach (IEnumerable<IMergeable> list in toReset)
            {
                foreach (IMergeable mergeable in list)
                {
                    mergeable.MergedFields = new Dictionary<string, bool>();
                }
            }
        }

        public static void CheckOfficeCompanies(List<Office> offices, List<Company> companies, List<string> errors)
        {
            foreach (Office office in offices)
            {
                if (!companies.Any(x => x.Company_Id == office.Company_Id))
                    errors.Add(string.Format("Office {0}:{1} doesn't have a corresponding company with id {2}", office.Office_Id, office.Office_Name, office.Company_Id));
            }
        }

        public static void CheckEmployees(List<Employee> employees, List<string> errors)
        {
            Console.WriteLine("Checking Employees...");

            IEnumerable<IGrouping<string, Employee>> emailGroup = employees.Where(x => !string.IsNullOrWhiteSpace(x.Email)).GroupBy(x => x.Email);
            if (emailGroup.Any(x => x.Count() > 1))
                errors.Add("Following emails are duplicated: " + string.Join(";", emailGroup.Where(x => x.Count() > 1).Select(x => x.Key)));

            IEnumerable<IGrouping<string, Employee>> usernameGroup = employees.Where(x => !string.IsNullOrWhiteSpace(x.UserName)).GroupBy(x => x.UserName);
            if (usernameGroup.Any(x => x.Count() > 1))
                errors.Add("Following usernames are duplicated: " + string.Join(";", usernameGroup.Where(x => x.Count() > 1).Select(x => x.Key)));

            IEnumerable<IGrouping<string, Employee>> employeeNumberGroup = employees.Where(x => !string.IsNullOrWhiteSpace(x.EmployeeNo)).GroupBy(x => x.EmployeeNo);
            if (employeeNumberGroup.Any(x => x.Count() > 1))
                errors.Add("Following employee #'s are duplicated: " + string.Join(";", employeeNumberGroup.Where(x => x.Count() > 1).Select(x => x.Key)));

            if (employees.Any(x => string.IsNullOrWhiteSpace(x.EmployeeNo)))
                errors.Add("There are employee's without employee #'s: " + string.Join(";", employees.Where(x => string.IsNullOrWhiteSpace(x.EmployeeNo)).Select(x => x.Id)));

            foreach (Employee employee in employees.Where(x => !string.IsNullOrWhiteSpace(x.Email)))
            {
                try
                {
                    string address = new MailAddress(employee.Email).Address;

                    if (employee.Email.Any(c => c > 128))
                        errors.Add(string.Format("Email address for user {0}:{1} is not valid: {2}", employee.Id, employee.EmployeeNo, employee.Email));
                }
                catch (FormatException)
                {
                    errors.Add(string.Format("Email address for user {0}:{1} is not valid: {2}", employee.Id, employee.EmployeeNo, employee.Email));
                }
            }
        }

        public static void CheckAndUpdateVariationItems(ApplicationDbContext dal, List<ProjectVariationItem> variationItems)
        {
            List<ProjectVariationItem> toRemove = new List<ProjectVariationItem>();
            foreach (ProjectVariationItem variationItem in variationItems)
            {
                ProjectVariationItem existing = dal.ProjectVariationItems.SingleOrDefault(x => x.TaskID == variationItem.TaskID && x.VariationID == variationItem.VariationID);
                if (existing != null)
                {
                    toRemove.Add(variationItem);
                }
            }
            foreach (ProjectVariationItem item in toRemove)
            {
                variationItems.Remove(item);
            }
        }

        public static string GetEmployeeUsername(Employee employee)
        {
            if (!string.IsNullOrWhiteSpace(employee.Email))
                return employee.Email;

            if (!string.IsNullOrWhiteSpace(employee.EmployeeNo))
                return employee.EmployeeNo;

            throw new Exception("Employee doesn't have an employee number or email! " + employee.Id);
        }

        public static string CleanSqlString(string str)
        {
            return str.Replace(", ,", ", NULL,").Replace(" False,", " 'FALSE',").Replace(" True,", " 'TRUE',").Replace(", ,", ", NULL,");
        }

        public static void CheckDuplicateTimesheetIds(List<EmployeeTimesheet> timesheets, List<string> errors)
        {
            Console.WriteLine("Checking for duplicate timesheet ids");
            IEnumerable<IGrouping<int, EmployeeTimesheet>> testDuplicateTimesheetIds = timesheets.GroupBy(x => x.TimesheetID);
            if (testDuplicateTimesheetIds.Any(x => x.Count() > 1))
                errors.Add("There are more than one timesheets with the same id: " + string.Join(";", testDuplicateTimesheetIds.Where(x => x.Count() > 1).Select(x => x.Key)));

            IEnumerable<IGrouping<string, EmployeeTimesheet>> testDuplicateTimesheetPeriods = timesheets.GroupBy(x => x.EmployeeID.ToString() + ":" + x.TimesheetPeriodID.ToString());
            if (testDuplicateTimesheetPeriods.Any(x => x.Count() > 1))
                errors.Add("The employee has more than one timesheet for the period: " + string.Join(";", testDuplicateTimesheetPeriods.Where(x => x.Count() > 1).Select(x => x.Key)));
        }

        public static void FixTasks(List<ProjectTask> tasks)
        {
            Console.WriteLine("Fixing Tasks");

            foreach (ProjectTask task in tasks)
            {
                if (task.PM.HasValue && task.PM.Value == 0)
                    task.PM = null;
            }
        }

        public static void FixGroups(List<ProjectGroup> groups)
        {
            Console.WriteLine("Fixing Groups");

            foreach (ProjectGroup grp in groups)
            {
                if (grp.PM.HasValue && grp.PM.Value == 0)
                    grp.PM = null;
            }
        }

        public static void FixParts(List<ProjectPart> parts)
        {
            Console.WriteLine("Fixing Parts");

            foreach (ProjectPart part in parts)
            {
                if (part.PM.HasValue && part.PM.Value == 0)
                    part.PM = null;
            }
        }

        public static void FixProjects(List<Project> projects)
        {
            Console.WriteLine("Fixing Projects");

            foreach (Project project in projects)
            {
                if (project.OfficeID.HasValue && project.OfficeID.Value == 0)
                    project.OfficeID = null;
            }
        }

        public static void FixEmployees(List<Employee> employees)
        {
            Console.WriteLine("Fixing Employees");

            foreach (Employee employee in employees)
            {
                if (employee.LastModifiedBy.HasValue && employee.LastModifiedBy.Value == 0)
                    employee.LastModifiedBy = null;

                employee.EmailConfirmed = true;
                employee.PhoneNumberConfirmed = false;
                employee.TwoFactorEnabled = false;
                employee.LockoutEnabled = true;
                employee.AccessFailedCount = 0;
                employee.UserName = string.IsNullOrWhiteSpace(employee.Email) ? employee.EmployeeNo : employee.Email;
            }
        }

        public static void CheckAndUpdateTasks(ApplicationDbContext dal, List<ProjectTask> tasks, List<ProjectVariationItem> variationItems, List<EmployeeTimesheetItem> timesheetItems)
        {
            List<int> toRemove = new List<int>();
            int nextIdInsert = Math.Max(dal.ProjectTasks.Max(x => x.TaskID), tasks.Max(x => x.TaskID)) + 1;
            foreach (ProjectTask task in tasks.OrderBy(x => x.TaskID))
            {
                ProjectTask existing = dal.ProjectTasks.SingleOrDefault(x => x.ProjectID == task.ProjectID && x.GroupID == task.GroupID && x.TaskNo == task.TaskNo);
                if (existing == null)
                {
                    //No existing company, need to check id, and update if needed
                    //See if the id exists
                    if (dal.ProjectTasks.Any(x => x.TaskID == task.TaskID))
                    {
                        UpdateGenericColumn(variationItems, "TaskID", task.TaskID, nextIdInsert);
                        UpdateGenericColumn(timesheetItems, "TaskID", task.TaskID, nextIdInsert);

                        task.TaskID = nextIdInsert;

                        nextIdInsert++;
                    }
                }
                else
                {
                    //Existing company, need to check id and update if needed
                    if (existing.TaskID != task.TaskID)
                    {
                        int newId = existing.TaskID;

                        UpdateGenericColumn(variationItems, "TaskID", task.TaskID, newId);
                        UpdateGenericColumn(timesheetItems, "TaskID", task.TaskID, newId);
                    }
                    toRemove.Add(task.TaskID);
                }
            }
            foreach (int i in toRemove)
            {
                tasks.RemoveAll(x => x.TaskID == i);
            }
        }

        public static void CheckAndUpdateGroups(ApplicationDbContext dal, List<ProjectGroup> groups, List<ProjectTask> tasks)
        {
            List<int> toRemove = new List<int>();
            int nextIdInsert = Math.Max(dal.ProjectGroups.Max(x => x.GroupID), groups.Max(x => x.GroupID)) + 1;
            foreach (ProjectGroup grp in groups.OrderBy(x => x.GroupID))
            {
                ProjectGroup existing = dal.ProjectGroups.SingleOrDefault(x => x.ProjectID == grp.ProjectID && x.PartID == grp.PartID && x.GroupNo == grp.GroupNo);
                if (existing == null)
                {
                    //No existing company, need to check id, and update if needed
                    //See if the id exists
                    if (dal.ProjectGroups.Any(x => x.GroupID == grp.GroupID))
                    {
                        UpdateGenericColumn(tasks, "GroupID", grp.GroupID, nextIdInsert);

                        grp.GroupID = nextIdInsert;

                        nextIdInsert++;
                    }
                }
                else
                {
                    //Existing company, need to check id and update if needed
                    if (existing.GroupID != grp.GroupID)
                    {
                        int newId = existing.GroupID;

                        UpdateGenericColumn(tasks, "GroupID", grp.GroupID, newId);
                    }
                    toRemove.Add(grp.GroupID);
                }
            }
            foreach (int i in toRemove)
            {
                groups.RemoveAll(x => x.GroupID == i);
            }
        }

        public static void CheckAndUpdateParts(ApplicationDbContext dal, List<ProjectPart> parts, List<ProjectGroup> groups)
        {
            List<int> toRemove = new List<int>();
            int nextIdInsert = Math.Max(dal.ProjectParts.Max(x => x.PartID), parts.Max(x => x.PartID)) + 1;
            foreach (ProjectPart part in parts.OrderBy(x => x.PartID))
            {
                ProjectPart existing = dal.ProjectParts.SingleOrDefault(x => x.ProjectID == part.ProjectID && x.PartNo == part.PartNo);
                if (existing == null)
                {
                    //No existing company, need to check id, and update if needed
                    //See if the id exists
                    if (dal.ProjectParts.Any(x => x.PartID == part.PartID))
                    {
                        UpdateGenericColumn(groups, "PartID", part.PartID, nextIdInsert);

                        part.PartID = nextIdInsert;

                        nextIdInsert++;
                    }
                }
                else
                {
                    //Existing company, need to check id and update if needed
                    if (existing.PartID != part.PartID)
                    {
                        int newId = existing.PartID;

                        UpdateGenericColumn(groups, "PartID", part.PartID, newId);
                    }
                    toRemove.Add(part.PartID);
                }
            }
            foreach (int i in toRemove)
            {
                parts.RemoveAll(x => x.PartID == i);
            }
        }

        public static void CheckAndUpdateVariations(ApplicationDbContext dal, List<ProjectVariation> variations, List<ProjectVariationItem> variationItems, List<EmployeeTimesheetItem> timesheetItems)
        {
            List<int> toRemove = new List<int>();
            int nextIdInsert = Math.Max(dal.ProjectVariations.Max(x => x.VariationID), variations.Max(x => x.VariationID)) + 1;
            foreach (ProjectVariation variation in variations.OrderBy(x => x.VariationID))
            {
                ProjectVariation existing = dal.ProjectVariations.SingleOrDefault(x => x.ProjectID == variation.ProjectID && x.VariationNo == variation.VariationNo);
                if (existing == null)
                {
                    //No existing company, need to check id, and update if needed
                    //See if the id exists
                    if (dal.ProjectVariations.Any(x => x.VariationID == variation.VariationID))
                    {
                        UpdateGenericColumn(variationItems, "VariationID", variation.VariationID, nextIdInsert);
                        UpdateGenericColumn(timesheetItems, "VariationID", variation.VariationID, nextIdInsert);

                        variation.VariationID = nextIdInsert;

                        nextIdInsert++;
                    }
                }
                else
                {
                    //Existing company, need to check id and update if needed
                    if (existing.VariationID != variation.VariationID)
                    {
                        int newId = existing.VariationID;

                        UpdateGenericColumn(variationItems, "VariationID", variation.VariationID, newId);
                        UpdateGenericColumn(timesheetItems, "VariationID", variation.VariationID, newId);
                    }
                    toRemove.Add(variation.VariationID);
                }
            }
            foreach (int i in toRemove)
            {
                variations.RemoveAll(x => x.VariationID == i);
            }
        }

        public static void CheckAndUpdateProjects(ApplicationDbContext dal, List<Project> projects, List<ProjectGroup> groups, List<ProjectPart> parts, List<ProjectTask> tasks, List<ProjectVariation> variations)
        {
            int nextIdInsert = Math.Max(dal.Projects.Max(x => x.ProjectID), projects.Max(x => x.ProjectID)) + 1;
            foreach (Project project in projects.OrderBy(x => x.ProjectID))
            {
                Project existing = dal.Projects.SingleOrDefault(x => x.ProjectNo == project.ProjectNo);
                if (existing == null)
                {
                    //No existing company, need to check id, and update if needed
                    //See if the id exists
                    if (dal.Projects.Any(x => x.ProjectID == project.ProjectID))
                    {
                        UpdateGenericColumn(groups, "ProjectID", project.ProjectID, nextIdInsert);
                        UpdateGenericColumn(parts, "ProjectID", project.ProjectID, nextIdInsert);
                        UpdateGenericColumn(tasks, "ProjectID", project.ProjectID, nextIdInsert);
                        UpdateGenericColumn(variations, "ProjectID", project.ProjectID, nextIdInsert);

                        project.ProjectID = nextIdInsert;

                        nextIdInsert++;
                    }
                }
                else
                {
                    throw new Exception("Cannot insert new project when same one already exists...");
                }
            }
        }

        public static void CheckAndUpdateOffices(ApplicationDbContext dal, List<Office> offices, List<Employee> employees, List<Project> projects)
        {
            if (offices == null || offices.Count == 0)
                return;

            List<int> toRemove = new List<int>();
            int nextIdInsert = Math.Max(dal.Offices.Max(x => x.Office_Id), offices.Max(x => x.Office_Id)) + 1;
            foreach (Office office in offices.OrderBy(x => x.Office_Id))
            {
                Office existing = dal.Offices.SingleOrDefault(x => x.Office_Code.Equals(office.Office_Code, StringComparison.InvariantCultureIgnoreCase));
                if (existing == null)
                {
                    //No existing company, need to check id, and update if needed
                    //See if the id exists
                    if (dal.Offices.Any(x => x.Office_Id == office.Office_Id))
                    {
                        UpdateGenericColumn(employees, "OfficeID", office.Office_Id, nextIdInsert);
                        UpdateGenericColumn(projects, "OfficeID", office.Office_Id, nextIdInsert);

                        office.Office_Id = nextIdInsert;

                        nextIdInsert++;
                    }
                }
                else
                {
                    //Existing company, need to check id and update if needed
                    if (existing.Office_Id != office.Office_Id)
                    {
                        int newId = existing.Office_Id;

                        UpdateGenericColumn(employees, "OfficeID", office.Office_Id, newId);
                        UpdateGenericColumn(projects, "OfficeID", office.Office_Id, newId);
                    }
                    toRemove.Add(office.Office_Id);
                }
            }
            foreach (int i in toRemove)
            {
                offices.RemoveAll(x => x.Office_Id == i);
            }
        }

        public static void CheckAndUpdateTimesheetPeriods(ApplicationDbContext dal, List<TimesheetPeriod> timesheetPeriods, List<EmployeeTimesheet> timesheets, List<string> errors)
        {
            List<int> toRemove = new List<int>();
            int nextIdInsert = Math.Max(dal.TimesheetPeriods.Max(x => x.TimesheetPeriodID), timesheetPeriods.Max(x => x.TimesheetPeriodID)) + 1;
            foreach (TimesheetPeriod timesheetPeriod in timesheetPeriods.OrderBy(x => x.TimesheetPeriodID))
            {
                TimesheetPeriod existing = dal.TimesheetPeriods.SingleOrDefault(x =>
                            x.StartDate == timesheetPeriod.StartDate || x.EndDate == timesheetPeriod.EndDate);
                if (existing == null)
                {
                    //No existing company, need to check id, and update if needed
                    //See if the id exists
                    if (dal.TimesheetPeriods.Any(x => x.TimesheetPeriodID == timesheetPeriod.TimesheetPeriodID))
                    {
                        UpdateGenericColumn(timesheets, "TimesheetPeriodID", timesheetPeriod.TimesheetPeriodID, nextIdInsert);

                        timesheetPeriod.TimesheetPeriodID = nextIdInsert;

                        nextIdInsert++;
                    }
                }
                else
                {
                    if (timesheetPeriod.StartDate != existing.StartDate || timesheetPeriod.EndDate != existing.EndDate)
                        errors.Add(
                            string.Format(
                                "Timesheet period {0}:{1}:{2} does not match StartDate/EndDate of existing timesheet period {3}:{4}:{5}",
                                timesheetPeriod.TimesheetPeriodID,
                                timesheetPeriod.StartDate.ToString("yyyy-MM-dd"),
                                timesheetPeriod.EndDate.ToString("yyyy-MM-dd"),
                                existing.TimesheetPeriodID,
                                existing.StartDate.ToString("yyyy-MM-dd"),
                                existing.EndDate.ToString("yyyy-MM-dd")));

                    //Existing company, need to check id and update if needed
                    if (existing.TimesheetPeriodID != timesheetPeriod.TimesheetPeriodID)
                    {
                        int newId = existing.TimesheetPeriodID;

                        UpdateGenericColumn(timesheets, "TimesheetPeriodID", timesheetPeriod.TimesheetPeriodID, newId);
                    }
                    toRemove.Add(timesheetPeriod.TimesheetPeriodID);
                }
            }
            foreach (int i in toRemove)
            {
                timesheetPeriods.RemoveAll(x => x.TimesheetPeriodID == i);
            }
        }

        public static void CheckAndUpdateTimesheets(ApplicationDbContext dal, List<EmployeeTimesheet> timesheets, List<EmployeeTimesheetItem> timesheetItems)
        {
            List<int> toRemove = new List<int>();
            int nextIdInsert = Math.Max(dal.EmployeeTimesheets.Max(x => x.TimesheetID), timesheets.Max(x => x.TimesheetID)) + 1;
            foreach (EmployeeTimesheet timesheet in timesheets.OrderBy(x => x.TimesheetID))
            {
                EmployeeTimesheet existing = dal.EmployeeTimesheets.SingleOrDefault(x => x.EmployeeID == timesheet.EmployeeID && x.TimesheetPeriodID == timesheet.TimesheetPeriodID);
                if (existing == null)
                {
                    //No existing company, need to check id, and update if needed
                    //See if the id exists
                    if (dal.EmployeeTimesheets.Any(x => x.TimesheetID == timesheet.TimesheetID))
                    {
                        UpdateGenericColumn(timesheetItems, "TimesheetID", timesheet.TimesheetID, nextIdInsert);

                        timesheet.TimesheetID = nextIdInsert;

                        nextIdInsert++;
                    }
                }
                else
                {
                    //Existing company, need to check id and update if needed
                    if (existing.TimesheetID != timesheet.TimesheetID)
                    {
                        int newId = existing.TimesheetID;

                        UpdateGenericColumn(timesheetItems, "TimesheetID", timesheet.TimesheetID, newId);
                    }
                    toRemove.Add(timesheet.TimesheetID);
                }
            }
            foreach (int i in toRemove)
            {
                timesheets.RemoveAll(x => x.TimesheetID == i);
            }
        }

        public static void CheckAndUpdateEmployees(ApplicationDbContext dal, List<Employee> employees, List<EmployeeTimesheetItem> timesheetItems, List<EmployeeTimesheet> timesheets, List<ProjectGroup> groups, List<ProjectPart> parts, List<Project> projects, List<ProjectTask> tasks, List<ProjectVariationItem> variationItems, List<ProjectVariation> variations, List<TimesheetPeriod> timesheetPeriods, List<int> employeesToAddToProject, List<string> errors, List<string> warnings)
        {
            List<int> toRemove = new List<int>();
            int nextIdInsert = Math.Max(dal.Users.Max(x => x.Id), employees.Max(x => x.Id)) + 1;
            foreach (Employee employee in employees.OrderBy(x => x.Id))
            {
                Employee existing = dal.Users.SingleOrDefault(x => x.EmployeeNo.Equals(employee.EmployeeNo, StringComparison.InvariantCultureIgnoreCase));
                if (existing == null)
                {
                    //No existing company, need to check id, and update if needed
                    //See if the id exists
                    if (dal.Users.Any(x => x.Id == employee.Id))
                    {
                        UpdateGenericColumn(employees, "ManagerID", employee.Id, nextIdInsert);
                        UpdateGenericColumn(employees, "LastModifiedBy", employee.Id, nextIdInsert);
                        UpdateGenericColumn(timesheetItems, "LastModifiedBy", employee.Id, nextIdInsert);
                        UpdateGenericColumn(timesheets, "EmployeeID", employee.Id, nextIdInsert);
                        UpdateGenericColumn(timesheets, "ApprovedByID", employee.Id, nextIdInsert);
                        UpdateGenericColumn(timesheets, "LastModifiedBy", employee.Id, nextIdInsert);
                        UpdateGenericColumn(groups, "LastModifiedBy", employee.Id, nextIdInsert);
                        UpdateGenericColumn(groups, "PM", employee.Id, nextIdInsert);
                        UpdateGenericColumn(parts, "LastModifiedBy", employee.Id, nextIdInsert);
                        UpdateGenericColumn(parts, "PM", employee.Id, nextIdInsert);
                        UpdateGenericColumn(projects, "LastModifiedBy", employee.Id, nextIdInsert);
                        UpdateGenericColumn(projects, "DirectorID", employee.Id, nextIdInsert);
                        UpdateGenericColumn(projects, "ManagerID", employee.Id, nextIdInsert);
                        UpdateGenericColumn(projects, "ClientContactID", employee.Id, nextIdInsert);
                        UpdateGenericColumn(tasks, "LastModifiedBy", employee.Id, nextIdInsert);
                        UpdateGenericColumn(tasks, "PM", employee.Id, nextIdInsert);
                        UpdateGenericColumn(variationItems, "LastModifiedBy", employee.Id, nextIdInsert);
                        UpdateGenericColumn(variations, "LastModifiedBy", employee.Id, nextIdInsert);
                        UpdateGenericColumn(timesheetPeriods, "LastModifiedBy", employee.Id, nextIdInsert);

                        employee.Id = nextIdInsert;

                        nextIdInsert++;
                    }
                    employeesToAddToProject.Add(employee.Id);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(employee.Email) && !string.IsNullOrWhiteSpace(existing.Email) &&
                        !employee.Email.Equals(existing.Email, StringComparison.InvariantCultureIgnoreCase))
                    {
                        errors.Add(string.Format("User {0}:{1} does not match existing user {2}:{3}", employee.EmployeeNo, employee.Email, existing.EmployeeNo, existing.Email));
                        continue;
                    }

                    if (employee.Email != existing.Email || (!string.IsNullOrWhiteSpace(employee.Email) && !employee.Email.Equals(existing.Email, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        warnings.Add(string.Format("User {0}:{1} does not have the same email as existing user {2}:{3}", employee.EmployeeNo, employee.Email, existing.EmployeeNo, existing.Email));
                    }

                    if (!string.IsNullOrWhiteSpace(employee.Email) &&
                        dal.Users.Any(
                            x =>
                                x.Email.Equals(employee.Email, StringComparison.InvariantCultureIgnoreCase) &&
                                !x.EmployeeNo.Equals(employee.EmployeeNo)))
                    {
                        Employee oldUser = dal.Users.First(x => x.Email.Equals(employee.Email, StringComparison.InvariantCultureIgnoreCase) && !x.EmployeeNo.Equals(employee.EmployeeNo));
                        errors.Add(string.Format("Import employee {0}:{1}:{2} doesn't match existing employee {3}:{4}:{5}", existing.Id, existing.EmployeeNo, existing.Email, oldUser.Id, oldUser.EmployeeNo, oldUser.Email));
                        continue;
                    }
                    //Existing company, need to check id and update if needed
                    if (existing.Id != employee.Id)
                    {
                        int newId = existing.Id;

                        UpdateGenericColumn(employees, "ManagerID", employee.Id, newId);
                        UpdateGenericColumn(employees, "LastModifiedBy", employee.Id, newId);
                        UpdateGenericColumn(timesheetItems, "LastModifiedBy", employee.Id, newId);
                        UpdateGenericColumn(timesheets, "EmployeeID", employee.Id, newId);
                        UpdateGenericColumn(timesheets, "ApprovedByID", employee.Id, newId);
                        UpdateGenericColumn(timesheets, "LastModifiedBy", employee.Id, newId);
                        UpdateGenericColumn(groups, "LastModifiedBy", employee.Id, newId);
                        UpdateGenericColumn(groups, "PM", employee.Id, newId);
                        UpdateGenericColumn(parts, "LastModifiedBy", employee.Id, newId);
                        UpdateGenericColumn(parts, "PM", employee.Id, newId);
                        UpdateGenericColumn(projects, "LastModifiedBy", employee.Id, newId);
                        UpdateGenericColumn(projects, "DirectorID", employee.Id, newId);
                        UpdateGenericColumn(projects, "ManagerID", employee.Id, newId);
                        UpdateGenericColumn(projects, "ClientContactID", employee.Id, newId);
                        UpdateGenericColumn(tasks, "LastModifiedBy", employee.Id, newId);
                        UpdateGenericColumn(tasks, "PM", employee.Id, newId);
                        UpdateGenericColumn(variationItems, "LastModifiedBy", employee.Id, newId);
                        UpdateGenericColumn(variations, "LastModifiedBy", employee.Id, newId);
                        UpdateGenericColumn(timesheetPeriods, "LastModifiedBy", employee.Id, newId);
                    }
                    toRemove.Add(employee.Id);
                    employeesToAddToProject.Add(existing.Id);
                }
            }
            foreach (int i in toRemove)
            {
                employees.RemoveAll(x => x.Id == i);
            }

            //Cross check
            foreach (Employee employee in employees)
            {
                if (dal.Users.Any(x => x.EmployeeNo.Equals(employee.EmployeeNo, StringComparison.InvariantCultureIgnoreCase)))
                    errors.Add("Employee # slipped through: " + employee.EmployeeNo);

                if (!string.IsNullOrWhiteSpace(employee.Email) && dal.Users.Any(x => x.Email.Equals(employee.Email, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Employee existing = dal.Users.First(x => x.Email.Equals(employee.Email));
                    errors.Add(string.Format("Import employee {0}:{1}:{2} doesn't match existing employee {3}:{4}:{5}", employee.Id, employee.EmployeeNo, employee.Email, existing.Id, existing.EmployeeNo, existing.Email));
                }

                if (!string.IsNullOrWhiteSpace(employee.UserName) && dal.Users.Any(x => x.UserName.Equals(employee.UserName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Employee existing = dal.Users.First(x => x.UserName.Equals(employee.UserName, StringComparison.InvariantCultureIgnoreCase));
                    errors.Add(string.Format("Import employee {0}:{1}:{2}:{6} doesn't match existing employee {3}:{4}:{5}:{7}", employee.Id, employee.EmployeeNo, employee.Email, existing.Id, existing.EmployeeNo, existing.Email, employee.UserName, existing.UserName));
                }
            }
        }

        public static void CheckAndUpdateEmployees(ApplicationDbContext dal, List<Employee> employees, List<int> employeesToAddToProject, List<string> errors, List<string> warnings)
        {
            List<int> toRemove = new List<int>();
            int nextIdInsert = Math.Max(dal.Users.Max(x => x.Id), employees.Max(x => x.Id)) + 1;
            foreach (Employee employee in employees.OrderBy(x => x.Id))
            {
                Employee existing = dal.Users.SingleOrDefault(x => x.EmployeeNo.Equals(employee.EmployeeNo, StringComparison.InvariantCultureIgnoreCase));
                if (existing == null)
                {
                    //No existing company, need to check id, and update if needed
                    //See if the id exists
                    if (dal.Users.Any(x => x.Id == employee.Id))
                    {
                        UpdateGenericColumn(employees, "ManagerID", employee.Id, nextIdInsert);
                        UpdateGenericColumn(employees, "LastModifiedBy", employee.Id, nextIdInsert);

                        employee.Id = nextIdInsert;

                        nextIdInsert++;
                    }
                    employeesToAddToProject.Add(employee.Id);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(employee.Email) && !string.IsNullOrWhiteSpace(existing.Email) &&
                        !employee.Email.Equals(existing.Email, StringComparison.InvariantCultureIgnoreCase))
                    {
                        errors.Add(string.Format("User {0}:{1} does not match existing user {2}:{3}", employee.EmployeeNo, employee.Email, existing.EmployeeNo, existing.Email));
                        continue;
                    }

                    if (!employee.Email.Equals(existing.Email, StringComparison.InvariantCultureIgnoreCase))
                    {
                        warnings.Add(string.Format("User {0}:{1} does not have the same email as existing user {2}:{3}", employee.EmployeeNo, employee.Email, existing.EmployeeNo, existing.Email));
                    }

                    if (!string.IsNullOrWhiteSpace(employee.Email) &&
                        dal.Users.Any(
                            x =>
                                x.Email.Equals(employee.Email, StringComparison.InvariantCultureIgnoreCase) &&
                                !x.EmployeeNo.Equals(employee.EmployeeNo)))
                    {
                        Employee oldUser = dal.Users.First(x => x.Email.Equals(employee.Email, StringComparison.InvariantCultureIgnoreCase) && !x.EmployeeNo.Equals(employee.EmployeeNo));
                        errors.Add(string.Format("Import employee {0}:{1}:{2} doesn't match existing employee {3}:{4}:{5}", existing.Id, existing.EmployeeNo, existing.Email, oldUser.Id, oldUser.EmployeeNo, oldUser.Email));
                        continue;
                    }
                    //Existing company, need to check id and update if needed
                    if (existing.Id != employee.Id)
                    {
                        int newId = existing.Id;

                        UpdateGenericColumn(employees, "ManagerID", employee.Id, newId);
                        UpdateGenericColumn(employees, "LastModifiedBy", employee.Id, newId);
                    }
                    toRemove.Add(employee.Id);
                    employeesToAddToProject.Add(existing.Id);
                }
            }
            foreach (int i in toRemove)
            {
                employees.RemoveAll(x => x.Id == i);
            }

            //Cross check
            foreach (Employee employee in employees)
            {
                if (dal.Users.Any(x => x.EmployeeNo.Equals(employee.EmployeeNo, StringComparison.InvariantCultureIgnoreCase)))
                    errors.Add("Employee # slipped through: " + employee.EmployeeNo);

                if (!string.IsNullOrWhiteSpace(employee.Email) && dal.Users.Any(x => x.Email.Equals(employee.Email, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Employee existing = dal.Users.First(x => x.Email.Equals(employee.Email));
                    errors.Add(string.Format("Import employee {0}:{1}:{2} doesn't match existing employee {3}:{4}:{5}", employee.Id, employee.EmployeeNo, employee.Email, existing.Id, existing.EmployeeNo, existing.Email));
                }

                if (!string.IsNullOrWhiteSpace(employee.UserName) && dal.Users.Any(x => x.UserName.Equals(employee.UserName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Employee existing = dal.Users.First(x => x.UserName.Equals(employee.UserName, StringComparison.InvariantCultureIgnoreCase));
                    errors.Add(string.Format("Import employee {0}:{1}:{2}:{6} doesn't match existing employee {3}:{4}:{5}:{7}", employee.Id, employee.EmployeeNo, employee.Email, existing.Id, existing.EmployeeNo, existing.Email, employee.UserName, existing.UserName));
                }
            }
        }

        public static void CheckAndUpdateCompanies(ApplicationDbContext dal, List<Company> companies, List<Employee> employees, List<Office> offices, List<Project> projects)
        {
            List<int> toRemove = new List<int>();
            int nextIdInsert = Math.Max(dal.Companies.Max(x => x.Company_Id), companies.Max(x => x.Company_Id)) + 1;
            foreach (Company company in companies.OrderBy(x => x.Company_Id))
            {
                Company existing = dal.Companies.SingleOrDefault(x => x.Company_Name.Equals(company.Company_Name, StringComparison.InvariantCultureIgnoreCase));
                if (existing == null)
                {
                    //No existing company, need to check id, and update if needed
                    //See if the id exists
                    if (dal.Companies.Any(x => x.Company_Id == company.Company_Id))
                    {
                        UpdateGenericColumn(employees, "CompanyID", company.Company_Id, nextIdInsert);
                        UpdateGenericColumn(offices, "Company_Id", company.Company_Id, nextIdInsert);
                        UpdateGenericColumn(projects, "ClientCompanyID", company.Company_Id, nextIdInsert);

                        company.Company_Id = nextIdInsert;

                        nextIdInsert++;
                    }
                }
                else
                {
                    //Existing company, need to check id and update if needed
                    if (existing.Company_Id != company.Company_Id)
                    {
                        UpdateGenericColumn(employees, "CompanyID", company.Company_Id, existing.Company_Id);
                        UpdateGenericColumn(offices, "Company_Id", company.Company_Id, existing.Company_Id);
                        UpdateGenericColumn(projects, "ClientCompanyID", company.Company_Id, existing.Company_Id);
                    }
                    toRemove.Add(company.Company_Id);
                }
            }
            foreach (int i in toRemove)
            {
                companies.RemoveAll(x => x.Company_Id == i);
            }
        }

        public static void UpdateGenericColumn<T>(List<T> completeList, string updateColumn, object oldValue, object newValue) where T : IMergeable
        {
            Type type = typeof(T);
            List<PropertyInfo> properties = type.GetProperties().Where(x => !x.GetMethod.IsVirtual).ToList();

            foreach (T instance in completeList)
            {
                if (instance.MergedFields.ContainsKey(updateColumn) && instance.MergedFields[updateColumn])
                    continue;

                foreach (PropertyInfo property in properties)
                {
                    if (property.Name == updateColumn)
                    {
                        object value = property.GetValue(instance);
                        if ((value == null && oldValue == null) || (value != null && value.Equals(oldValue)))
                        {
                            property.SetValue(instance, newValue);
                            instance.MergedFields[updateColumn] = true;
                        }

                        break;
                    }
                }
            }
        }

        public static void CheckTasks(List<ProjectTask> tasks, List<string> errors)
        {
            foreach (ProjectTask task in tasks)
            {
                if (string.IsNullOrWhiteSpace(task.TaskNo) || string.IsNullOrWhiteSpace(task.Name) || string.IsNullOrWhiteSpace(task.AliasCode))
                    errors.Add(string.Format("Task with id {0} does not have a valid TaskNo, Name or AliasCode", task.TaskID));
            }
        }

        public static void CheckTimesheetsAndItems(List<EmployeeTimesheet> timesheets, List<EmployeeTimesheetItem> timesheetItems, List<string> errors, List<TimesheetPeriod> timesheetPeriods)
        {
            Console.WriteLine("Checking timesheet items...");
            foreach (EmployeeTimesheetItem employeeTimesheetItem in timesheetItems)
            {
                if (!timesheets.Any(x => x.TimesheetID == employeeTimesheetItem.TimesheetID))
                    errors.Add("Timesheet item with id: " + employeeTimesheetItem.TimesheetItemID + " doesn't have respective timesheet: " + employeeTimesheetItem.TimesheetID);
            }

            foreach (EmployeeTimesheet timesheet in timesheets)
            {
                if (!timesheetPeriods.Any(x => x.TimesheetPeriodID == timesheet.TimesheetPeriodID))
                    errors.Add(string.Format("Timesheet with id: {0} is referencing a non-existent timesheet period: {1}", timesheet.TimesheetID, timesheet.TimesheetPeriodID));
            }
        }

        public static decimal GetDecimal(object value)
        {
            if (value == null)
                throw new Exception("Cannot determine decimal...");

            decimal d;
            if (decimal.TryParse(value.ToString(), out d))
                return d;

            throw new Exception("Cannot determine decimal...");
        }

        public static DateTime GetDate(object value)
        {
            if (value == null)
                throw new Exception("Cannot determine datetime...");

            DateTime dt;
            if (DateTime.TryParse(value.ToString(), out dt))
                return dt;

            throw new Exception("Cannot determine datetime...");
        }

        public static DateTime? GetNullableDate(object value)
        {
            if (value == null)
                return null;

            DateTime dt;
            if (DateTime.TryParse(value.ToString(), out dt))
                return dt;

            if (value.Equals(0) || value.ToString() == "0")
                return null;

            throw new Exception("Cannot parse datetime: " + value);
        }

        public static bool GetBool(object value)
        {
            return string.Equals(GetString(value), "true", StringComparison.InvariantCultureIgnoreCase);
        }

        public static List<T> ImportSpreadsheet<T>(string filename, Func<ExcelWorksheet, int, T> createFunc)
        {
            List<T> rets = new List<T>();

            using (ExcelPackage package = new ExcelPackage(new FileInfo(filename)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.First();

                int row = 2;
                int col = 1;
                while (worksheet.Cells[row, 1].Value != null)
                {
                    rets.Add(createFunc(worksheet, row));
                    col = 1;
                    row++;
                }

                if (!IsRowEmpty(worksheet, row) || !IsRowEmpty(worksheet, row + 1))
                    throw new Exception(string.Format("Import file {0} has a breaking line at row {1}", filename, row));
            }
            return rets;
        }

        public static bool IsRowEmpty(ExcelWorksheet worksheet, int row)
        {
            for (int i = 1; i < 20; i++)
            {
                if (worksheet.Cells[row, i].Value != null)
                    return false;
            }
            return true;
        }

        public static int? GetNullableInt(object value, bool zeroIsNull)
        {
            if (value == null)
                return null;

            int ret;
            if (int.TryParse(value.ToString(), out ret))
                return zeroIsNull && ret == 0 ? (int?)null : ret;

            return null;
        }

        public static string GetString(object value)
        {
            if (value == null)
                return null;

            return value.ToString();
        }

        public static int GetInt(object value)
        {
            if (value == null)
                throw new Exception("Cannot convert into int...");

            int ret;
            if (int.TryParse(value.ToString(), out ret))
                return ret;

            throw new Exception("Cannot convert into int...");
        }
    }
}
