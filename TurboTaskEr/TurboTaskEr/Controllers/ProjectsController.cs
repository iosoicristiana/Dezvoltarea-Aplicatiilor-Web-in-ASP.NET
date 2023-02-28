using TurboTaskEr.Data;
using TurboTaskEr.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Task = TurboTaskEr.Models.Task;

namespace TurboTaskEr.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;
        public ProjectsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //afisez proiectele 
        // httpget implicit

        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            ViewBag.EsteAdmin = User.IsInRole("Admin");

            if (ViewBag.EsteAdmin)
            {

                var projects = db.Projects.Include("User");
                ViewBag.Projects = projects;



                var search = "";

                // MOTOR DE CAUTARE

                if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
                {
                    search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere 

                    // Cautare in articol (Title si Content)

                    List<int> projectIds = db.Projects.Where
                                            (
                                             at => at.Title.Contains(search)
                                             || at.Description.Contains(search)
                                            ).Select(a => a.Id).ToList();


                    // Lista articolelor care contin cuvantul cautat
                    // fie in articol -> Title si Content
                    // fie in comentarii -> Content
                    projects = db.Projects.Where(project => projectIds.Contains(project.Id))
                                          .Include("User")
                                          .OrderBy(a => a.Date);

                }

                ViewBag.SearchString = search;

                // AFISARE PAGINATA

                // Alegem sa afisam 3 articole pe pagina
                int _perPage = 3;

                if (TempData.ContainsKey("message"))
                {
                    ViewBag.message = TempData["message"].ToString();
                }


                // Fiind un numar variabil de articole, verificam de fiecare data utilizand 
                // metoda Count()

                int totalItems = projects.Count();


                // Se preia pagina curenta din View-ul asociat
                // Numarul paginii este valoarea parametrului page din ruta
                // /Articles/Index?page=valoare

                var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

                // Pentru prima pagina offsetul o sa fie zero
                // Pentru pagina 2 o sa fie 3 
                // Asadar offsetul este egal cu numarul de articole care au fost deja afisate pe paginile anterioare
                var offset = 0;

                // Se calculeaza offsetul in functie de numarul paginii la care suntem
                if (!currentPage.Equals(0))
                {
                    offset = (currentPage - 1) * _perPage;
                }

                // Se preiau articolele corespunzatoare pentru fiecare pagina la care ne aflam 
                // in functie de offset
                var paginatedProjects = projects.Skip(offset).Take(_perPage);


                // Preluam numarul ultimei pagini

                ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

                // Trimitem articolele cu ajutorul unui ViewBag catre View-ul corespunzator
                ViewBag.Projects = paginatedProjects;

                if (search != "")
                {
                    ViewBag.PaginationBaseUrl = "/Projects/Index/?search=" + search + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Projects/Index/?page";
                }


                if (TempData.ContainsKey("message"))
                {
                    ViewBag.Message = TempData["message"];
                }
                return View();
            }


            else 
            {
                var userID = _userManager.GetUserId(User);
                var projects = db.Teams.Where(proj => proj.UserId == userID)
                                       .Select(x => x.ProjectId);

                List<int?> projects_ids = projects.ToList();

                var proj = db.Projects.Include("User").Where(proj => projects_ids.Contains(proj.Id));

                ViewBag.Projects = proj;

                if (TempData.ContainsKey("message"))
                {
                    ViewBag.Message = TempData["message"];
                }

                return View();
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            Project project = db.Projects.Include("Tasks.User").Include("User")
                              .Where(x => x.Id == id)
                              .First();

            SetAccessRights();

            return View(project);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show([FromForm] Task task)
        {
            task.Date = DateTime.Now;
            task.UserId = _userManager.GetUserId(User);

            Project project = db.Projects.Include("Tasks").Include("User")
                              .First();

            if (ModelState.IsValid)
            {
                if (project.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    db.Tasks.Add(task);
                    db.SaveChanges();
                    return Redirect("/Projects/Show/" + task.ProjectId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa adaugati task unui proiect care nu va apartine";
                    return Redirect("/Projects/Show/" + project.Id);
                }
            }
            else
            {
                Project proj = db.Projects.Include("Tasks").Include("User")
                               .Where(proj => proj.Id == task.ProjectId)
                               .First();
               

                SetAccessRights();

                return View(proj);

            }
        }

        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            Project project = new Project();

            return View(project);
        }

        //linkuim proiectul de echipa
        [HttpPost]
        public async Task<IActionResult> NewAsync(Project project)
        {
            var user_id = _userManager.GetUserId(User);
            project.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                project.UserId = user_id;
                Team p = new Team();
                p.UserId = user_id;
                p.ProjectId = project.Id;
                p.Project = project;

                db.Teams.Add(p);

                await db.SaveChangesAsync();
                TempData["message"] = "Proiectul a fost adaugat";
                return Redirect("/Projects/Show/" + project.Id);
            }
            else
            {
                return View(project);
            }
        }
         
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Project project = db.Projects.Include("Tasks")
                              .Where(x => x.Id == id)
                              .First();

            if(project.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(project);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui proiect care nu va apartine";
                return Redirect("/Projects/Show/" + id);
            }
            
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Project requestProject)
        {
            Project project = db.Projects.Include("Tasks")
                              .Where(x => x.Id == id)
                              .First();

            try
            {
                if (ModelState.IsValid)
                {
                    if(project.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                    {
                        project.Title = requestProject.Title;
                        project.Description = requestProject.Description;
                        project.Deadline = requestProject.Deadline;
                        project.Date = requestProject.Date;
                        db.SaveChanges();
                        TempData["message"] = "Proiectul a fost modificat";
                        return Redirect("/Projects/Show/" + id);
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui proiect care nu va apartine";
                        return Redirect("/Projects/Show/" + id);
                    }
                    
                }
                else return View(requestProject);
            }

            catch (Exception e)
            {
                return View(requestProject);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            Project project= db.Projects.Find(id);

            if(project.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Projects.Remove(project);
                db.SaveChanges();
                TempData["message"] = "Proiectul a fost sters";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui proiect care nu va apartine";
                return Redirect("/Projects/Show/" + id);
            }
        }



        [Authorize(Roles = "User,Admin")]
        public IActionResult Users(int id)
        {
            var userid = _userManager.GetUserId(User);
            var users = db.Teams.Include("User").Where(user => user.ProjectId == id);
            List<string?> users_ids = users.Select(c => c.UserId).ToList();
            if (users_ids.Contains(userid) || User.IsInRole("Admin"))
            {

                var users_search = db.Users.Where(a => 1 == 0);
                var search = "";
                // MOTOR DE CAUTARE
                if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
                {
                    search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
                    users_search = db.Users.Where(usn => usn.UserName.Contains(search) &&
                                                        !users_ids.Contains(usn.Id))
                                            .OrderBy(a => a.UserName);
                }

                var project = db.Projects.Find(id);
                ViewBag.Users = users;
                ViewBag.Project = project;
                ViewBag.AllUsers = users_search;
                return View();
            }
            else
            {
                TempData["message"] = "Error! Nu ai acces";
                ViewBag.Message = TempData["message"];
                return RedirectToAction("Index");
            }
        }


        
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Users(int id, [FromForm] Team requestUser)
        {
            var userid = _userManager.GetUserId(User);
            var project = db.Projects.Find(id);
            if (project == null)
            {
                TempData["message"] = "Database error!";
                return RedirectToAction("Index");
            }
            else
            {
                if (userid == project.UserId || User.IsInRole("Admin"))
                {
                    Team team= new Team();
                    team.ProjectId = id;
                    team.UserId = requestUser.UserId;
                    db.Teams.Add(team);
                    db.SaveChanges();
                    return Redirect("/Projects/Users/" + project.Id);
                }
                else
                {
                    TempData["message"] = "Error! Nu ai acces";
                    ViewBag.Message = TempData["message"];
                    return RedirectToAction("Index");
                }
            }
        }

       
        
    }
}