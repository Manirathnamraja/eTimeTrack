<Query Kind="Expression">
  <Connection>
    <ID>8f397039-4072-484a-b818-b79b5452217b</ID>
    <Persist>true</Persist>
    <Server>AUBNE1LT4285\SQLEXPRESS</Server>
    <Database>etimetrack_live</Database>
    <ShowServer>true</ShowServer>
  </Connection>
  <Output>DataGrids</Output>
</Query>

from e in Employees
join t in EmployeeTimesheets
	on e.EmployeeID equals t.EmployeeID

join p in TimesheetPeriods
	on t.TimesheetPeriodID equals p.TimesheetPeriodID

where p.EndDate >= new DateTime(2017,09,01) && p.EndDate <= new DateTime(2017,09,30)
orderby e.EmployeeID
select new {
	User = e.UserName,
	ID = e.EmployeeNo,
	Email = e.Email,
	Timesheets = p.EndDate,
	Projects = t.EmployeeTimesheetItems.Select(x => x.ProjectTasks).Select(x => x.Project.Name).Distinct().Count()
}


