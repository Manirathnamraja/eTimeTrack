SELECT  *      
FROM            Employees left JOIN EmployeeProjects ON EmployeeProjects.EmployeeId = Employees.EmployeeID
where EmployeeProjectId is null