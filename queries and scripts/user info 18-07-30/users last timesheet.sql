SELECT *
FROM            Employees left JOIN EmployeeProjects ON EmployeeProjects.EmployeeId = Employees.EmployeeID
left join Companies on Employees.CompanyID = Companies.Company_Id left join Projects on EmployeeProjects.ProjectId = Projects.ProjectID
left join EmployeeTimesheets on Employees.EmployeeID = EmployeeTimesheets.EmployeeID left join TimesheetPeriods on EmployeeTimesheets.TimesheetPeriodID = TimesheetPeriods.TimesheetPeriodID