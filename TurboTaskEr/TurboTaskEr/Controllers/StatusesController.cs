using TurboTaskEr.Data;
using TurboTaskEr.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace TurboTaskEr.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatusesController : Controller
    {
        private readonly ApplicationDbContext db;
        public StatusesController(ApplicationDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            if(TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var statuses = from status in db.Statuses
                             orderby status.StatusName
                             select status;

            ViewBag.Statuses = statuses;
            return View();
        }

        public ActionResult Show(int id)
        {
            Status status = db.Statuses.Find(id);
            return View(status);
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(Status stat)
        {
            if(ModelState.IsValid)
            {
                db.Statuses.Add(stat);
                db.SaveChanges();
                TempData["message"] = "Statusul a fost adaugat";
                return RedirectToAction("Index");
            }
            else
            {
                return View(stat);
            }
        }

        public ActionResult Edit(int id)
        {
            Status status = db.Statuses.Find(id);
            return View(status);

        }

        [HttpPost]
        public ActionResult Edit(int id, Status requestStatus)
        {

            Status status = db.Statuses.Find(id);

            try
            {
                if (ModelState.IsValid)
                {
                    status.StatusName = requestStatus.StatusName;
                    db.SaveChanges();
                    TempData["message"] = "Statusul a fost modificat";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(requestStatus);
                }
            }

            catch (Exception e)
            {
                return View(requestStatus);
            }
            

        }

        [HttpPost]
        public ActionResult Delete(int id)
            {
            Status status = db.Statuses.Find(id);
            db.Statuses.Remove(status);
            TempData["message"] = "Statusul a fost sters";
            db.SaveChanges();
            return RedirectToAction("Index");
            }

    }
}
