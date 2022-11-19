SELECT employees.EmployeeID, EmployeeNo, IsActive, Email, UserName, names, companies.Company_Name, ProjectNo, Projects.Name 
FROM            Employees left JOIN EmployeeProjects ON EmployeeProjects.EmployeeId = Employees.EmployeeID
left join Companies on Employees.CompanyID = Companies.Company_Id left join Projects on EmployeeProjects.ProjectId = Projects.ProjectID