SELECT        EmployeeId, employeeno, company_name, email, names, Role = 
CASE 
     WHEN RoleId = 2 THEN 'Admin'
     WHEN RoleId = 3 THEN 'Superuser'
     WHEN RoleId = 4 THEN 'UserPlus'
     WHEN RoleId = 5 THEN 'Editor'
	 WHEN RoleId = 5 THEN 'UserAdmin'
  END
FROM            AspNetUserRoles INNER JOIN
                         Employees ON AspNetUserRoles.UserId = Employees.EmployeeID
						 left JOIN
                         companies ON Employees.companyid = companies.Company_Id
						 where Employees.LockoutEndDateUtc is null