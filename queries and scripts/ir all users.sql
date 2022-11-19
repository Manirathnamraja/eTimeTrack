  select 
  Email,
  employees.EmployeeId,
  EmployeeNo,
  Companies.Company_Name,
  Company_Code,
  employees.IsActive
  
  from
  EmployeeProjects 
  join Employees on employees.EmployeeID = employeeprojects.EmployeeId
  join companies on Companies.Company_Id = employees.CompanyID
  WHERE        (EmployeeProjects.ProjectId = 1316)