using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;
using Newtonsoft.Json;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextUserPlusOrAbove)]
    public class ReconciliationTemplatesController : BaseController
    {
        public ActionResult Index()
        {
            List<ReconciliationTemplatesIndexViewModel> vm = Db.ReconciliationTemplates.Select(x => new ReconciliationTemplatesIndexViewModel
            {
                Id = x.Id,
                Name = x.Name,
                CompanyName = x.Company.Company_Name,
                LastModifiedDateTime = x.LastModifiedDate

            }).OrderBy(x => x.CompanyName).ThenByDescending(x => x.LastModifiedDateTime).ToList();
            return View(vm);
        }

        public ActionResult CreateReconciliationTemplate()
        {
            ReconciliationTemplateCreateViewModel model = new ReconciliationTemplateCreateViewModel();

            SelectList companiesList = GetAvailableCompaniesDropdown();
            ViewBag.CompanyId = companiesList;

            return View(model);
        }

        [HttpPost]
        public ActionResult CreateReconciliationTemplate(ReconciliationTemplateCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SelectList companies = GetAvailableCompaniesDropdown();
                ViewBag.CompanyId = companies;
                return View(model);
            }

            InfoMessage message;

            if (!string.IsNullOrWhiteSpace(model.TypeIdentifierColumn) && string.IsNullOrWhiteSpace(model.TypeIdentifierText))
            {
                SelectList companies = GetAvailableCompaniesDropdown();
                ViewBag.CompanyId = companies;
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = $"You have entered an Identifier Column but have not entered any Identifier Values. Please fill these out and try again." };
                ViewBag.InfoMessage = message;
                return View(model);
            }

            List<ReconciliationTemplate> existingReconciliationTemplates = Db.ReconciliationTemplates.ToList();

            ReconciliationTemplate alreadyExistingEntry = existingReconciliationTemplates.SingleOrDefault(x => x.Name == model.Name && x.CompanyId == model.CompanyId);

            if (alreadyExistingEntry != null)
            {
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = $"A reconciliation import template already exists for this selected company for the name {model.Name}. Change the name and try again." };
                ViewBag.InfoMessage = message;
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.TypeIdentifierColumn))
            {
                model.TypeIdentifierText = null;
            }

            ReconciliationTemplate reconciliationTemplate = new ReconciliationTemplate
            {
                Name = model.Name,
                CompanyId = model.CompanyId,
                EmployeeNumberColumn = model.EmployeeNumberColumn,
                WeekEndingColumn = model.WeekEndingColumn,
                HoursColumn = model.HoursColumn,
                TypeIdentifierColumn = model.TypeIdentifierColumn,
                TypeIdentifierText = model.TypeIdentifierText,
                DailyDates = model.DailyDates
            };

            Db.ReconciliationTemplates.Add(reconciliationTemplate);

            Db.SaveChanges();

            message = new InfoMessage
            {
                MessageType = InfoMessageType.Success,
                MessageContent = "Successfully created new reconciliation import template."
            };

            TempData["message"] = message;
            return RedirectToAction("Index");
        }

        public ActionResult EditReconciliationTemplate(int? reconciliationTemplateId)
        {
            ReconciliationTemplate reconciliationTemplate = Db.ReconciliationTemplates.Single(x => x.Id == reconciliationTemplateId);

            ReconciliationTemplateUpdateViewModel vm = new ReconciliationTemplateUpdateViewModel
            {
                Name = reconciliationTemplate.Name,
                EmployeeNumberColumn = reconciliationTemplate.EmployeeNumberColumn,
                WeekEndingColumn = reconciliationTemplate.WeekEndingColumn,
                HoursColumn = reconciliationTemplate.HoursColumn,
                TypeIdentifierColumn = reconciliationTemplate.TypeIdentifierColumn,
                ReconciliationTemplateId = reconciliationTemplate.Id,
                TypeIdentifierText = reconciliationTemplate.TypeIdentifierText,
                DailyDates = reconciliationTemplate.DailyDates
            };

            return View(vm);
        }

        [HttpPost]
        public ActionResult EditReconciliationTemplate(ReconciliationTemplateUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            InfoMessage message;

            ReconciliationTemplate reconciliationTemplate = Db.ReconciliationTemplates.Single(x => x.Id == model.ReconciliationTemplateId);

            if (!string.IsNullOrWhiteSpace(model.TypeIdentifierColumn) && string.IsNullOrWhiteSpace(model.TypeIdentifierText))
            {
                SelectList companies = GetAvailableCompaniesDropdown();
                ViewBag.CompanyId = companies;
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = $"You have entered an Identifier Column but have not entered any Identifier Values. Please fill these out and try again." };
                ViewBag.InfoMessage = message;
                return View(model);
            }

            List<ReconciliationTemplate> existingReconciliationTemplates = Db.ReconciliationTemplates.ToList();

            ReconciliationTemplate alreadyExistingOtherEntry = existingReconciliationTemplates.SingleOrDefault(x => x.Name == model.Name && x.CompanyId == reconciliationTemplate.CompanyId && x.Id != model.ReconciliationTemplateId);

            if (alreadyExistingOtherEntry != null)
            {
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = $"A reconciliation import template already exists for this selected company for the name {model.Name}. Change the name and try again." };
                ViewBag.InfoMessage = message;
                return View(model);
            }

            reconciliationTemplate.Name = model.Name;
            reconciliationTemplate.EmployeeNumberColumn = model.EmployeeNumberColumn;
            reconciliationTemplate.WeekEndingColumn = model.WeekEndingColumn;
            reconciliationTemplate.HoursColumn = model.HoursColumn;
            reconciliationTemplate.TypeIdentifierColumn = model.TypeIdentifierColumn;
            reconciliationTemplate.DailyDates = model.DailyDates;

            if (string.IsNullOrWhiteSpace(reconciliationTemplate.TypeIdentifierColumn))
            {
                reconciliationTemplate.TypeIdentifierText = null;
            }
            else
            {
                reconciliationTemplate.TypeIdentifierText = model.TypeIdentifierText;
            }

            Db.SaveChanges();

            message = new InfoMessage
            {
                MessageType = InfoMessageType.Success,
                MessageContent = "Successfully updated reconciliation import template."
            };

            TempData["message"] = message;
            return RedirectToAction("Index");
        }

        private SelectList GetAvailableCompaniesDropdown()
        {
            List<SelectListItem> selectItems = Db.Companies.OrderBy(x => x.Company_Name).Select(x => new SelectListItem { Value = x.Company_Id.ToString(), Text = x.Company_Name }).ToList();
            return new SelectList(selectItems.OrderBy(x => x.Text), "Value", "Text");
        }

        [HttpPost]
        public ContentResult GetReconciliationTemplateDetails(int? id)
        {
            ReconciliationTemplate reconciliationTemplate = Db.ReconciliationTemplates.Single(x => x.Id == id);
            string jsonString = JsonConvert.SerializeObject(reconciliationTemplate);
            return new ContentResult { Content = jsonString, ContentType = "application/json" };
        }
    }
}