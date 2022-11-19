select EmployeeID, EmployeeNo, ProjectNo, max(EndDate) as LastTimesheet from

	(SELECT employees.EmployeeID as EmployeeID, EmployeeNo, ProjectNo, EndDate FROM 
	Employees 
	left JOIN EmployeeTimesheets on Employees.EmployeeID = EmployeeTimesheets.EmployeeID
	left join TimesheetPeriods on EmployeeTimesheets.TimesheetPeriodID = TimesheetPeriods.TimesheetPeriodID
	left join EmployeeTimesheetItems on EmployeeTimesheetItems.TimesheetID = EmployeeTimesheets.TimesheetID
	left join ProjectTasks on EmployeeTimesheetItems.TaskID = ProjectTasks.TaskID
	left join projects on projects.ProjectID = ProjectTasks.ProjectID
	group by employees.EmployeeID, EmployeeNo, projects.ProjectNo, EndDate) x

group by EmployeeID, EmployeeNo, ProjectNo