select EmployeeID, EmployeeNo, email, username, ProjectNo, ProjectName, max(EndDate) as LastTimesheet from

	(SELECT employees.EmployeeID as EmployeeID, EmployeeNo, email, username, ProjectNo, Projects.Name as ProjectName, EndDate FROM 
	Employees 
	left JOIN EmployeeTimesheets on Employees.EmployeeID = EmployeeTimesheets.EmployeeID
	left join TimesheetPeriods on EmployeeTimesheets.TimesheetPeriodID = TimesheetPeriods.TimesheetPeriodID
	left join EmployeeTimesheetItems on EmployeeTimesheetItems.TimesheetID = EmployeeTimesheets.TimesheetID
	left join ProjectTasks on EmployeeTimesheetItems.TaskID = ProjectTasks.TaskID
	left join projects on projects.ProjectID = ProjectTasks.ProjectID
	group by employees.EmployeeID, EmployeeNo, email, username, projects.ProjectNo, Projects.Name, EndDate) x

group by EmployeeID, EmployeeNo, email, username, ProjectNo, ProjectName