CREATE VIEW vw_BORRDD_Employees AS
SELECT        dbo.Employees.EmployeeID, dbo.Employees.EmployeeNo, dbo.Employees.IsActive, dbo.Employees.ManagerID, dbo.Employees.CompanyID, 
                         dbo.Employees.OfficeID, dbo.Employees.AllowOT, dbo.Employees.LastModifiedBy, dbo.Employees.LastModifiedDate, dbo.Employees.Email, 
                         dbo.Employees.EmailConfirmed, dbo.Employees.Names AS UserName
FROM            dbo.Employees INNER JOIN
                         dbo.EmployeeProjects ON dbo.EmployeeProjects.EmployeeId = dbo.Employees.EmployeeID
WHERE        (dbo.EmployeeProjects.ProjectId = 1610)
GO

  CREATE VIEW vw_BORRDD_EmployeeTimesheetItems AS
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
  WHERE PV.ProjectID = 1610
  AND (ETI.[Day1Hrs] <> 0 OR ETI.[Day2Hrs] <> 0 OR ETI.[Day3Hrs] <> 0 OR ETI.[Day4Hrs] <> 0 OR ETI.[Day5Hrs] <> 0 OR ETI.[Day6Hrs] <> 0 OR ETI.[Day7Hrs] <> 0)
GO
  
  CREATE VIEW vw_BORRDD_EmployeeTimesheets AS
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
  WHERE PV.ProjectID = 1610
  GO
  
  CREATE VIEW vw_BORRDD_ProjectGroups AS 
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
	WHERE (ProjectID = 1610)
	GO
  
  CREATE VIEW vw_BORRDD_ProjectParts AS 
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
	WHERE (ProjectID = 1610)
	GO
  
  CREATE VIEW vw_BORRDD_Projects AS 
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
	WHERE (ProjectID = 1610)
	GO
  
  CREATE VIEW vw_BORRDD_ProjectTasks AS 
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
	WHERE (ProjectID = 1610)
	GO

  CREATE VIEW vw_BORRDD_ProjectVariationItems AS 
  SELECT PVI.[VariationID]
      ,[TaskID]
      ,PVI.[IsApproved]
      ,PVI.[IsClosed]
      ,PVI.[LastModifiedBy]
      ,PVI.[LastModifiedDate]
  FROM [dbo].[ProjectVariationItems] PVI
  INNER JOIN [dbo].[ProjectVariations] PV ON PVI.VariationID = PV.VariationID
  WHERE PV.ProjectID = 1610
  GO
  
  CREATE VIEW vw_BORRDD_ProjectVariations AS 
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
	WHERE (ProjectID = 1610)
	GO
	
	CREATE VIEW vw_BORRDD_ProjectTimeCodeConfigs AS 
	SELECT
		ProjectTimeCodeConfigId,
		ProjectID,
		DisplayOT1,
		DisplayOT2,
		DisplayOT3,
		NTName,
		OT1Name,
		OT2Name,
		OT3Name,
		NTNotes,
		OT1Notes,
		OT2Notes,
		OT3Notes
	FROM dbo.ProjectTimeCodeConfigs
	WHERE ProjectID = 1610
	GO
	
	CREATE VIEW vw_BORRDD_Custom_Report AS 
	SELECT
	dbo.EmployeeTimesheetItems.TimesheetItemID, dbo.EmployeeTimesheetItems.LastModifiedBy, dbo.EmployeeTimesheetItems.LastModifiedDate, dbo.EmployeeTimesheetItems.TimesheetID, 
	dbo.EmployeeTimesheetItems.Day1Hrs, dbo.EmployeeTimesheetItems.Day2Hrs, dbo.EmployeeTimesheetItems.Day3Hrs, dbo.EmployeeTimesheetItems.Day4Hrs, dbo.EmployeeTimesheetItems.Day5Hrs, 
	dbo.EmployeeTimesheetItems.Day6Hrs, dbo.EmployeeTimesheetItems.Day7Hrs, 
	dbo.EmployeeTimesheetItems.Day1Hrs + dbo.EmployeeTimesheetItems.Day2Hrs + dbo.EmployeeTimesheetItems.Day3Hrs + dbo.EmployeeTimesheetItems.Day4Hrs + dbo.EmployeeTimesheetItems.Day5Hrs + dbo.EmployeeTimesheetItems.Day6Hrs
	+ dbo.EmployeeTimesheetItems.Day7Hrs AS Total, CASE TimeCode WHEN 0 THEN 'NT' WHEN 1 THEN 'OT1' WHEN 2 THEN 'OT2' WHEN 3 THEN 'OT3' END AS TimeCode, dbo.EmployeeTimesheetItems.Comments, 
	dbo.Employees.EmployeeID, dbo.Employees.EmployeeNo, dbo.Employees.Names, dbo.ProjectTasks.TaskID, dbo.ProjectTasks.TaskNo, dbo.ProjectVariations.VariationID, dbo.ProjectVariations.VariationNo, 
	dbo.ProjectVariations.Description AS [Variation Description], dbo.ProjectTasks.Name, dbo.ProjectGroups.GroupID, dbo.ProjectGroups.GroupNo, dbo.ProjectGroups.Name AS GroupName, dbo.ProjectParts.PartID, 
	dbo.ProjectParts.PartNo, dbo.ProjectParts.Name AS PartName, dbo.TimesheetPeriods.WeekNo, dbo.TimesheetPeriods.TimesheetPeriodID, dbo.TimesheetPeriods.StartDate, dbo.TimesheetPeriods.EndDate, 
	dbo.Companies.Company_Id, dbo.Companies.Company_Code, dbo.Companies.Company_Name
	FROM
	dbo.EmployeeTimesheetItems INNER JOIN
	dbo.EmployeeTimesheets ON dbo.EmployeeTimesheetItems.TimesheetID = dbo.EmployeeTimesheets.TimesheetID INNER JOIN
	dbo.TimesheetPeriods ON dbo.EmployeeTimesheets.TimesheetPeriodID = dbo.TimesheetPeriods.TimesheetPeriodID INNER JOIN
	dbo.Employees ON dbo.EmployeeTimesheets.EmployeeID = dbo.Employees.EmployeeID INNER JOIN
	dbo.Companies ON dbo.Employees.CompanyID = dbo.Companies.Company_Id INNER JOIN
	dbo.ProjectTasks ON dbo.EmployeeTimesheetItems.TaskID = dbo.ProjectTasks.TaskID INNER JOIN
	dbo.ProjectGroups ON dbo.ProjectTasks.GroupID = dbo.ProjectGroups.GroupID INNER JOIN
	dbo.ProjectParts ON dbo.ProjectGroups.PartID = dbo.ProjectParts.PartID INNER JOIN
	dbo.Projects ON dbo.ProjectParts.ProjectID = dbo.Projects.ProjectID INNER JOIN
	dbo.ProjectVariations ON dbo.EmployeeTimesheetItems.VariationID = dbo.ProjectVariations.VariationID
	WHERE dbo.Projects.ProjectID = 1610
	AND ([Day1Hrs] <> 0 OR [Day2Hrs] <> 0 OR [Day3Hrs] <> 0 OR [Day4Hrs] <> 0 OR [Day5Hrs] <> 0 OR [Day6Hrs] <> 0 OR [Day7Hrs] <> 0)
	GO