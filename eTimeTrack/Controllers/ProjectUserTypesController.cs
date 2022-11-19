using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Models;
using eTimeTrack.Helpers;
using eTimeTrack.ViewModels;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.RoleSuperUser)]
    public class ProjectUserTypesController : BaseController
    {
        public ActionResult Index()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            List<ProjectUserType> projectUserTypes = Db.ProjectUserTypes.Where(x => x.ProjectID == projectId).Include(x => x.UserType).OrderByDescending(x => x.UserType.Code).ToList();

            List<ProjectUserTypeIndexViewModel> model = projectUserTypes.Select(x => new ProjectUserTypeIndexViewModel
            {
                ProjectUserTypeID = x.ProjectUserTypeID,
                AliasCode = x.AliasCode,
                AliasType = x.AliasType,
                AliasDescription = x.AliasDescription,
                IsGenericUserType = !x.UserTypeID.HasValue,
                ProjectID = x.ProjectID,
                MaxNTHours = x.MaxNTHours,
                MaxOT1Hours = x.MaxOT1Hours,
                MaxOT2Hours = x.MaxOT2Hours,
                MaxOT3Hours = x.MaxOT3Hours,
                UserTypeName = x.UserType == null ? UserTypesController.GenericUserTypeText : x.UserType.Code + ": " + x.UserType.Type
            }).ToList();

            ProjectUserTypeIndexViewModel genericUserType = model.SingleOrDefault(x => x.IsGenericUserType);

            if (genericUserType == null)
            {
                ViewBag.NoGenericProjectUserType = true;
                model.Insert(0, new ProjectUserTypeIndexViewModel
                {
                    ProjectUserTypeID = null,
                    ProjectID = projectId,
                    UserTypeName = UserTypesController.GenericUserTypeText
                });
            }
            else
            {
                ViewBag.NoGenericProjectUserType = false;
                model.Remove(genericUserType);
                model.Insert(0, genericUserType);
            }

            ViewBag.InfoMessage = TempData["message"];
            return View(model);
        }

        public ActionResult CreateProjectUserType()
        {
            int projectId = (int?)Session["SelectedProject"] ?? 0;
            Project project = Db.Projects.Find(projectId) ?? Db.Projects.OrderBy(x => x.ProjectNo).First();

            if (project == null)
            {
                return InvokeHttp404(HttpContext);
            }

            ProjectUserTypeCreateViewModel model = new ProjectUserTypeCreateViewModel
            {
                ProjectID = project.ProjectID,
                UserTypeID = null
            };

            SelectList availableProjectUserTypes = GetAvailableUserTypesDropdown(project.ProjectID);
            ViewBag.UserTypeID = availableProjectUserTypes;

            return View(model);
        }

        [HttpPost]
        public ActionResult CreateProjectUserType(ProjectUserTypeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SelectList availableProjectUserTypes = GetAvailableUserTypesDropdown(model.ProjectID);
                ViewBag.UserTypeID = availableProjectUserTypes;
                return View(model);
            }

            InfoMessage message;

            List<ProjectUserType> allExistingProjectUserTypes = Db.ProjectUserTypes.Where(x => x.ProjectID == model.ProjectID).ToList();

            var alreadyExistingEntry = allExistingProjectUserTypes.SingleOrDefault(x => x.UserTypeID == model.UserTypeID);

            if (alreadyExistingEntry != null)
            {
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = $"A project user type for user type {alreadyExistingEntry.UserType.Code} already exists for this project." };
                ViewBag.InfoMessage = message;
                return View(model);
            }

            bool validAlias = model.AliasCode == null || !allExistingProjectUserTypes.Select(x => x.AliasCode).Contains(model.AliasCode);

            if (!validAlias)
            {
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = "Alias is already taken. Cannot create new project user type." };
                TempData["message"] = message;
                SelectList availableProjectUserTypes = GetAvailableUserTypesDropdown(model.ProjectID);
                ViewBag.UserTypeID = availableProjectUserTypes;
                return View(model);
            }

            ProjectUserType projectUserTypes = new ProjectUserType
            {
                IsEnabled = true,
                AliasCode = model.AliasCode,
                AliasType = model.AliasType,
                AliasDescription = model.AliasDescription,
                MaxNTHours = model.MaxNTHours,
                MaxOT1Hours = model.MaxOT1Hours,
                MaxOT2Hours = model.MaxOT2Hours,
                MaxOT3Hours = model.MaxOT3Hours,
                ProjectID = model.ProjectID,
                UserTypeID = (int)model.UserTypeID
            };

            Db.ProjectUserTypes.Add(projectUserTypes);

            Db.SaveChanges();

            message = new InfoMessage
            {
                MessageType = InfoMessageType.Success,
                MessageContent = "Successfully created new project user type."
            };

            TempData["message"] = message;
            return RedirectToAction("Index");
        }

        public ActionResult EditProjectUserType(int? projectUserTypeId, int projectId)
        {
            ProjectUserType projectUserType;

            bool isGenericUserType = !projectUserTypeId.HasValue;

            if (projectUserTypeId.HasValue)
            {
                projectUserType = Db.ProjectUserTypes.Include(x => x.UserType).Single(x => x.ProjectUserTypeID == projectUserTypeId);
            }
            else
            {
                projectUserType = Db.ProjectUserTypes.SingleOrDefault(x => x.ProjectID == projectId && x.UserTypeID == null);

                if (projectUserType == null)
                {
                    projectUserType = new ProjectUserType
                    {
                        ProjectID = projectId,
                        UserTypeID = 0
                    };
                }
            }

            ProjectUserTypeUpdateViewModel model = new ProjectUserTypeUpdateViewModel
            {
                ProjectID = projectUserType.ProjectID,
                UserTypeID = projectUserType.UserTypeID ?? 0,
                AliasCode = projectUserType.AliasCode,
                AliasType = projectUserType.AliasType,
                AliasDescription = projectUserType.AliasDescription,
                MaxNTHours = projectUserType.MaxNTHours,
                MaxOT1Hours = projectUserType.MaxOT1Hours,
                MaxOT2Hours = projectUserType.MaxOT2Hours,
                MaxOT3Hours = projectUserType.MaxOT3Hours,
                ProjectUserTypeID = projectUserType.ProjectUserTypeID,
                UserTypeName = isGenericUserType ? UserTypesController.GenericUserTypeText : projectUserType.UserType?.Code + ": " + projectUserType.UserType?.Type,
                IsGenericUserType = isGenericUserType
            };

            if (model.UserTypeName == ": ")
            {
                model.UserTypeName = UserTypesController.GenericUserTypeText;
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditProjectUserType(ProjectUserTypeUpdateViewModel model)
       {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            InfoMessage message;

            List<ProjectUserType> allExistingProjectUserTypes = Db.ProjectUserTypes.Where(x => x.ProjectID == model.ProjectID).ToList();

            bool validAlias = model.AliasCode == null || !allExistingProjectUserTypes.Where(x => model.ProjectUserTypeID == null ? x.ProjectID == model.ProjectID && x.UserTypeID == null : x.ProjectUserTypeID != model.ProjectUserTypeID).Select(x => x.AliasCode).Contains(model.AliasCode);

            if (!validAlias)
            {
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = "Alias is already taken. Cannot update project user type." };
                ViewBag.InfoMessage = message;
                return View(model);
            }

            ProjectUserType existing = Db.ProjectUserTypes.Find(model.ProjectUserTypeID);

           if (existing == null && model.IsGenericUserType)
           {
               existing = new ProjectUserType
               {
                   ProjectID = model.ProjectID
               };
               Db.ProjectUserTypes.Add(existing);
           }

            existing.AliasCode = model.AliasCode;
            existing.AliasType = model.AliasType;
            existing.AliasDescription = model.AliasDescription;
            existing.MaxNTHours = model.MaxNTHours;
            existing.MaxOT1Hours = model.MaxOT1Hours;
            existing.MaxOT2Hours = model.MaxOT2Hours;
            existing.MaxOT3Hours = model.MaxOT3Hours;

            Db.SaveChanges();

            message = new InfoMessage
            {
                MessageType = InfoMessageType.Success,
                MessageContent = "Successfully updated project user type."
            };

            TempData["message"] = message;
            return RedirectToAction("Index");
        }

        private SelectList GetAvailableUserTypesDropdown(int projectId)
        {
            List<int?> existingProjectUserTypes = Db.ProjectUserTypes.Where(x => x.ProjectID == projectId && x.UserTypeID != null).Select(x => x.UserTypeID).ToList();
            List<SelectListItem> selectItems = Db.UserTypes.Where(x => !existingProjectUserTypes.Contains(x.UserTypeID)).OrderBy(x => x.UserTypeID).Select(x => new SelectListItem { Value = x.UserTypeID.ToString(), Text = x.Code }).ToList();
            SelectList availableProjectUserTypes = new SelectList(selectItems, "Value", "Text");
            return availableProjectUserTypes;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}