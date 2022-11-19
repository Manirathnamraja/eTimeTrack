CREATE VIEW vw_CRLLA_Employees AS
SELECT        dbo.Employees.EmployeeID, dbo.Employees.EmployeeNo, dbo.Employees.IsActive, dbo.Employees.ManagerID, dbo.Employees.CompanyID, 
                         dbo.Employees.OfficeID, dbo.Employees.AllowOT, dbo.Employees.LastModifiedBy, dbo.Employees.LastModifiedDate, dbo.Employees.Email, 
                         dbo.Employees.EmailConfirmed, dbo.Employees.Names AS UserName
FROM            dbo.Employees INNER JOIN
                         dbo.EmployeeProjects ON dbo.EmployeeProjects.EmployeeId = dbo.Employees.EmployeeID
WHERE        (dbo.EmployeeProjects.ProjectId = 1505)
GO

  CREATE VIEW vw_CRLLA_EmployeeTimesheetItems AS
  SELECT [TimesheetItemID]
      ,[TimesheetID]
      ,ETI.[VariationID]
      ,[TaskID]
      ,[OTCode]
      ,[PayTypeID]
      ,[ItemNo]
      ,[Day1Hrs]
      ,[Day1Comments]
      ,[Day2Hrs]
      ,[Day2Comments]
      ,[Day3Hrs]
      ,[Day3Comments]
      ,[Day4Hrs]
      ,[Day4Comments]
      ,[Day5Hrs]
      ,[Day5Comments]
      ,[Day6Hrs]
      ,[Day6Comments]
      ,[Day7Hrs]
      ,[Day7Comments]
      ,[InvoiceID]
      ,[Comments]
      ,ETI.[LastModifiedBy]
      ,ETI.[LastModifiedDate]
	  ,TimeCode
  FROM [dbo].[EmployeeTimesheetItems] ETI
  INNER JOIN [dbo].[ProjectVariations] PV ON ETI.VariationID = PV.VariationID
  WHERE PV.ProjectID = 1505
GO
  
  CREATE VIEW vw_CRLLA_EmployeeTimesheets AS
