SELECT        distinct employees.Email, Employees.Employeeno
FROM            Employees JOIN
                EmployeeTimesheets ON  Employees.EmployeeID = EmployeeTimesheets.EmployeeID AND 
                Employees.EmployeeID = EmployeeTimesheets.LastModifiedBy  JOIN
                EmployeeTimesheetItems ON EmployeeTimesheets.TimesheetID = EmployeeTimesheetItems.TimesheetID  JOIN
                ProjectTasks ON EmployeeTimesheetItems.TaskID = ProjectTasks.TaskID  JOIN
				TimesheetPeriods ON EmployeeTimesheets.TimesheetPeriodID = TimesheetPeriods.TimesheetPeriodID 

where
ProjectTasks.ProjectID = 1316
and  
EndDate >= '2020-09-01'
and
(Day1Hrs <> 0 OR Day2Hrs <> 0 OR Day3Hrs <> 0 OR Day4Hrs <> 0 OR Day5Hrs <> 0 OR Day6Hrs <> 0 OR Day7Hrs <> 0)