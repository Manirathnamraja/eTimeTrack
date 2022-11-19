/****** Script for SelectTopNRows command from SSMS  ******/
SELECT ProjectTasks.TaskNo, ProjectVariations.VariationNo, ProjectVariationItems.IsApproved, ProjectVariationItems.IsClosed, ProjectVariationItems.LastModifiedDate, Names
  FROM [eTimeTrack].[dbo].[ProjectVariationItems]
  left join ProjectVariations on ProjectVariationItems.VariationID = ProjectVariations.VariationID
  left join ProjectTasks on ProjectVariationItems.TaskID = ProjectTasks.TaskID
  left join Employees on ProjectVariationItems.LastModifiedBy = Employees.EmployeeID
  where ProjectVariations.VariationID = 702 and ProjectVariationItems.lastmodifieddate > '2017-04-30'