SELECT DISTINCT ET.[TimesheetID]
      ,[EmployeeID]
      ,[TimesheetPeriodID]
      ,[ApprovedByID]
      ,ET.[DateApproved]
      ,[UseDayTimeEntry]
      ,[Day1StartTime]
      ,[Day1EndTime]
      ,[Day2StartTime]
      ,[Day2EndTime]
      ,[Day3StartTime]
      ,[Day3EndTime]
      ,[Day4StartTime]
      ,[Day4EndTime]
      ,[Day5StartTime]
      ,[Day5EndTime]
      ,[Day6StartTime]
      ,[Day6EndTime]
      ,[Day7StartTime]
      ,[Day7EndTime]
      ,ET.[LastModifiedBy]
      ,ET.[LastModifiedDate]
  FROM [dbo].[EmployeeTimesheets] ET
  INNER JOIN [dbo].[EmployeeTimesheetItems] ETI ON ET.TimesheetID = ETI.TimesheetID
  INNER JOIN [dbo].[ProjectVariations] PV ON ETI.VariationID = PV.VariationID
  WHERE PV.ProjectID = 1505
  GO
  
  CREATE VIEW vw_CRLLA_ProjectGroups AS 
  SELECT [GroupID]
      ,[ProjectID]
      ,[PartID]
      ,[GroupNo]
      ,[Name]
      ,[Notes]
      ,[GroupTypeID]
      ,[IsClosed]
      ,[AliasCode]
      ,[LastModifiedBy]
      ,[LastModifiedDate]
      ,[PM]
	FROM [dbo].[ProjectGroups]
	WHERE (ProjectID = 1505)
	GO
  
  CREATE VIEW vw_CRLLA_ProjectParts AS 
  SELECT [PartID]
      ,[ProjectID]
      ,[PartNo]
      ,[Name]
      ,[Notes]
      ,[IsClosed]
      ,[LastModifiedBy]
      ,[LastModifiedDate]
      ,[PM]
	FROM [dbo].[ProjectParts]
	WHERE (ProjectID = 1505)
	GO
  
  CREATE VIEW vw_CRLLA_Projects AS 
  SELECT [ProjectID]
      ,[ProjectNo]
      ,[OfficeID]
      ,[SeqNo]
      ,[YearNo]
      ,[RegistrationNo]
      ,[Name]
      ,[DirectorID]
      ,[ManagerID]
      ,[IsClosed]
      ,[DateClosed]
      ,[DateOpened]
      ,[ClientCompanyID]
      ,[ClientContactID]
      ,[Notes]
      ,[LastModifiedBy]
      ,[LastModifiedDate]
	FROM [dbo].[Projects]
	WHERE (ProjectID = 1505)
	GO
  
  CREATE VIEW vw_CRLLA_ProjectTasks AS 
  SELECT [TaskID]
      ,[ProjectID]
      ,[GroupID]
      ,[TaskNo]
      ,[Name]
      ,[AliasCode]
      ,[OracleCode]
      ,[IsClosed]
      ,[Notes]
      ,[LastModifiedBy]
      ,[LastModifiedDate]
      ,[PM]
	FROM [dbo].[ProjectTasks]
	WHERE (ProjectID = 1505)
	GO

  CREATE VIEW vw_CRLLA_ProjectVariationItems AS 
  SELECT PVI.[VariationID]
      ,[TaskID]
      ,PVI.[IsApproved]
      ,PVI.[IsClosed]
      ,PVI.[LastModifiedBy]
      ,PVI.[LastModifiedDate]
  FROM [dbo].[ProjectVariationItems] PVI
  INNER JOIN [dbo].[ProjectVariations] PV ON PVI.VariationID = PV.VariationID
  WHERE PV.ProjectID = 1505
  GO
  
  CREATE VIEW vw_CRLLA_ProjectVariations AS 
  SELECT [VariationID]
      ,[ProjectID]
      ,[VariationNo]
      ,[RevNo]
      ,[Description]
      ,[IsClosed]
      ,[DateSubmitted]
      ,[IsApproved]
      ,[DateApproved]
      ,[Reference]
      ,[Notes]
      ,[IsOriginalScope]
      ,[LastModifiedBy]
      ,[LastModifiedDate]
	FROM [dbo].[ProjectVariations]
	WHERE (ProjectID = 1505)
	GO
	
	CREATE VIEW vw_CRLLA_ProjectUserTypes AS 
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
	WHERE ProjectUserTypes.ProjectID = 1505
	GO
	
	CREATE VIEW vw_CRLLA_ProjectEmployeeDetails AS 
	SELECT 
		EmployeeProjects.EmployeeID, 
		ProjectUserTypes.ProjectUserTypeID
	FROM EmployeeProjects
	LEFT JOIN ProjectUserTypes ON EmployeeProjects.ProjectUserTypeID = ProjectUserTypes.ProjectUserTypeID
	WHERE 
	EmployeeProjects.ProjectID = 1505
	AND EmployeeProjects.ProjectUserTypeID IS NOT NULL
	UNION
	SELECT
		EmployeeProjects.EmployeeID, 
		ProjectUserTypes.ProjectUserTypeID
	FROM EmployeeProjects
	CROSS JOIN ProjectUserTypes
	WHERE 
	EmployeeProjects.ProjectID = 1505
	AND EmployeeProjects.ProjectUserTypeID IS NULL
	AND ProjectUserTypes.UserTypeID IS NULL
	AND ProjectUserTypes.ProjectID = 1505
	GO
	
	CREATE VIEW vw_CRLLA_ProjectUserTimesheetItems AS 
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
	ProjectTasks.ProjectID = 1505 
	AND EmployeeProjects.ProjectID = 1505
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
	ProjectTasks.ProjectID = 1505 
	AND EmployeeProjects.ProjectID = 1505
	AND EmployeeProjects.ProjectUserTypeID IS NULL
	AND ProjectUserTypes.UserTypeID IS NULL
	AND ProjectUserTypes.ProjectID = 1505
	GO