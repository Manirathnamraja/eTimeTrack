using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using eTimeTrack.Helpers;
using eTimeTrack.Models;

namespace eTimeTrack.Controllers
{
    [Authorize(Roles = UserHelpers.AuthTextAdminOrAbove)]
    public class OfficesController : BaseController
    {
        public ActionResult Index()
        {
            var offices = Db.Offices.Include(o => o.Company);
            return View(offices.ToList());
        }

        public ActionResult Create()
        {
            ViewBag.Company_Id = new SelectList(Db.Companies, "Company_Id", "Company_Code");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Office_Id,Office_Code,Office_Name,Company_Id")] Office office)
        {
            if (ModelState.IsValid)
            {
                Db.Offices.Add(office);
                Db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Company_Id = new SelectList(Db.Companies, "Company_Id", "Company_Code", office.Company_Id);
            return View(office);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return InvokeHttp400(HttpContext);
            }
            Office office = Db.Offices.Find(id);
            if (office == null)
            {
                return InvokeHttp404(HttpContext);
            }
            ViewBag.Company_Id = new SelectList(Db.Companies, "Company_Id", "Company_Code", office.Company_Id);
            return View(office);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Office_Id,Office_Code,Office_Name,Company_Id")] Office office)
        {
            if (ModelState.IsValid)
            {
                Db.Entry(office).State = EntityState.Modified;
                Db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Company_Id = new SelectList(Db.Companies, "Company_Id", "Company_Code", office.Company_Id);
            return View(office);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Office office = Db.Offices.Find(id);
            Db.Offices.Remove(office);
            Db.SaveChanges();
            return RedirectToAction("Index");
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
