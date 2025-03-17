using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PhotoWebApp.Data;
using PhotoWebApp.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace PhotoWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _db = db;
            _logger = logger;
        }

        public IActionResult Index()
        {

            try
            {
                string userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Users? user = _db.Users.FirstOrDefault(u => u.Email == userEmail);

                if (user == null)
                {
                    Response.Cookies.Delete("JwtToken");
                }

                return View();
            }
            catch (Exception ex) {
                _logger.LogInformation($"Exception on home dir: {ex}");
                TempData["Message"] = "Exception on home page, contact Admin.";
                TempData["IsSuccess"] = false;

                return RedirectToAction("Index", "Auth");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
