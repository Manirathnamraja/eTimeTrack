SELECT Employees.EmployeeID, employees.EmployeeNo, username, max(TimesheetPeriods.EndDate) as last_timesheet
FROM            Employees left JOIN EmployeeTimesheets on Employees.EmployeeID = EmployeeTimesheets.EmployeeID left join TimesheetPeriods on EmployeeTimesheets.TimesheetPeriodID = TimesheetPeriods.TimesheetPeriodID
group by Employees.EmployeeID, employees.EmployeeNo, UserName
order by last_timesheet desc