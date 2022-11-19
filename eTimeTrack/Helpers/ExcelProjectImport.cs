using System;
using System.Collections.Generic;
using System.Linq;
using eTimeTrack.Extensions;
using eTimeTrack.Models;
using OfficeOpenXml;

namespace eTimeTrack.Helpers
{
    public static class ExcelProjectImport
    {
        private const int BlankRowLimit = 0;

        public static ProjectStructureImportResults GenerateProjectModelFromExcel(Project project, ExcelWorksheet ws, ApplicationDbContext db, bool concatenateCodes, string concatenateCharacter)
        {
            List<ProjectPart> projectParts = db.ProjectParts.Where(x => x.ProjectID == project.ProjectID).ToList();
            List<ProjectGroup> projectGroups = db.ProjectGroups.Where(x => x.ProjectID == project.ProjectID).ToList();
            List<ProjectTask> projectTasks = db.ProjectTasks.Where(x => x.ProjectID == project.ProjectID).ToList();

            ProjectStructureImportResults results = new ProjectStructureImportResults();

            const int startRow = 2;

            const string headingPartCode = "Part Code";
            const string headingPartName = "Part Name";
            const string headingGroupCode = "Group Code";
            const string headingGroupName = "Group Name";
            const string headingTaskCode = "Task Code";
            const string headingTaskDescription = "Task Name";
            const string headingTaskNotes = "Task Notes";

            int colPartCode = ws.GetColumn(headingPartCode);
            int colPartName = ws.GetColumn(headingPartName);
            int colGroupCode = ws.GetColumn(headingGroupCode);
            int colGroupName = ws.GetColumn(headingGroupName);
            int colTaskCode = ws.GetColumn(headingTaskCode);
            int colTaskDescription = ws.GetColumn(headingTaskDescription);
            int colTaskNotes = ws.GetColumn(headingTaskNotes);

            int blankCounter = 0;
            // loop through rows until an empty one is detected
            for (int i = startRow; i < int.MaxValue; i++)
            {
                string partCode = ws.Cells[i, colPartCode].Text?.Trim();
                string partName = ws.Cells[i, colPartName].Text?.Trim().Truncate(150);
                string groupCode = ws.Cells[i, colGroupCode].Text?.Trim();
                string groupName = ws.Cells[i, colGroupName].Text?.Trim().Truncate(150);
                string taskCode = ws.Cells[i, colTaskCode].Text?.Trim();
                string taskDescription = ws.Cells[i, colTaskDescription].Text?.Trim().Truncate(150);
                string taskNotes = ws.Cells[i, colTaskNotes].Text?.Trim();

                // concatenate if required
                if (concatenateCodes)
                {
                    groupCode = partCode + concatenateCharacter + groupCode;
                    taskCode = groupCode + concatenateCharacter + taskCode;
                }

                if (partCode != null && partCode.Length > 10)
                {
                    throw new Exception("Part Code greater than 10 characters: " + partCode);
                }

                if (groupCode != null && groupCode.Length > 12)
                {
                    throw new Exception("Group Code greater than 12 characters: " + groupCode);
                }

                if (taskCode != null && taskCode.Length > 30)
                {
                    throw new Exception("Task Code greater than 30 characters: " + taskCode);
                }

                // end of file when breaks are detected
                if (CheckBlankCounter(partCode, ref blankCounter)) break;

                // Project Part
                ProjectPart projectPart = projectParts.SingleOrDefault(x => x.PartNo == partCode);
                if (projectPart == null)
                {
                    projectPart = new ProjectPart { Project = project, ProjectID = project.ProjectID, PartNo = partCode, Name = partName };
                    projectParts.Add(projectPart);
                    db.ProjectParts.Add(projectPart);
                    results.PartsAdded++;
                }

                // Project Group
                ProjectGroup projectGroup = projectGroups.SingleOrDefault(x => x.PartID == projectPart.PartID && x.GroupNo == groupCode);
                if (projectGroup == null)
                {
                    projectGroup = new ProjectGroup { Project = project, ProjectPart = projectPart, PartID = projectPart.PartID, GroupNo = groupCode, Name = groupName };
                    db.ProjectGroups.Add(projectGroup);
                    projectGroups.Add(projectGroup);
                    results.GroupsAdded++;
                }

                // Project Task
                ProjectTask projectTask = projectTasks.SingleOrDefault(x => x.GroupID == projectGroup.GroupID && x.TaskNo == taskCode);
                if (projectTask == null)
                {
                    projectTask = new ProjectTask { Project = project, ProjectGroup = projectGroup, GroupID = projectGroup.GroupID, TaskNo = taskCode, Name = taskDescription, Notes = taskNotes };
                    projectTasks.Add(projectTask);
                    db.ProjectTasks.Add(projectTask);
                    results.TasksAdded++;
                }
            }

            db.SaveChanges();

            return results;
        }

        private static bool CheckBlankCounter(string partCode, ref int blankCounter)
        {
            if (string.IsNullOrWhiteSpace(partCode))
            {
                blankCounter++;
            }
            else
            {
                blankCounter = 0;
            }
            return blankCounter > BlankRowLimit;
        }
    }

    public class ProjectStructureImportResults
    {
        public int PartsAdded { get; set; }
        public int GroupsAdded { get; set; }
        public int TasksAdded { get; set; }
    }
}