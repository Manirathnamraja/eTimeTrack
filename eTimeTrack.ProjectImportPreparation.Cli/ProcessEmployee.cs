using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using eTimeTrack.Models;
using eTimeTrack.ProjectImportPreparation.Cli.Extensions;
using static eTimeTrack.ProjectImportPreparation.Cli.ChecksAndHelpers;

namespace eTimeTrack.ProjectImportPreparation.Cli
{
    class ProcessEmployee
    {
        private readonly string _importDirectory;
        private readonly string _exportDirectory;
        private readonly int _projectId;

        public ProcessEmployee(string importDirectory, string exportDirectory, int projectId)
        {
            _importDirectory = importDirectory;
            _exportDirectory = exportDirectory;
            _projectId = projectId;
        }
        public void Process()
        {
            //Check files all exist
            string employeeFilename = Path.Combine(_importDirectory, "Employees.xlsx");

            if (!File.Exists(employeeFilename)) { Console.WriteLine("Import file doesn't exist: " + employeeFilename); return; }

            Console.WriteLine("Importing employees...");
            List<Employee> employees = ImportSpreadsheet(employeeFilename, (worksheet, row) =>
            {
                int col = 1;
                Employee emp = new Employee
                {
                    Id = GetInt(worksheet.Cells[row, col++].Value),
                    LockoutEnabled = true
                    
                };
                emp.EmployeeNo = GetString(worksheet.Cells[row, col++].Value);
                emp.Names = GetString(worksheet.Cells[row, col++].Value);
                emp.IsActive = true;
                emp.AllowOT = false;
                col++;
                col++;
                col++;
                col++;
                emp.Email = GetString(worksheet.Cells[row, col++].Value);
                emp.CompanyID = GetInt(worksheet.Cells[row, col++].Value);

                emp.UserName = string.IsNullOrWhiteSpace(emp.Email) ? emp.EmployeeNo : emp.Email;
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
            CheckEmployees(employees, errors);

            List<int> employeesToAddToProject = new List<int>();

            ResetMerges(employees);

            //Open entity framework models and compare one by one
            using (ApplicationDbContext dal = new ApplicationDbContext())
            {
                Console.WriteLine("Merging Employees...");
                CheckAndUpdateEmployees(dal, employees, employeesToAddToProject, errors, warnings);
            }

            Console.WriteLine("Finished merging with live db, about to update columns...");

            FixEmployees(employees);

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
                sw.WriteLine("SET IDENTITY_INSERT Employees ON;");
                foreach (Employee employee in employees)
                {
                    sw.WriteLine(CleanSqlString(string.Format(
                        "INSERT INTO Employees (EmployeeID, EmployeeNo, IsActive, ManagerID, CompanyID, " +
                        "OfficeID, AllowOT, LastModifiedBy, LastModifiedDate, Email, EmailConfirmed, SecurityStamp, PhoneNumberConfirmed, " +
                        "TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName, Names) " +
                        "VALUES ({0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, '{11}', {12}, {13}, {14}, {15}, '{16}', '{17}');",
                        employee.Id, employee.EmployeeNo, employee.IsActive, employee.ManagerID, employee.CompanyID,
                        employee.OfficeID, employee.AllowOT, employee.LastModifiedBy, employee.LastModifiedDate.ToNullableString(),
                        employee.Email.ToNullableString(), employee.EmailConfirmed, Guid.NewGuid(), 0, 0, 0, 0, GetEmployeeUsername(employee), employee.Names)));
                }
                sw.WriteLine("SET IDENTITY_INSERT Employees OFF;");
                sw.WriteLine("GO");
                foreach (int employeeId in employeesToAddToProject)
                {
                    sw.WriteLine(CleanSqlString(string.Format(
                        "INSERT INTO EmployeeProjects (EmployeeId, ProjectId) VALUES ({0}, {1});", employeeId, _projectId)));
                }
                sw.WriteLine("GO");
            }
        }
    }
}
