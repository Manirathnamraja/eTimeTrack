CREATE VIEW vw_SGA_Employees AS
SELECT        dbo.Employees.EmployeeID, dbo.Employees.EmployeeNo, dbo.Employees.IsActive, dbo.Employees.ManagerID, dbo.Employees.CompanyID, 
                         dbo.Employees.OfficeID, dbo.Employees.AllowOT, dbo.Employees.LastModifiedBy, dbo.Employees.LastModifiedDate, dbo.Employees.Email, 
                         dbo.Employees.EmailConfirmed, dbo.Employees.Names AS UserName
FROM            dbo.Employees INNER JOIN
                         dbo.EmployeeProjects ON dbo.EmployeeProjects.EmployeeId = dbo.Employees.EmployeeID
WHERE        (dbo.EmployeeProjects.ProjectId = 1408)
GO

  CREATE VIEW vw_SGA_EmployeeTimesheetItems AS
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
  FROM [dbo].[EmployeeTimesheetItems] ETI
  INNER JOIN [dbo].[ProjectVariations] PV ON ETI.VariationID = PV.VariationID
  WHERE PV.ProjectID = 1408
GO
  
  CREATE VIEW vw_SGA_EmployeeTimesheets AS
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
  WHERE PV.ProjectID = 1408
  GO
  
  CREATE VIEW vw_SGA_ProjectGroups AS 
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
	WHERE (ProjectID = 1408)
	GO
  
  CREATE VIEW vw_SGA_ProjectParts AS 
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
	WHERE (ProjectID = 1408)
	GO
  
  CREATE VIEW vw_SGA_Projects AS 
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
	WHERE (ProjectID = 1408)
	GO
  
  CREATE VIEW vw_SGA_ProjectTasks AS 
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
	WHERE (ProjectID = 1408)
	GO

  CREATE VIEW vw_SGA_ProjectVariationItems AS 
  SELECT PVI.[VariationID]
      ,[TaskID]
      ,PVI.[IsApproved]
      ,PVI.[IsClosed]
      ,PVI.[LastModifiedBy]
      ,PVI.[LastModifiedDate]
  FROM [dbo].[ProjectVariationItems] PVI
  INNER JOIN [dbo].[ProjectVariations] PV ON PVI.VariationID = PV.VariationID
  WHERE PV.ProjectID = 1408
  GO
  
  CREATE VIEW vw_SGA_ProjectVariations AS 
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
	WHERE (ProjectID = 1408)
	GO