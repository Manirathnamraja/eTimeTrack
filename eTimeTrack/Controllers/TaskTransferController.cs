using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
    public class TaskTransferController : BaseController
    {
        public ActionResult TaskSelect()
        {
            int selectedProject = (int?)Session?["SelectedProject"] ?? 0;
            if (selectedProject == 0) { return InvokeHttp404(HttpContext); }

            TaskTransferSelectViewModel model = new TaskTransferSelectViewModel
            {
                ProjectId = selectedProject,
                ProjectParts = GetProjectPartsSelect(selectedProject)
            };
            SelectList select = GetProjectTimesheetPeriodsSelect(selectedProject);
            model.DateSelect = select;

            ViewBag.InfoMessage = TempData["InfoMessage"];
            return View(model);
        }

        [HttpPost]
        public ActionResult TaskSelect(TaskTransferSelectViewModel model)
        {
            if ((model.TaskFrom?.TaskID ?? 0) == 0 || (model.TaskTo?.TaskID ?? 0) == 0 ||
                (model.VariationFrom?.VariationID ?? 0) == 0 || (model.VariationTo?.VariationID ?? 0) == 0)
            {
                TempData["InfoMessage"] = new InfoMessage { MessageContent = "Please select all required parameters", MessageType = InfoMessageType.Failure };
                return RedirectToAction("TaskSelect");
            }

            return RedirectToAction("ItemSelect", new { taskFromId = model.TaskFrom?.TaskID, taskToId = model.TaskTo?.TaskID, variationFromId = model.VariationFrom?.VariationID, variationToId = model.VariationTo?.VariationID, startDate = model.StartDate, endDate = model.EndDate });
        }

        public ActionResult ItemSelect(int taskFromId, int taskToId, int variationFromId, int variationToId, int? startDate, int? endDate)
        {
            ProjectVariationItem variationItemFrom = Db.ProjectVariationItems.Find(variationFromId, taskFromId);
            ProjectVariationItem variationItemTo = Db.ProjectVariationItems.Find(variationToId, taskToId);

            TimesheetPeriod startPeriod = Db.TimesheetPeriods.Find(startDate);
            TimesheetPeriod endPeriod = Db.TimesheetPeriods.Find(endDate);

            if (startPeriod?.EndDate != null && endPeriod?.EndDate != null && startPeriod.EndDate > endPeriod.EndDate)
            {
                TempData["InfoMessage"] = new InfoMessage { MessageContent = "End date must not be before start date. Please try again.", MessageType = InfoMessageType.Failure };
                return RedirectToAction("TaskSelect");
            }

            List<EmployeeTimesheetItem> taskVariationItems = Db.EmployeeTimesheetItems.Where(x => x.TaskID == variationItemFrom.TaskID && x.VariationID == variationItemFrom.VariationID).OrderBy(x => x.Timesheet.TimesheetPeriod.EndDate).ToList();

            List<TransferItemViewModel> timeRelevantItems = taskVariationItems.Where(x => (startPeriod == null || x.Timesheet.TimesheetPeriod.EndDate >= startPeriod.EndDate) && (endPeriod == null || x.Timesheet.TimesheetPeriod.EndDate <= endPeriod.EndDate)).Select(x => new TransferItemViewModel { EmployeeTimesheetItem = x }).ToList();

            return View(new TaskTransferItemViewModel { ProjectVariationItemFrom = variationItemFrom, ProjectVariationItemTo = variationItemTo, EmployeeTimesheetItems = timeRelevantItems });
        }

        [HttpPost]
        public ActionResult ItemSelect(TaskTransferItemViewModel model)
        {
            ProjectVariationItem variationItemFrom = Db.ProjectVariationItems.Find(model.ProjectVariationItemFrom.VariationID, model.ProjectVariationItemFrom.TaskID);
            ProjectVariationItem variationItemTo = Db.ProjectVariationItems.Find(model.ProjectVariationItemTo.VariationID, model.ProjectVariationItemTo.TaskID);

            if (variationItemFrom == null || variationItemTo == null)
            {
                return InvokeHttp400(HttpContext);
            }

            TransferItems(model);

            TempData["InfoMessage"] = new InfoMessage { MessageContent = $"<p>{model.EmployeeTimesheetItems.Count(x => x.Transfer)} timesheet items successfully transferred</p><p>From:</p><ul><li>Task: {variationItemFrom.ProjectTask.DisplayName}</li><li>Variation: {variationItemFrom.ProjectVariation.DisplayName}</li></ul><p>To:</p><ul><li>{variationItemTo.ProjectTask.DisplayName}</li><li>Variation: {variationItemTo.ProjectVariation.DisplayName}</li></ul>", MessageType = InfoMessageType.Success };
            return RedirectToAction("TaskSelect");
        }

        private void TransferItems(TaskTransferItemViewModel model)
        {
            List<int> itemsToTransfer = model.EmployeeTimesheetItems.Where(x => x.Transfer)
                .Select(x => x.EmployeeTimesheetItem.TimesheetItemID).ToList();

            foreach (int id in itemsToTransfer)
            {
                EmployeeTimesheetItem item = Db.EmployeeTimesheetItems.Find(id);

                if (item == null) continue;

                item.TaskID = model.ProjectVariationItemTo.TaskID;
                item.VariationID = model.ProjectVariationItemTo.VariationID;
                Db.SaveChanges();
            }
        }
    }
}