using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Models;

namespace WeddingPlanner.Controllers
{

    public class HomeController : Controller
    {
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        // SESSION
        private int? UserSession
        {
            get { return HttpContext.Session.GetInt32("UserId"); }
            set { HttpContext.Session.SetInt32("UserId", (int)value); }
        }
        public IActionResult Index()
        {
            if (UserSession != null)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }
        [HttpPost("/new/Users")]
        public IActionResult Registration(User newUser)
        {
            if (ModelState.IsValid)
            {
                // if email provided is NOT in database create new account
                if (dbContext.Users.FirstOrDefault(user => user.Email == newUser.Email) == null)
                {
                    PasswordHasher<User> hasher = new PasswordHasher<User>();
                    newUser.Password = hasher.HashPassword(newUser, newUser.Password);
                    dbContext.Add(newUser);
                    dbContext.SaveChanges();
                    UserSession = newUser.UserId;

                    return RedirectToAction("Dashboard");

                }
                else
                {
                    // if it already exist say to log in
                    ModelState.AddModelError("email", "Please log in");
                    return View("Index");
                }
            }
            return View("Index");
        }

        [HttpPost("User/login")]
        public IActionResult Login(Login userinfo)
        {
            if (ModelState.IsValid)
            {
                User userFromDb = dbContext.Users.FirstOrDefault(user => user.Email == userinfo.LoginEmail);
                if (userFromDb == null)
                {
                    // Create error if email not in db
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }
                var hasher = new PasswordHasher<Login>();
                var result = hasher.VerifyHashedPassword(userinfo, userFromDb.Password, userinfo.LoginPassword);

                if (result == 0)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid cradentails");
                    return View("Index");

                }
                UserSession = userFromDb.UserId;
                return RedirectToAction("Dashboard");
            }
            return View("Index");
        }

        [HttpGet("/dashboard")]
        public IActionResult Dashboard()
        {
            if (UserSession == null)
                return RedirectToAction("Index");
            // User currentUser = dbContext.Users.Include(user => user.WeddingsCreated).ThenInclude(user => user.Associations).FirstOrDefault(user => user.UserId == userId);
            // if (UserSession != userId)
            // {
            //     return RedirectToAction("Dashboard", new { userId = UserSession });
            // }
            ViewBag.userId = UserSession;
            List<Wedding> AllWeddings = dbContext.Weddings
                .Include(w => w.Associations)
                .OrderByDescending(w => w.Date)
                .ToList();
            return View(AllWeddings);
        }

        [HttpGet("Wedding/New")]
        public IActionResult NewWeddingPage()
        {
            return View();
        }
        [HttpPost("Wedding/New")]
        public IActionResult CreateWedding(Wedding newWedding)
        {
            if (UserSession == null)
                return RedirectToAction("Index");
            if (ModelState.IsValid)
            {
                // Crete new wedding with UserId set to current session user's id
                newWedding.UserId = (int)UserSession;
                dbContext.Weddings.Add(newWedding);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("NewWeddingPage");
            }
        }

        [HttpGet("Wedding/Delete/{wId}")]
        public IActionResult Delete(int wId)
        {
            Wedding wedtoDel = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == wId);
            dbContext.Weddings.Remove(wedtoDel);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("Wedding/RSVP/{wId}")]
        public IActionResult RSVP(int wId)
        {
            if (UserSession == null)
                return RedirectToAction("Index");
            Association going = new Association();
            going.UserId = (int)UserSession;
            going.WeddingId = wId;
            dbContext.Associations.Add(going);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("Wedding/NVM/not-going/{wId}")]
        public IActionResult UnRSVP(int wId)
        {
            if (UserSession == null)
                return RedirectToAction("Index");
            Association NvmNotGoing = dbContext.Associations.FirstOrDefault(asso => asso.WeddingId == wId && asso.UserId == UserSession);
            dbContext.Associations.Remove(NvmNotGoing);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpGet("Wedding/WeddingInfo/{wId}")]
        public IActionResult WeddingInfo(int wId)
        {
            var currentWed = dbContext.Weddings.Include(w => w.Associations).ThenInclude(w => w.Guest).FirstOrDefault(w => w.WeddingId == wId);
            return View(currentWed);
        }

        [HttpGet("/logout")]
        public IActionResult logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
