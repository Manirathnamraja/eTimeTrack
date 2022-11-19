using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using eTimeTrack.Models;
using eTimeTrack.ProjectImportPreparation.Cli.Extensions;
using static eTimeTrack.ProjectImportPreparation.Cli.ChecksAndHelpers;

namespace eTimeTrack.ProjectImportPreparation.Cli
{
    class ProcessProject
    {
        private readonly string _importDirectory;
        private readonly string _exportDirectory;

        public ProcessProject(string importDirectory, string exportDirectory)
        {
            _importDirectory = importDirectory;
            _exportDirectory = exportDirectory;
        }
        public void Process()
        {
            //Check files all exist
            string companyFilename = Path.Combine(_importDirectory, "Companies.xlsx");
            string employeeFilename = Path.Combine(_importDirectory, "Employees.xlsx");
            string employeeTimesheetItemsFilename = Path.Combine(_importDirectory, "EmployeeTimesheetItems.xlsx");
            string employeeTimesheetsFilename = Path.Combine(_importDirectory, "EmployeeTimesheets.xlsx");
            string officesFilename = Path.Combine(_importDirectory, "Offices.xlsx");
            string groupsFilename = Path.Combine(_importDirectory, "ProjectGroups.xlsx");
            string partsFilename = Path.Combine(_importDirectory, "ProjectParts.xlsx");
            string projectsFilename = Path.Combine(_importDirectory, "Projects.xlsx");
            string tasksFilename = Path.Combine(_importDirectory, "ProjectTasks.xlsx");
            string variationItemsFilename = Path.Combine(_importDirectory, "ProjectVariationItems.xlsx");
            string variationsFilename = Path.Combine(_importDirectory, "ProjectVariations.xlsx");
            string timesheetPeriodsFilename = Path.Combine(_importDirectory, "TimesheetPeriods.xlsx");

            if (!File.Exists(companyFilename)) { Console.WriteLine("Import file doesn't exist: " + companyFilename); return; }
            if (!File.Exists(employeeFilename)) { Console.WriteLine("Import file doesn't exist: " + employeeFilename); return; }
            if (!File.Exists(employeeTimesheetItemsFilename)) { Console.WriteLine("Import file doesn't exist: " + employeeTimesheetItemsFilename); return; }
            if (!File.Exists(employeeTimesheetsFilename)) { Console.WriteLine("Import file doesn't exist: " + employeeTimesheetsFilename); return; }
            if (!File.Exists(officesFilename)) { Console.WriteLine("Import file doesn't exist: " + officesFilename); return; }
            if (!File.Exists(groupsFilename)) { Console.WriteLine("Import file doesn't exist: " + groupsFilename); return; }
            if (!File.Exists(partsFilename)) { Console.WriteLine("Import file doesn't exist: " + partsFilename); return; }
            if (!File.Exists(projectsFilename)) { Console.WriteLine("Import file doesn't exist: " + projectsFilename); return; }
            if (!File.Exists(tasksFilename)) { Console.WriteLine("Import file doesn't exist: " + tasksFilename); return; }
            if (!File.Exists(variationItemsFilename)) { Console.WriteLine("Import file doesn't exist: " + variationItemsFilename); return; }
            if (!File.Exists(variationsFilename)) { Console.WriteLine("Import file doesn't exist: " + variationsFilename); return; }
            if (!File.Exists(timesheetPeriodsFilename)) { Console.WriteLine("Import file doesn't exist: " + timesheetPeriodsFilename); return; }

            //Import each file into models
            Console.WriteLine("Importing companies...");
            List<Company> companies = ImportSpreadsheet(companyFilename, (worksheet, row) =>
            {
                int col = 1;
                return new Company
                {
                    Company_Id = GetInt(worksheet.Cells[row, col++].Value),
                    Company_Code = GetString(worksheet.Cells[row, col++].Value),
                    Company_Name = GetString(worksheet.Cells[row, col++].Value),
                    Address = GetString(worksheet.Cells[row, col++].Value),
                    E_Org = GetNullableInt(worksheet.Cells[row, col++].Value, true)
                };
            });
            Console.WriteLine("Importing employees...");
            List<Employee> employees = ImportSpreadsheet(employeeFilename, (worksheet, row) =>
            {
                int col = 1;
                Employee emp = new Employee
                {
                    Id = GetInt(worksheet.Cells[row, col++].Value),
                    EmployeeNo = GetString(worksheet.Cells[row, col++].Value),
                    LockoutEnabled = true
                };

                //Use this if they haven't added the email column
                //col += 6; //skip names, username, password, misrate

                //Use this if they have added the email column
                col += 4;
                emp.Email = GetString(worksheet.Cells[row, col++].Value); //BEFORE password
                col += 2;

                emp.IsActive = GetBool(worksheet.Cells[row, col++].Value);
                col++; //skip default...
                emp.ManagerID = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                emp.CompanyID = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                emp.OfficeID = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                emp.AllowOT = GetBool(worksheet.Cells[row, col++].Value);
                col += 9; //skip access infomation
                emp.LastModifiedBy = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                emp.LastModifiedDate = GetNullableDate(worksheet.Cells[row, col++].Value);
                return emp;
            });
            Console.WriteLine("Importing timesheet items...");
            List<EmployeeTimesheetItem> timesheetItems = ImportSpreadsheet(employeeTimesheetItemsFilename, (worksheet, row) =>
            {
                int col = 1;
                EmployeeTimesheetItem emp = new EmployeeTimesheetItem
                {
                    TimesheetID = GetInt(worksheet.Cells[row, col++].Value),
                    VariationID = GetInt(worksheet.Cells[row, col++].Value),
                    TaskID = GetInt(worksheet.Cells[row, col++].Value),
                    OTCode = GetInt(worksheet.Cells[row, col++].Value),
                };
                col++; //skip activity id

                emp.PayTypeID = 11;
                col++;
                //emp.PayTypeID = GetInt(worksheet.Cells[row, col++].Value);

                emp.ItemNo = GetInt(worksheet.Cells[row, col++].Value);
                emp.Day1Hrs = GetDecimal(worksheet.Cells[row, col++].Value);
                emp.Day2Hrs = GetDecimal(worksheet.Cells[row, col++].Value);
                emp.Day3Hrs = GetDecimal(worksheet.Cells[row, col++].Value);
                emp.Day4Hrs = GetDecimal(worksheet.Cells[row, col++].Value);
                emp.Day5Hrs = GetDecimal(worksheet.Cells[row, col++].Value);
                emp.Day6Hrs = GetDecimal(worksheet.Cells[row, col++].Value);
                emp.Day7Hrs = GetDecimal(worksheet.Cells[row, col++].Value);
                col += 2; //skip discipline, chargeout
                emp.InvoiceID = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                emp.Comments = GetString(worksheet.Cells[row, col++].Value);
                emp.LastModifiedBy = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                emp.LastModifiedDate = GetNullableDate(worksheet.Cells[row, col++].Value);
                return emp;
            });
            Console.WriteLine("Importing timesheets...");
            List<EmployeeTimesheet> timesheets = ImportSpreadsheet(employeeTimesheetsFilename, (worksheet, row) =>
            {
                int col = 1;
                EmployeeTimesheet emp = new EmployeeTimesheet
                {
                    TimesheetID = GetInt(worksheet.Cells[row, col++].Value),
                    EmployeeID = GetInt(worksheet.Cells[row, col++].Value),
                    TimesheetPeriodID = GetInt(worksheet.Cells[row, col++].Value),
                    ApprovedByID = GetNullableInt(worksheet.Cells[row, col++].Value, true),
                    DateApproved = GetNullableDate(worksheet.Cells[row, col++].Value),
                };
                col++; //skip isCorrection
                emp.UseDayTimeEntry = GetBool(worksheet.Cells[row, col++].Value);
                emp.Day1StartTime = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.Day1EndTime = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.Day2StartTime = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.Day2EndTime = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.Day3StartTime = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.Day3EndTime = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.Day4StartTime = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.Day4EndTime = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.Day5StartTime = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.Day5EndTime = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.Day6StartTime = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.Day6EndTime = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.Day7StartTime = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.Day7EndTime = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.LastModifiedBy = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                emp.LastModifiedDate = GetNullableDate(worksheet.Cells[row, col++].Value);
                return emp;
            });
            Console.WriteLine("Importing offices...");
            List<Office> offices = ImportSpreadsheet(officesFilename, (worksheet, row) =>
            {
                int col = 1;
                Office emp = new Office
                {
                    Office_Id = GetInt(worksheet.Cells[row, col++].Value),
                    Office_Code = GetString(worksheet.Cells[row, col++].Value),
                    Office_Name = GetString(worksheet.Cells[row, col++].Value),
                    Company_Id = GetInt(worksheet.Cells[row, col++].Value),
                };
                return emp;
            });
            Console.WriteLine("Importing groups...");
            List<ProjectGroup> groups = ImportSpreadsheet(groupsFilename, (worksheet, row) =>
            {
                int col = 1;
                ProjectGroup emp = new ProjectGroup
                {
                    GroupID = GetInt(worksheet.Cells[row, col++].Value),
                    ProjectID = GetInt(worksheet.Cells[row, col++].Value),
                    PartID = GetInt(worksheet.Cells[row, col++].Value),
                };
                col++; //skip parentid
                emp.GroupNo = GetString(worksheet.Cells[row, col++].Value);
                emp.Name = GetString(worksheet.Cells[row, col++].Value);
                emp.Notes = GetString(worksheet.Cells[row, col++].Value);
                emp.GroupTypeID = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                emp.IsClosed = GetBool(worksheet.Cells[row, col++].Value);
                emp.AliasCode = GetString(worksheet.Cells[row, col++].Value);
                emp.LastModifiedBy = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                emp.LastModifiedDate = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.PM = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                return emp;
            });
            Console.WriteLine("Importing parts...");
            List<ProjectPart> parts = ImportSpreadsheet(partsFilename, (worksheet, row) =>
            {
                int col = 1;
                ProjectPart emp = new ProjectPart
                {
                    PartID = GetInt(worksheet.Cells[row, col++].Value),
                    ProjectID = GetInt(worksheet.Cells[row, col++].Value),
                    PartNo = GetString(worksheet.Cells[row, col++].Value),
                };
                col++; //skip invisicpart....
                emp.Name = GetString(worksheet.Cells[row, col++].Value);
                emp.Notes = GetString(worksheet.Cells[row, col++].Value);
                emp.IsClosed = GetBool(worksheet.Cells[row, col++].Value);
                col++; //skip aliascode
                emp.LastModifiedBy = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                emp.LastModifiedDate = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.PM = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                return emp;
            });
            Console.WriteLine("Importing projects...");
            List<Project> projects = ImportSpreadsheet(projectsFilename, (worksheet, row) =>
            {
                int col = 1;
                Project emp = new Project
                {
                    ProjectID = GetInt(worksheet.Cells[row, col++].Value),
                    ProjectNo = GetString(worksheet.Cells[row, col++].Value),
                    OfficeID = GetNullableInt(worksheet.Cells[row, col++].Value, true),
                    SeqNo = GetString(worksheet.Cells[row, col++].Value),
                    YearNo = GetString(worksheet.Cells[row, col++].Value),
                    RegistrationNo = GetString(worksheet.Cells[row, col++].Value),
                    Name = GetString(worksheet.Cells[row, col++].Value),
                    DirectorID = GetNullableInt(worksheet.Cells[row, col++].Value, true),
                    ManagerID = GetNullableInt(worksheet.Cells[row, col++].Value, true),
                    IsClosed = GetBool(worksheet.Cells[row, col++].Value),
                    DateClosed = GetNullableDate(worksheet.Cells[row, col++].Value),
                    DateOpened = GetNullableDate(worksheet.Cells[row, col++].Value),
                    ClientCompanyID = GetNullableInt(worksheet.Cells[row, col++].Value, true),
                    ClientContactID = GetNullableInt(worksheet.Cells[row, col++].Value, true),
                    Notes = GetString(worksheet.Cells[row, col++].Value),
                    LastModifiedBy = GetNullableInt(worksheet.Cells[row, col++].Value, true),
                    LastModifiedDate = GetNullableDate(worksheet.Cells[row, col++].Value)
                };
                return emp;
            });
            Console.WriteLine("Importing tasks...");
            List<ProjectTask> tasks = ImportSpreadsheet(tasksFilename, (worksheet, row) =>
            {
                int col = 1;
                ProjectTask emp = new ProjectTask
                {
                    TaskID = GetInt(worksheet.Cells[row, col++].Value),
                    ProjectID = GetInt(worksheet.Cells[row, col++].Value),
                    GroupID = GetInt(worksheet.Cells[row, col++].Value),
                    TaskNo = GetString(worksheet.Cells[row, col++].Value),
                };
                col++; //skip tasktype
                emp.Name = GetString(worksheet.Cells[row, col++].Value);
                emp.AliasCode = GetString(worksheet.Cells[row, col++].Value);
                col += 2; //skip aecom/aurecon code
                emp.IsClosed = GetBool(worksheet.Cells[row, col++].Value);
                emp.Notes = GetString(worksheet.Cells[row, col++].Value);
                col++; //skip percent complete
                emp.LastModifiedBy = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                emp.LastModifiedDate = GetNullableDate(worksheet.Cells[row, col++].Value);
                emp.PM = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                return emp;
            });
            Console.WriteLine("Importing variation items...");
            List<ProjectVariationItem> variationItems = ImportSpreadsheet(variationItemsFilename, (worksheet, row) =>
            {
                int col = 1;
                ProjectVariationItem emp = new ProjectVariationItem
                {
                    VariationID = GetInt(worksheet.Cells[row, col++].Value),
                    TaskID = GetInt(worksheet.Cells[row, col++].Value),
                    IsApproved = GetBool(worksheet.Cells[row, col++].Value),
                    IsClosed = GetBool(worksheet.Cells[row, col++].Value),
                    LastModifiedBy = GetNullableInt(worksheet.Cells[row, col++].Value, true),
                    LastModifiedDate = GetNullableDate(worksheet.Cells[row, col++].Value)
                };
                return emp;
            });
            Console.WriteLine("Importing variations...");
            List<ProjectVariation> variations = ImportSpreadsheet(variationsFilename, (worksheet, row) =>
            {
                int col = 1;
                ProjectVariation emp = new ProjectVariation
                {
                    VariationID = GetInt(worksheet.Cells[row, col++].Value),
                    ProjectID = GetInt(worksheet.Cells[row, col++].Value),
                    VariationNo = GetString(worksheet.Cells[row, col++].Value),
                    RevNo = GetString(worksheet.Cells[row, col++].Value),
                    Description = GetString(worksheet.Cells[row, col++].Value),
                    IsClosed = GetBool(worksheet.Cells[row, col++].Value),
                    DateSubmitted = GetNullableDate(worksheet.Cells[row, col++].Value),
                    IsApproved = GetBool(worksheet.Cells[row, col++].Value),
                    DateApproved = GetNullableDate(worksheet.Cells[row, col++].Value),
                    Reference = GetString(worksheet.Cells[row, col++].Value),
                    Notes = GetString(worksheet.Cells[row, col++].Value),
                    IsOriginalScope = GetBool(worksheet.Cells[row, col++].Value),
                    LastModifiedBy = GetNullableInt(worksheet.Cells[row, col++].Value, true),
                    LastModifiedDate = GetNullableDate(worksheet.Cells[row, col++].Value)
                };
                return emp;
            });
            Console.WriteLine("Importing timesheet periods...");
            List<TimesheetPeriod> timesheetPeriods = ImportSpreadsheet(timesheetPeriodsFilename, (worksheet, row) =>
            {
                int col = 1;
                TimesheetPeriod emp = new TimesheetPeriod
                {
                    TimesheetPeriodID = GetInt(worksheet.Cells[row, col++].Value)
                };
                col++; //FinancialPeriodId
                emp.WeekNo = GetInt(worksheet.Cells[row, col++].Value);
                emp.StartDate = GetDate(worksheet.Cells[row, col++].Value);
                emp.EndDate = GetDate(worksheet.Cells[row, col++].Value);
                emp.IsClosed = GetBool(worksheet.Cells[row, col++].Value);
                emp.StandardDays = GetInt(worksheet.Cells[row, col++].Value);
                emp.LastModifiedBy = GetNullableInt(worksheet.Cells[row, col++].Value, true);
                emp.LastModifiedDate = GetNullableDate(worksheet.Cells[row, col++].Value);
                return emp;
            });

            Console.WriteLine("Finished importing!");

            List<string> errors = new List<string>();
            List<string> warnings = new List<string>
            {
                string.Format("There were only {0} out of {1} employees with emails.",
                    employees.Count(x => !string.IsNullOrWhiteSpace(x.Email)), employees.Count)
            };
            
            //Integrity check models to make sure all link correctly
            CheckDuplicateTimesheetIds(timesheets, errors);
            CheckTimesheetsAndItems(timesheets, timesheetItems, errors, timesheetPeriods);
            CheckEmployees(employees, errors);

            CheckOfficeCompanies(offices, companies, errors);

            CheckTimesheetPeriods(timesheetPeriods, errors);

            CheckTasks(tasks, errors);

            CheckLinkedEmployees(employees, timesheetItems, timesheets, groups, parts, projects, tasks, variationItems, variations, timesheetPeriods, errors);

            if (projects.Count != 1)
                warnings.Add(string.Format("Trying to import {0} projects.", projects.Count));

            ResetMerges(companies, employees, timesheets, timesheetItems, offices, projects, groups,
                parts, tasks, variations, variationItems, timesheetPeriods);

            List<int> employeesToAddToProject = new List<int>();

            //Open entity framework models and compare one by one
            using (ApplicationDbContext dal = new ApplicationDbContext())
            {
                Console.WriteLine("Merging Companies...");
                CheckAndUpdateCompanies(dal, companies, employees, offices, projects);
                ResetMerges(companies, employees, offices, projects);

                Console.WriteLine("Merging Employees...");
                CheckAndUpdateEmployees(dal, employees, timesheetItems, timesheets, groups,
                    parts, projects, tasks, variationItems, variations, timesheetPeriods, employeesToAddToProject, errors, warnings);
                ResetMerges(employees, timesheetItems, timesheets, groups,
                    parts, projects, tasks, variationItems, variations, timesheetPeriods);

                Console.WriteLine("Merging TimesheetPeriods...");
                CheckAndUpdateTimesheetPeriods(dal, timesheetPeriods, timesheets, errors);
                ResetMerges(timesheetPeriods, timesheets);

                Console.WriteLine("Merging Timesheet Items...");
                //Don't need to do anything, nothing references the timesheet items id

                Console.WriteLine("Merging Timesheets...");
                CheckAndUpdateTimesheets(dal, timesheets, timesheetItems); //NOTE TODO need to do this after timesheet period merge
                ResetMerges(timesheets, timesheetItems);

                Console.WriteLine("Merging Offices...");
                CheckAndUpdateOffices(dal, offices, employees, projects);
                ResetMerges(offices, employees, projects);

                Console.WriteLine("Merging Projects...");
                CheckAndUpdateProjects(dal, projects, groups, parts, tasks, variations);
                ResetMerges(projects, groups, parts, tasks, variations);

                Console.WriteLine("Merging Variations...");
                CheckAndUpdateVariations(dal, variations, variationItems, timesheetItems);
                ResetMerges(variations, variationItems, timesheetItems);

                Console.WriteLine("Merging Parts...");
                CheckAndUpdateParts(dal, parts, groups);
                ResetMerges(parts, groups);

                Console.WriteLine("Merging Groups...");
                CheckAndUpdateGroups(dal, groups, tasks);
                ResetMerges(groups, tasks);

                Console.WriteLine("Merging Tasks...");
                CheckAndUpdateTasks(dal, tasks, variationItems, timesheetItems);
                ResetMerges(tasks, variationItems, timesheetItems);

                Console.WriteLine("Merging Variation Items...");
                CheckAndUpdateVariationItems(dal, variationItems);
                ResetMerges(variationItems);
            }

            Console.WriteLine("Finished merging with live db, about to update columns...");

            FixEmployees(employees);
            FixProjects(projects);
            FixParts(parts);
            FixGroups(groups);
            FixTasks(tasks);

            if (warnings.Any())
            {
                string warningsFilename = Path.Combine(_exportDirectory, "eTimeTrack-ProjectMergeSQL-WARNINGS-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".txt");
                using (StreamWriter sw = new StreamWriter(warningsFilename))
                {
                    foreach (string warning in warnings)
                    {
                        sw.WriteLine(warning);
                    }
                }
                Console.WriteLine("WARNINGS occured during the import process. Please view in the following file: " + warningsFilename);
            }

            if (errors.Any())
            {
                string errorsFilename = Path.Combine(_exportDirectory, "eTimeTrack-ProjectMergeSQL-ERRORS-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".txt");
                using (StreamWriter sw = new StreamWriter(errorsFilename))
                {
                    foreach (string error in errors)
                    {
                        sw.WriteLine(error);
                    }
                }
                Console.WriteLine("ERRORS occured during the import process. Please view in the following file: " + errorsFilename);
                return;
            }

            Console.WriteLine("Exporting to file...");

            string filename = Path.Combine(_exportDirectory, "eTimeTrack-ProjectMergeSQL-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".sql");
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine("SET IDENTITY_INSERT Companies ON;");
                foreach (Company company in companies)
                {
                    sw.WriteLine(CleanSqlString(string.Format(
                        "INSERT INTO Companies (Company_Id, Company_Code, Company_Name, Address, E_Org) VALUES ({0}, '{1}', '{2}', {3}, {4});",
                        company.Company_Id, company.Company_Code, company.Company_Name, company.Address.ToNullableString(), company.E_Org.ToStringOrNullString())));
                }
                sw.WriteLine("SET IDENTITY_INSERT Companies OFF;");
                sw.WriteLine("GO");
                sw.WriteLine("SET IDENTITY_INSERT Offices ON;");
                foreach (Office office in offices)
                {
                    sw.WriteLine(CleanSqlString(string.Format(
                        "INSERT INTO Offices (Office_Id, Office_Code, Office_Name, Company_Id) VALUES ({0}, '{1}', '{2}', {3});",
                        office.Office_Id, office.Office_Code, office.Office_Name, office.Company_Id)));
                }
                sw.WriteLine("SET IDENTITY_INSERT Offices OFF;");
                sw.WriteLine("GO");
                sw.WriteLine("SET IDENTITY_INSERT Employees ON;");
                foreach (Employee employee in employees)
                {
                    sw.WriteLine(CleanSqlString(string.Format(
                        "INSERT INTO Employees (EmployeeID, EmployeeNo, IsActive, ManagerID, CompanyID, " +
                        "OfficeID, AllowOT, LastModifiedBy, LastModifiedDate, Email, EmailConfirmed, SecurityStamp, PhoneNumberConfirmed, " +
                        "TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName) " +
                        "VALUES ({0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, '{11}', {12}, {13}, {14}, {15}, {16});",
                        employee.Id, employee.EmployeeNo, employee.IsActive, employee.ManagerID, employee.CompanyID,
                        employee.OfficeID, employee.AllowOT, employee.LastModifiedBy, employee.LastModifiedDate.ToNullableString(),
                        employee.Email.ToNullableString(), employee.EmailConfirmed, Guid.NewGuid(), 0, 0, 1, 0, GetEmployeeUsername(employee).ToNullableString())));
                }
                sw.WriteLine("SET IDENTITY_INSERT Employees OFF;");
                sw.WriteLine("GO");
                sw.WriteLine("SET IDENTITY_INSERT Projects ON;");
                foreach (Project project in projects)
                {
                    sw.WriteLine(CleanSqlString(string.Format(
                        "INSERT INTO Projects (ProjectID, ProjectNo, OfficeID, SeqNo, YearNo, RegistrationNo, Name, DirectorID, ManagerID, IsClosed, " +
                        "DateClosed, DateOpened, ClientCompanyID, ClientContactID, Notes, LastModifiedBy, LastModifiedDate) " +
                        "VALUES ({0}, '{1}', {2}, '{3}', '{4}', '{5}', {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16});",
                        project.ProjectID, project.ProjectNo, project.OfficeID, project.SeqNo, project.YearNo,
                        project.RegistrationNo, project.Name.ToNullableString(), project.DirectorID, project.ManagerID, project.IsClosed,
                        project.DateClosed.ToNullableString(), project.DateOpened.ToNullableString(), project.ClientCompanyID, project.ClientContactID,
                        project.Notes.ToNullableString(), project.LastModifiedBy, project.LastModifiedDate.ToNullableString())));
                }
                sw.WriteLine("SET IDENTITY_INSERT Projects OFF;");
                sw.WriteLine("GO");
                sw.WriteLine("SET IDENTITY_INSERT TimesheetPeriods ON;");
                foreach (TimesheetPeriod timesheetPeriod in timesheetPeriods)
                {
                    sw.WriteLine(CleanSqlString(string.Format(
                        "INSERT INTO TimesheetPeriods (TimesheetPeriodID, WeekNo, StartDate, EndDate, " +
                        "IsClosed, StandardDays, LastModifiedBy, LastModifiedDate) VALUES ({0}, {1}, '{2}', '{3}', {4}, {5}, {6}, {7});",
                        timesheetPeriod.TimesheetPeriodID, timesheetPeriod.WeekNo,
                        timesheetPeriod.StartDate.ToString("yyyy-MM-dd HH:mm:ss"), timesheetPeriod.EndDate.ToString("yyyy-MM-dd HH:mm:ss"), timesheetPeriod.IsClosed,
                        timesheetPeriod.StandardDays, timesheetPeriod.LastModifiedBy, timesheetPeriod.LastModifiedDate.ToNullableString())));
                }
                sw.WriteLine("SET IDENTITY_INSERT TimesheetPeriods OFF;");
                sw.WriteLine("GO");
                sw.WriteLine("SET IDENTITY_INSERT ProjectVariations ON;");
                foreach (ProjectVariation variation in variations)
                {
                    sw.WriteLine(CleanSqlString(string.Format(
                        "INSERT INTO ProjectVariations (VariationID, ProjectID, VariationNo, RevNo, Description, IsClosed, DateSubmitted, " +
                        "IsApproved, DateApproved, Reference, Notes, IsOriginalScope, LastModifiedBy, LastModifiedDate) " +
                        "VALUES ({0}, {1}, '{2}', {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13});",
                        variation.VariationID, variation.ProjectID, variation.VariationNo, variation.RevNo.ToNullableString(),
                        variation.Description.ToNullableString(), variation.IsClosed, variation.DateSubmitted.ToNullableString(), variation.IsApproved,
                        variation.DateApproved.ToNullableString(), variation.Reference.ToNullableString(), variation.Notes.ToNullableString(), variation.IsOriginalScope,
                        variation.LastModifiedBy, variation.LastModifiedDate.ToNullableString())));
                    //sw.WriteLine("GO");
                }
                sw.WriteLine("SET IDENTITY_INSERT ProjectVariations OFF;");
                sw.WriteLine("GO");
                sw.WriteLine("SET IDENTITY_INSERT ProjectParts ON;");
                foreach (ProjectPart part in parts)
                {
                    sw.WriteLine(CleanSqlString(string.Format(
                        "INSERT INTO ProjectParts (PartID, ProjectID, PartNo, Name, Notes, IsClosed, LastModifiedBy, " +
                        "LastModifiedDate, PM) VALUES ({0}, {1}, '{2}', {3}, {4}, {5}, {6}, {7}, {8});",
                        part.PartID, part.ProjectID, part.PartNo, part.Name.ToNullableString(), part.Notes.ToNullableString(), part.IsClosed,
                        part.LastModifiedBy, part.LastModifiedDate.ToNullableString(), part.PM.ToStringOrNullString())));
                    //sw.WriteLine("GO");
                }
                sw.WriteLine("SET IDENTITY_INSERT ProjectParts OFF;");
                sw.WriteLine("GO");
                sw.WriteLine("SET IDENTITY_INSERT ProjectGroups ON;");
                foreach (ProjectGroup grp in groups)
                {
                    sw.WriteLine(CleanSqlString(string.Format(
                        "INSERT INTO ProjectGroups (GroupID, ProjectID, PartID, GroupNo, Name, Notes, GroupTypeID, " +
                        "IsClosed, AliasCode, LastModifiedBy, LastModifiedDate, PM) " +
                        "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11});",
                        grp.GroupID, grp.ProjectID, grp.PartID, grp.GroupNo.ToNullableString(), grp.Name.ToNullableString(), grp.Notes.ToNullableString(), grp.GroupTypeID,
                        grp.IsClosed, grp.AliasCode.ToNullableString(), grp.LastModifiedBy, grp.LastModifiedDate.ToNullableString(), grp.PM.ToStringOrNullString())));
                    //sw.WriteLine("GO");
                }
                sw.WriteLine("SET IDENTITY_INSERT ProjectGroups OFF;");
                sw.WriteLine("GO");
                sw.WriteLine("SET IDENTITY_INSERT ProjectTasks ON;");
                foreach (ProjectTask task in tasks)
                {
                    sw.WriteLine(CleanSqlString(string.Format(
                        "INSERT INTO ProjectTasks (TaskID, ProjectID, GroupID, TaskNo, Name, AliasCode, OracleCode, IsClosed, " +
                        "Notes, LastModifiedBy, LastModifiedDate, PM) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11});",
                        task.TaskID, task.ProjectID, task.GroupID, task.TaskNo.ToNullableString(), task.Name.ToNullableString(), task.AliasCode.ToNullableString(),
                        task.OracleCode.ToNullableString(), task.IsClosed, task.Notes.ToNullableString(), task.LastModifiedBy, task.LastModifiedDate.ToNullableString(), task.PM.ToStringOrNullString())));
                    //sw.WriteLine("GO");
                }
                sw.WriteLine("SET IDENTITY_INSERT ProjectTasks OFF;");
                sw.WriteLine("GO");
                foreach (ProjectVariationItem item in variationItems)
                {
                    sw.WriteLine(CleanSqlString(string.Format(
                        "INSERT INTO ProjectVariationItems (VariationID, TaskID, IsApproved, IsClosed, LastModifiedBy, " +
                        "LastModifiedDate) VALUES ({0}, {1}, {2}, {3}, {4}, {5});",
                        item.VariationID, item.TaskID, item.IsApproved, item.IsClosed, item.LastModifiedBy,
                        item.LastModifiedDate.ToNullableString())));
                    //sw.WriteLine("GO");
                }
                sw.WriteLine("GO");
                sw.WriteLine("SET IDENTITY_INSERT EmployeeTimesheets ON;");
                foreach (EmployeeTimesheet timesheet in timesheets)
                {
                    sw.WriteLine(CleanSqlString(string.Format(
                        "INSERT INTO EmployeeTimesheets (TimesheetID, EmployeeID, TimesheetPeriodID, ApprovedByID, DateApproved, " +
                        "UseDayTimeEntry, Day1StartTime, Day1EndTime, Day2StartTime, Day2EndTime, Day3StartTime, Day3EndTime, " +
                        "Day4StartTime, Day4EndTime, Day5StartTime, Day5EndTime, Day6StartTime, Day6EndTime, Day7StartTime, Day7EndTime, " +
                        "LastModifiedBy, LastModifiedDate) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, " +
                        "{10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21});",
                        timesheet.TimesheetID, timesheet.EmployeeID, timesheet.TimesheetPeriodID, timesheet.ApprovedByID,
                        timesheet.DateApproved.ToNullableString(), timesheet.UseDayTimeEntry, timesheet.Day1StartTime.ToNullableString(),
                        timesheet.Day1EndTime.ToNullableString(), timesheet.Day2StartTime.ToNullableString(), timesheet.Day2EndTime.ToNullableString(), timesheet.Day3StartTime.ToNullableString(),
                        timesheet.Day3EndTime.ToNullableString(), timesheet.Day4StartTime.ToNullableString(), timesheet.Day4EndTime.ToNullableString(), timesheet.Day5StartTime.ToNullableString(),
                        timesheet.Day5EndTime.ToNullableString(), timesheet.Day6StartTime.ToNullableString(), timesheet.Day6EndTime.ToNullableString(), timesheet.Day7StartTime.ToNullableString(),
                        timesheet.Day7EndTime.ToNullableString(), timesheet.LastModifiedBy, timesheet.LastModifiedDate.ToNullableString())));
                    //sw.WriteLine("GO");
                }
                sw.WriteLine("SET IDENTITY_INSERT EmployeeTimesheets OFF;");
                sw.WriteLine("GO");
                foreach (EmployeeTimesheetItem item in timesheetItems)
                {
                    sw.WriteLine(CleanSqlString(string.Format(
                        "INSERT INTO EmployeeTimesheetItems (TimesheetID, VariationID, TaskID, OTCode, PayTypeID, ItemNo, Day1Hrs, " +
                        "Day2Hrs, Day3Hrs, Day4Hrs, Day5Hrs, Day6Hrs, Day7Hrs, InvoiceID, Comments, LastModifiedBy, LastModifiedDate) " +
                        "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16});",
                        item.TimesheetID, item.VariationID, item.TaskID, item.OTCode, item.PayTypeID, item.ItemNo,
                        item.Day1Hrs, item.Day2Hrs, item.Day3Hrs, item.Day4Hrs, item.Day5Hrs, item.Day6Hrs, item.Day7Hrs,
                        item.InvoiceID, item.Comments.ToNullableString(), item.LastModifiedBy, item.LastModifiedDate.ToNullableString())));
                    //sw.WriteLine("GO");
                }
                sw.WriteLine("GO");
                foreach (Project project in projects)
                {
                    foreach (int employeeId in employeesToAddToProject)
                    {
                        sw.WriteLine(CleanSqlString(string.Format(
                            "INSERT INTO EmployeeProjects (EmployeeId, ProjectId) VALUES ({0}, {1});", employeeId, project.ProjectID)));
                    }
                    sw.WriteLine("GO");
                    sw.WriteLine("INSERT INTO ProjectCompanies (ProjectId, CompanyId) VALUES ({0}, {1})", project.ProjectID, 1);
                    sw.WriteLine("GO");
                }
            }
        }
    }
}
