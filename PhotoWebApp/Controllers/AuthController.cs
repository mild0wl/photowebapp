using PhotoWebApp.Models;
using PhotoWebApp.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Security;
using System.Net;

namespace PhotoWebApp.Controllers
{
    public class AuthController : Controller
    {
        // static logger
        private static readonly ILogger<AuthController> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AuthController>();

        private readonly ApplicationDbContext _db;
        public AuthController(ApplicationDbContext db)
        {
            _db = db;
        }

        public ActionResult Index() 
        {
            return View(); 
        }

        // sending Email Token
        [HttpPost]
        public async Task<ActionResult> SendToken(string email)
        {
            Users? user = _db.Users.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                _logger.LogInformation("Email not found to send token on Login.");
                ViewBag.Message = "Email Not Found!";
                ViewBag.IsSuccess = false;
                return View("Index");
            }

            string token = GenerateToken();
            _logger.LogInformation("Token generated");
            user.Token = token;
            user.TokenExpiry = DateTime.UtcNow.AddMinutes(5);
            _db.Entry(user).State = EntityState.Modified;
            _db.SaveChanges();

            try
            {
                _logger.LogInformation($"Sending Token to {email}.");
                SendEmail(user.Email, token);
            }
            catch (Exception ex) {
                TempData["Message"] = "Error occurred!";
                _logger.LogInformation($"Sending Token to {email} failed.");
                TempData["IsSuccess"] = false;
                return View("Error");
            }

            // To Debug and view the token, add Injection token value here: {token}
            _logger.LogInformation($"Sending Token to {email} Success.");
            ViewBag.Message = $"Token sent to your Email!";
            ViewBag.IsSuccess = true;
            return View("Index");

        }

        // Generates Token
        private string GenerateToken() {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new SecureRandom();
            var token = new char[8];

            for (int i = 0; i < token.Length; i++)
            {
                token[i] = chars[random.Next(chars.Length)];
            }
            return new string(token);
        }

        // For unit-testing created an interface EmailSender
        public interface IEmailSender
        {
            Task SendEmailAsync(string email, string subject, string body);
        }

        // Send Email with Token
        private void SendEmail(string email, string token) {

            // calling SmtpClient class
            using var smtpClient = new SmtpClient();
            smtpClient.UseDefaultCredentials = false;

            // get the APP password from Google account and use your gmail to send tokens
            _logger.LogInformation($"EmailSender: {email}");
            smtpClient.Credentials = new NetworkCredential("AddYouEmailHere@gmail.com", "AddYourPasswdHere");
            smtpClient.Host = "smtp.gmail.com";
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;

            // message body
            //// Use your Gmail below
            var message = new MailMessage("AddYouEmailHere@gmail.com", email);
            message.Subject = "PhotoWebApp: Authentication Token";
            message.Body = $"Your Token to login is: {token}";
            smtpClient.Send(message);
        }

        [HttpPost]
        public ActionResult VerifyToken(string email, string token)
        {
            Users? user = _db.Users.FirstOrDefault(x => x.Email == email);

            if (user == null) 
            {
                _logger.LogInformation($"Verifying token failed: User Not Found!");
                ViewBag.Message = "Email Not Found!";
                ViewBag.IsSuccess = false;
                return View("Index");
            }

            if (token == null) {
                _logger.LogInformation($"Verifying token failed: Token is Empty!");
                ViewBag.Message = "Token is Empty";
                ViewBag.IsSuccess = false;
                return View("Index");
            }

            if (user.Token == token && user.TokenExpiry > DateTime.UtcNow) 
            {
                _logger.LogInformation($"Generating the JWT session for user: {user.Username}");

                
                string jwtToken = GenerateJwtToken(user);
                // checking for valid JWT token
                if (jwtToken.Split('.').Length == 3) {
                    user.Token = null;
                    user.TokenExpiry = null;
                    _db.Entry(user).State = EntityState.Modified;
                    _db.SaveChanges();

                    // save session on server-side
                    HttpContext.Session.SetString("JwtToken", jwtToken);

                    // create session on client-side
                    Response.Cookies.Append("JwtToken", jwtToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                    });

                    _logger.LogInformation($"Login successful for user {user.Email}");
                    TempData["Message"] = "Login successful!";
                    TempData["IsSuccess"] = true;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["Message"] = "Invalid JWT Token";
                    TempData["IsSuccess"] = false;
                    _logger.LogInformation($"Invalid JWT Token: {jwtToken}");
                    return RedirectToAction("Index");
                }
            }

            _logger.LogInformation($"Invalid or expired Token for user {user.Email}");
            ViewBag.Message = "Invalid or expired Token";
            ViewBag.IsSuccess = false;
            return View("Index");
        }

        public string GenerateJwtToken(Users? user) 
        {
            try
            {
                if (user == null)
                {
                    _logger.LogInformation($"Generating JWT token: User is null.");
                    return "User Model is empty.";
                }

                _logger.LogInformation($"Generating JWT token for user {user.Email}");
                SymmetricSecurityKey? key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Y0uC@n00tF1ndMyK3y!myk3y1s$tr0ng"));
                SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: "photowebapp.com",
                    audience: "photowebapp.com",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: creds
                );
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex) {
                _logger.LogInformation($"Exception while generating JWT token: {ex}");
            }

            _logger.LogInformation($"Generating JWT token failed.");
            return "Empty JWT Token";
        }

        [HttpGet]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("JwtToken");
            HttpContext.Session.Clear();
            _logger.LogInformation($"Logged out: {Environment.NewLine}");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
