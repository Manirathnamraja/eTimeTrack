delete from EmployeeTimesheetItems where TimesheetItemID in
(
select EmployeeTimesheetItems.TimesheetItemID
from EmployeeTimesheetItems
left join EmployeeTimesheets on EmployeeTimesheetItems.TimesheetID = EmployeeTimesheets.TimesheetID
left join TimesheetPeriods on EmployeeTimesheets.TimesheetPeriodID = TimesheetPeriods.TimesheetPeriodID
where ([Day1Hrs] = 0 AND [Day2Hrs] = 0 AND [Day3Hrs] = 0 AND [Day4Hrs] = 0 AND [Day5Hrs] = 0 AND [Day6Hrs] = 0 AND [Day7Hrs] = 0)
and TimesheetPeriods.EndDate < '2021-06-30'
)





This script works fine
Retested in August 2021 and works fine