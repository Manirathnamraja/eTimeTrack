select projectno, name, count(*) as count from
	(select projects.ProjectNo, projects.Name, Employees.UserName, enddate from [dbo].[Projects] join [dbo].[ProjectTasks] on projects.projectid = projecttasks.projectid join [EmployeeTimesheetItems] on [dbo].projecttasks.taskid = [dbo].[EmployeeTimesheetItems].TaskID join [dbo].[EmployeeTimesheets] on EmployeeTimesheetItems.TimesheetID = EmployeeTimesheets.TimesheetID join Employees on EmployeeTimesheets.EmployeeID = Employees.EmployeeID join [dbo].[TimesheetPeriods] on EmployeeTimesheets.TimesheetPeriodID = [TimesheetPeriods].TimesheetPeriodID
	where (Day1Hrs > 0 or Day2Hrs > 0 or Day3Hrs > 0 or Day4Hrs > 0 or Day5Hrs > 0 or Day6Hrs > 0 or Day7Hrs > 0) and EndDate > '2017-07-27'
	group by projects.ProjectNo, projects.Name, username, enddate) innertable
group by projectno, name
order by count desc