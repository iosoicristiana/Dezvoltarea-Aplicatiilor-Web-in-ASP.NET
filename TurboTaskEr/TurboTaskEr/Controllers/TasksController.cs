using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System.Threading.Tasks;
using TurboTaskEr.Data;
using TurboTaskEr.Models;
using Task = TurboTaskEr.Models.Task;

namespace TurboTaskEr.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;
        public TasksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private void SetAccessRights()
        {
            Task task = db.Tasks.Include("Project")
                                .Include("Status")
                                .Include("User")
                                .Include("Comments")
                                .Include("Comments.User")
                                .First();

            Project proj = task.Project;

            ViewBag.AfisareButoane = false;

            if (task.UserId == _userManager.GetUserId(User))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.EsteOrganizator = proj.UserId;
        }

        // Se afiseaza lista tuturor taskurilor din baza de date 
        // HttpGet implicit

        [Authorize(Roles ="User,Admin")]
        public IActionResult Index()
        {
            var tasks = db.Tasks.Include("Status");

            ViewBag.Tasks = tasks;

            ViewBag.UserCurent = _userManager.GetUserId(User);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            //SetAccessRights();

            return View();
        }

        ////se afiseaza un articol dupa id
        ////sau dupa status
        ////+ comentarii

        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            Task task = db.Tasks.Include("Status")
                                .Include("User")
                                .Include("Comments")
                                .Include("Comments.User")
                                .Where(x => x.Id == id)
                                .First();

            task.Stat = GetAllStatuses();

            ViewBag.Stats = GetAllStatuses();

            /*ChangeStatus(id, task.Status);*/

            SetAccessRights();

            return View(task);
        }

       [Authorize(Roles = "User,Admin")]
        public IActionResult New(int id)
        {
            ViewBag.ProjectId = id;

            Task task = new Task();

            task.Stat = GetAllStatuses();

            task.UserInTeam = GetAllUsers(id);

            return View(task);
        }
        //adaugam si in baza de date

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New(Task task)
        {
            task.Date = DateTime.Now;
            //task.UserId = _userManager.GetUserId(User);

            var UserCurent = _userManager.GetUserId(User);

            Project project = db.Projects.Include("Tasks").Include("User")
                              .Where(proj => task.ProjectId == proj.Id)
                              .First();

            if (ModelState.IsValid)
            {
                if (project.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    Task task2 = new Task();
                    task2.StatusId = task.StatusId;
                    task2.Deadline = task.Deadline;
                    task2.ProjectId = task.ProjectId;
                    task2.Title = task.Title;
                    task2.Description = task.Description;
                    task2.UserId = task.UserId;
                    task2.Date = task.Date;
                    db.Tasks.Add(task2);
                    db.SaveChanges();
                    TempData["message"] = "Task-ul a fost adaugat";
                    return Redirect("/Projects/Show/" + task2.ProjectId);
                }

                else
                {
                    TempData["message"] = "Nu aveti dreptul sa adaugati task unui proiect care nu va apartine";
                    return Redirect("/Projects/Show/" + task.ProjectId);
                }
            }           
            else
            {
                task.Stat = GetAllStatuses();
                task.UserInTeam = GetAllUsers(task.ProjectId);
                return View(task);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Task task = db.Tasks.Include("Project").Include("Status").Include("User")
                                .Where(x => x.Id == id)
                                .First();

            SetAccessRights();

            task.Stat = GetAllStatuses();

            task.UserInTeam = GetAllUsers(task.ProjectId);

            /*Project proj = task.Project;*/


            Project proj = db.Projects.Include("Tasks").Include("User")
                              .Where(proj => task.ProjectId == proj.Id)
                              .First();

            if (proj.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(task);
            }
            else
            {
                TempData["message"] = "Nu aveti voie sa faceti modificari asupra unui task care nu va apartine";
                return Redirect("/Projects/Show/" + proj.Id);
            }
        }


        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Task requestTask)
        {
            Task task = db.Tasks.Find(id);

            /*Project proj = task.Project;*/


            Project proj = db.Projects.Include("Tasks").Include("User")
                              .Where(proj => task.ProjectId == proj.Id)
                              .First();


            if (ModelState.IsValid)
            {
                if (proj.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    //task.ProjectId = requestTask.ProjectId;
                    task.Title = requestTask.Title;
                    task.Description = requestTask.Description;
                    task.StatusId = requestTask.StatusId;
                    task.Deadline = requestTask.Deadline;
                    task.UserId = requestTask.UserId;
                    
                    TempData["message"] = "Task-ul a fost modificat";
                    db.SaveChanges();
                    return Redirect("/Projects/Show/" + task.ProjectId);
                }
                else
                {
                    TempData["message"] = "Nu aveti voie sa faceti modificari asupra unui task care nu va apartine";
                    return Redirect("/Projects/Show/" + proj.Id);
                }
            }
            else
            {
                requestTask.Stat = GetAllStatuses();
                requestTask.UserInTeam = GetAllUsers(requestTask.ProjectId);
                return View(requestTask);
            }
        }

        [HttpPost]
        [Authorize(Roles ="User,Admin")]
        public ActionResult Delete(int id)
        {
            Task task = db.Tasks.Include("Status")
                                .Where(x => x.Id == id)
                                .First();

            Project proj = db.Projects.Include("Tasks").Include("User")
                              .Where(proj => task.ProjectId == proj.Id)
                              .First();

            if (proj.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Tasks.Remove(task);
                db.SaveChanges();
                TempData["message"] = "Taskul a fost sters";
                return Redirect("/Projects/Show/" + task.ProjectId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un task care nu va apartine";
                return Redirect("/Projects/Show/" + task.Project);
            }
                
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllStatuses()
        {
            var selectList = new List<SelectListItem>();

            var statuses = from stat in db.Statuses
                           select stat;

            foreach(var status in statuses)
            {
                selectList.Add(new SelectListItem
                {
                    Value = status.Id.ToString(),
                    Text = status.StatusName.ToString()
                }); ;
            }

            return selectList;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllUsers(int? projId)
        {
            var selectList = new List<SelectListItem>();

            var UsersInTeam = db.Teams.Include("User")
                                  .Where(c => c.ProjectId == projId);


            foreach (var user in UsersInTeam)
            {
                selectList.Add(new SelectListItem
                {
                    Value = user.UserId.ToString(),
                    Text = user.User.UserName.ToString()
                }) ; 
            }

            return selectList;
        }

        [HttpPost]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);

            
            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Tasks/Show/" + comment.TaskId);

            }
            else
            {
                Task task = db.Tasks.Include("Status")
                                .Include("User")
                                .Include("Comments")
                                .Include("Comments.User")
                                .Where(x => x.Id == comment.TaskId)
                                .First();

                SetAccessRights();

                return View(task);
            }
        }

        [HttpPost]
        public IActionResult ChangeStatus(int id, int StatusId)
        {
            Task task = db.Tasks.Find(id);

            task.Stat = GetAllStatuses();

            SetAccessRights();

            //Status stat = db.Statuses.Find(task.Status);

            Project proj = db.Projects.Include("Tasks").Include("User")
                              .Where(proj => task.ProjectId == proj.Id)
                              .First();

            if (ModelState.IsValid)
            {
                if (proj.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin") || task.UserId == _userManager.GetUserId(User))
                {
                    //task.ProjectId = requestTask.ProjectId;
                    

                    task.Status = db.Statuses.Find(StatusId);
                    task.StatusId = StatusId;


                    /*task.StatusId = requestTask.StatusId;
                    task.Status.StatusName = requestTask.Status.StatusName;*/


                    TempData["message"] = "Status-ul a fost modificat";
                    db.SaveChanges();
                    //return View(requestTask);
                    return Redirect("/Tasks/Show/" + task.Id);
                }
                else
                {
                    TempData["message"] = "Nu aveti voie sa faceti modificari asupra unui task care nu va apartine";
                    return Redirect("/Tasks/Show/" + task.Id);
                    //return RedirectToAction("Index");
                    //return Redirect("/Tasks/Show/" + task.Id);
                }
            }
            else
            {
                task.Stat = GetAllStatuses();
                return Redirect("/Tasks/Show/" + task.Id);
            }
        }
    }
}