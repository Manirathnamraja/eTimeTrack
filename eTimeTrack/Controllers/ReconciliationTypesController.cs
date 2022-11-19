using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;
using eTimeTrack.ViewModels;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.RoleSuperUser)]
    public class ReconciliationTypesController : BaseController
    {
        public ActionResult Index()
        {
            List<ReconciliationType> reconciliationTypes = Db.ReconciliationTypes.ToList();
            ReconciliationTypesIndexViewModel vm = new ReconciliationTypesIndexViewModel {ReconciliationTypes = reconciliationTypes };
            return View(vm);
        }

        public ActionResult CreateReconciliationType()
        {
            ReconciliationTypeCreateViewModel model = new ReconciliationTypeCreateViewModel
            {
                Text = null
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult CreateReconciliationType(ReconciliationTypeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            List<ReconciliationType> allExistingReconciliationTypes = Db.ReconciliationTypes.ToList();

            InfoMessage message;

            bool validNewText = !allExistingReconciliationTypes.Select(x => x.Text).Contains(model.Text);

            if (!validNewText)
            {
                message = new InfoMessage { MessageType = InfoMessageType.Failure, MessageContent = "Text is already taken. Cannot create new reconciliation type." };
                ViewBag.InfoMessage = message;
                return View(model);
            }

            ReconciliationType reconciliationType = new ReconciliationType
            {
                Text = model.Text,
                Description = model.Description
            };

            Db.ReconciliationTypes.Add(reconciliationType);
            Db.SaveChanges();

            message = new InfoMessage
            {
                MessageType = InfoMessageType.Success,
                MessageContent = "Successfully created new reconciliation type."
            };

            TempData["message"] = message;
            return RedirectToAction("Index");
        }
    }
}