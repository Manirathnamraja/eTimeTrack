using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Models;
using eTimeTrack.Helpers;
using eTimeTrack.ViewModels;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.RoleSuperUser)]
    public class UserTypesController : BaseController
    {
        public const string GenericUserTypeTextCode = "0";
        public const string GenericUserTypeTextType = "Default / Unassigned";
        public const string GenericUserTypeText = "0: Default / Unassigned";
        public ActionResult Index()
        {
            List<UserType> userTypes = Db.UserTypes.OrderBy(x => x.UserTypeID).ToList();

            userTypes.Insert(0, new UserType {UserTypeID = 0, Code = GenericUserTypeTextCode, Type = GenericUserTypeTextType, Description = "Default Category. Project Personnel are automatically allocated this category when assigned to the project" });

            ViewBag.InfoMessage = TempData["message"];
            return View(userTypes);
        }

        public ActionResult CreateUserType()
        {
            UserTypeCreateViewModel model = new UserTypeCreateViewModel
            {
                Code = null
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult CreateUserType(UserTypeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            List<UserType> allExistingUserTypes = Db.UserTypes.ToList();

            InfoMessage message;

            bool validNewName = !allExistingUserTypes.Select(x => x.Code).Contains(model.Code);

            if (!validNewName)
            {
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = "Name is already taken. Cannot create new user type." };
                ViewBag.InfoMessage = message;
                return View(model);
            }

            UserType userType = new UserType
            {
                Code = model.Code,
                Type = model.Type,
                Description = model.Description,
                IsEnabled = true
            };

            Db.UserTypes.Add(userType);
            Db.SaveChanges();

            message = new InfoMessage
            {
                MessageType = InfoMessageType.Success,MessageContent = "Successfully created new user type."
            };
    
            TempData["message"] = message;
            return RedirectToAction("Index");
        }

        public JsonResult GetUserType(int? id)
        {
            UserType userType = id == null || id == 0 ? new UserType {Code = GenericUserTypeTextCode, Type = GenericUserTypeTextType, Description = "Default Category. Project Personnel are automatically allocated this category when assigned to the project" } : Db.UserTypes.Find(id);
            return Json(new {Code = userType.Code, Type = userType.Type, Description = userType.Description});
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