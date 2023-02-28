﻿using TurboTaskEr.Data;
using TurboTaskEr.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace TurboTaskEr.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;
        public CommentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        // Adaugarea unui comentariu asociat unui task in baza de date
        /*[HttpPost]
        public IActionResult New(Comment comm)
        {
            comm.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Comments.Add(comm);
                db.SaveChanges();
                return Redirect("/Tasks/Show/" + comm.TaskId);
            }

            else
            {
                return Redirect("/Tasks/Show/" + comm.TaskId);
            }

        }*/

        [HttpPost]
        [Authorize(Roles ="User,Admin")]
        public IActionResult Delete(int id)
        {
            Comment comm = db.Comments.Find(id);

            if(comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comm);
                db.SaveChanges();
                return Redirect("/Tasks/Show/" + comm.TaskId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti comentariul";
                return RedirectToAction("Index","Tasks");
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(comm);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati comentariul";
                return RedirectToAction("Index", "Tasks");
            }
                
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comm = db.Comments.Find(id);

            if(ModelState.IsValid)
            {
                if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    comm.Content = requestComment.Content;
                    db.SaveChanges();
                    return Redirect("/Tasks/Show/" + comm.TaskId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa editati comentariul";
                    return RedirectToAction("Index", "Tasks");
                }
            }
            else
            {
                return View(requestComment);
            }
        }
    }
}
