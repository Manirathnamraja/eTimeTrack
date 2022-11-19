	CREATE VIEW vw_GCLR3_ProjectUserTypes AS 
	SELECT 
		ProjectUserTypeID,
		ProjectID,
		CASE WHEN UserTypes.UserTypeID IS NULL THEN 0 ELSE UserTypes.UserTypeID END AS UserTypeId,
		ProjectUserTypes.IsEnabled,
		MaxNTHours,
		MaxOT1Hours,
		MaxOT2Hours,
		MaxOT3Hours,
		CASE ProjectUserTypes.AliasCode WHEN NULL THEN UserTypes.Code ELSE ProjectUserTypes.AliasCode END AS AliasCode,
		CASE ProjectUserTypes.AliasType WHEN NULL THEN UserTypes.Type ELSE ProjectUserTypes.AliasType END AS AliasType,
		CASE ProjectUserTypes.AliasDescription WHEN NULL THEN UserTypes.Description ELSE ProjectUserTypes.AliasDescription END AS AliasDescription
	FROM ProjectUserTypes
	LEFT JOIN UserTypes ON ProjectUserTypes.UserTypeID = UserTypes.UserTypeID
	WHERE ProjectUserTypes.ProjectID = 1646
	GO
	
	CREATE VIEW vw_GCLR3_ProjectEmployeeDetails AS 
	SELECT 
		EmployeeProjects.EmployeeID, 
		ProjectUserTypes.ProjectUserTypeID
	FROM EmployeeProjects
	LEFT JOIN ProjectUserTypes ON EmployeeProjects.ProjectUserTypeID = ProjectUserTypes.ProjectUserTypeID
	WHERE 
	EmployeeProjects.ProjectID = 1646
	AND EmployeeProjects.ProjectUserTypeID IS NOT NULL
	UNION
	SELECT
		EmployeeProjects.EmployeeID, 
		ProjectUserTypes.ProjectUserTypeID
	FROM EmployeeProjects
	CROSS JOIN ProjectUserTypes
	WHERE 
	EmployeeProjects.ProjectID = 1646
	AND EmployeeProjects.ProjectUserTypeID IS NULL
	AND ProjectUserTypes.UserTypeID IS NULL
	AND ProjectUserTypes.ProjectID = 1646
	GO
	
	CREATE VIEW vw_GCLR3_ProjectUserTimesheetItems AS 
	SELECT 
		EmployeeTimesheetItems.TimesheetItemID, 
		EmployeeTimesheets.EmployeeID, 
		EmployeeProjects.ProjectUserTypeID, 
		ProjectUserTypes.UserTypeID, 
		Employees.CompanyID,
		EmployeeTimesheets.TimesheetPeriodID, ProjectGroups.PartID, ProjectGroups.GroupID, ProjectTasks.TaskID, EmployeeTimesheetItems.VariationID, ProjectVariations.IsApproved, EmployeeTimesheetItems.TimeCode,
		CASE TimeCode WHEN 0 THEN Day1Hrs + Day2Hrs + Day3Hrs + Day4Hrs + Day5Hrs + Day6Hrs + Day7Hrs ELSE 0 END AS NTHours,
		CASE TimeCode WHEN 1 THEN Day1Hrs + Day2Hrs + Day3Hrs + Day4Hrs + Day5Hrs + Day6Hrs + Day7Hrs ELSE 0 END AS OT1Hours,
		CASE TimeCode WHEN 2 THEN Day1Hrs + Day2Hrs + Day3Hrs + Day4Hrs + Day5Hrs + Day6Hrs + Day7Hrs ELSE 0 END AS OT2Hours,
		CASE TimeCode WHEN 3 THEN Day1Hrs + Day2Hrs + Day3Hrs + Day4Hrs + Day5Hrs + Day6Hrs + Day7Hrs ELSE 0 END AS OT3Hours
	FROM EmployeeTimesheetItems
	INNER JOIN EmployeeTimesheets ON EmployeeTimesheets.TimesheetID = EmployeeTimesheetItems.TimesheetID
	INNER JOIN Employees ON EmployeeTimesheets.EmployeeID = Employees.EmployeeID
	INNER JOIN ProjectTasks ON EmployeeTimesheetItems.TaskID = ProjectTasks.TaskID
	INNER JOIN ProjectVariations ON EmployeeTimesheetItems.VariationID = ProjectVariations.VariationID
	INNER JOIN ProjectGroups ON ProjectTasks.GroupID = ProjectGroups.GroupID
	LEFT JOIN EmployeeProjects ON EmployeeTimesheets.EmployeeID = EmployeeProjects.EmployeeId
	LEFT JOIN ProjectUserTypes ON EmployeeProjects.ProjectUserTypeID = ProjectUserTypes.ProjectUserTypeID
	WHERE 
	ProjectTasks.ProjectID = 1646 
	AND EmployeeProjects.ProjectID = 1646
	AND EmployeeProjects.ProjectUserTypeID IS NOT NULL

	UNION

	SELECT
		EmployeeTimesheetItems.TimesheetItemID, 
		EmployeeTimesheets.EmployeeID, 
		ProjectUserTypes.ProjectUserTypeID, 
		0 AS [UserTypeID], 
		Employees.CompanyID,
		EmployeeTimesheets.TimesheetPeriodID, ProjectGroups.PartID, ProjectGroups.GroupID, ProjectTasks.TaskID, EmployeeTimesheetItems.VariationID, ProjectVariations.IsApproved, EmployeeTimesheetItems.TimeCode,
		CASE TimeCode WHEN 0 THEN Day1Hrs + Day2Hrs + Day3Hrs + Day4Hrs + Day5Hrs + Day6Hrs + Day7Hrs ELSE 0 END AS NTHours,
		CASE TimeCode WHEN 1 THEN Day1Hrs + Day2Hrs + Day3Hrs + Day4Hrs + Day5Hrs + Day6Hrs + Day7Hrs ELSE 0 END AS OT1Hours,
		CASE TimeCode WHEN 2 THEN Day1Hrs + Day2Hrs + Day3Hrs + Day4Hrs + Day5Hrs + Day6Hrs + Day7Hrs ELSE 0 END AS OT2Hours,
		CASE TimeCode WHEN 3 THEN Day1Hrs + Day2Hrs + Day3Hrs + Day4Hrs + Day5Hrs + Day6Hrs + Day7Hrs ELSE 0 END AS OT3Hours
	FROM EmployeeTimesheetItems
	INNER JOIN EmployeeTimesheets ON EmployeeTimesheets.TimesheetID = EmployeeTimesheetItems.TimesheetID
	INNER JOIN ProjectTasks ON EmployeeTimesheetItems.TaskID = ProjectTasks.TaskID
	INNER JOIN Employees ON EmployeeTimesheets.EmployeeID = Employees.EmployeeID
	INNER JOIN ProjectVariations ON EmployeeTimesheetItems.VariationID = ProjectVariations.VariationID
	INNER JOIN ProjectGroups ON ProjectTasks.GroupID = ProjectGroups.GroupID
	LEFT JOIN EmployeeProjects ON EmployeeTimesheets.EmployeeID = EmployeeProjects.EmployeeId
	CROSS JOIN ProjectUserTypes
	WHERE 
	ProjectTasks.ProjectID = 1646 
	AND EmployeeProjects.ProjectID = 1646
	AND EmployeeProjects.ProjectUserTypeID IS NULL
	AND ProjectUserTypes.UserTypeID IS NULL
	AND ProjectUserTypes.ProjectID = 1646
	GO