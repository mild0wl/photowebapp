using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PhotoWebApp.Controllers
{
    public class LogController : Controller
    {
        private readonly IWebHostEnvironment _env;

        // static logger
        private static readonly ILogger<AuthController> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AuthController>();

        public LogController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var LogPath = Path.Combine(_env.ContentRootPath, "Logs", $"app-{DateTime.Now:yyyyMMdd}.log");
            try
            {
                if (System.IO.File.Exists(LogPath))
                {
                    /* var logCntxt = System.IO.File.ReadAllText(LogPath);
                     return View(model: logCntxt);*/
                    using (var fileStream = new FileStream(LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(fileStream))
                    {
                        var logContent = reader.ReadToEnd();
                        TempData["DebugLogContent"] = logContent.Substring(0, Math.Min(1000, logContent.Length));
                        return View(model: logContent);
                    }
                }
            }
            catch (Exception ex) 
            {
                _logger.LogInformation($"Log file not found! Exception: {ex}");

                TempData["Message"] = $"Log file not found! Exception: {ex}";
                TempData["IsSuccess"] = false;

                Response.Cookies.Delete("JwtToken");
                return RedirectToAction("Index", "Home");
            }

            // if it fails, it returns null model
            return View(model: null);
        }
    }
}
