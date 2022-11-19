select distinct EmployeeID from EmployeeTimesheets where TimesheetPeriodID = 408

select * from EmployeeTimesheets join EmployeeTimesheetItems on EmployeeTimesheets.TimesheetID = EmployeeTimesheetItems.TimesheetID where TimesheetPeriodID = 408