using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoWebApp.Data;
using PhotoWebApp.Models;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using Azure.Identity;

namespace PhotoWebApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly AuthController _authController;

        // static logger
        private static readonly ILogger<AuthController> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AuthController>();
        public UsersController(ApplicationDbContext db)
        {
            _db = db;
            _authController = new AuthController(db);
        }

        [Authorize(Roles = "Admin,ContentCreator")]
        public async Task<IActionResult> Index()
        {
            try
            {
                // returns list of users
                _logger.LogInformation($"List of users page.");
                var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userEmail) || !User.Identity.IsAuthenticated)
                {
                    _logger.LogInformation($"User not Found {userEmail}");

                    TempData["Message"] = "Login please!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", "Auth");
                }

                List<Users> users = await _db.Users
                    .Where(u => u.Email != userEmail)
                    .ToListAsync();

                //List<Users> usersList = _db.Users.ToList();
                return View(users);
            }
            catch (Exception ex) {
                _logger.LogInformation($"Exception on returning list of users: {ex}");

                TempData["Message"] = "Error on returning list of users.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpGet]
        public IActionResult Register() { 
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Users newUser) {

            try
            {
                // check if user already existed
                _logger.LogInformation($"Register {newUser.Email}");
                var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == newUser.Email);

                if (existingUser != null)
                {
                    _logger.LogInformation($"{newUser.Email} already existed.");

                    ViewBag.Message = $"Users with that {newUser.Email} already existed!";
                    ViewBag.IsSuccess = false;
                    return View();
                }


                if (ModelState.IsValid)
                {
                    newUser.Role = UserRole.ContentCreator;
                    newUser.TokenExpiry = DateTime.Now;
                    newUser.Token = null;

                    _db.Users.Add(newUser);
                    await _db.SaveChangesAsync();

                    _logger.LogInformation($"User registered successfully: {newUser.Email}");
                    // Dont need of sending token after registering, user will be redirected to login page
                    //var result = await _authController.SendToken(newUser.Email);

                }
                TempData["Message"] = "User registered successfully!";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index", "Auth");
            }
            catch (Exception ex) { 
                _logger.LogInformation($"Error on registering user {newUser.Email}: {ex}");

                TempData["Message"] = "Exception on register page!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Error", "Auth");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult InviteUsers()
        {
            _logger.LogInformation("Calling invite users page");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> InviteUsersAsync(string? newEmail) {

            try
            {
                _logger.LogInformation($"Inviting user: {newEmail}");
                // check if user already existed
                var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == newEmail);

                if (existingUser != null)
                {
                    _logger.LogInformation($"User already existed: {newEmail}");

                    ViewBag.Message = $"Users with that {newEmail} already existed!";
                    ViewBag.IsSuccess = false;
                    return View();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _logger.LogInformation($"Sending invite email to user: {newEmail}");

                        SendEmail(newEmail);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"Exception on inviting user: {ex}");

                        TempData["Message"] = $"Error while inviting user: {newEmail}";
                        TempData["IsSuccess"] = false;
                        return RedirectToAction("Error", "Auth");
                    }
                    ViewBag.Message = $"Sent invitation email to {newEmail}";
                    ViewBag.IsSuccess = true;
                    return View();
                }

                return View();
            }
            catch (Exception e) {
                _logger.LogInformation($"Exception on inviting user: {e}");

                TempData["Message"] = "Error occurred while inviting user!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Error", "Auth");
            }
        }

        private void SendEmail(string email)
        {
            try
            {
                // calling SmtpClient class
                using var smtpClient = new SmtpClient();
                smtpClient.UseDefaultCredentials = false;
                _logger.LogInformation($"Initiating email object to invite user: {email}");

                // get the APP password from Google account and use your gmail to send tokens
                smtpClient.Credentials = new NetworkCredential("AddYourEmailHere@gmail.com", "AddYourGooglePasswdHere");
                smtpClient.Host = "smtp.gmail.com";
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;

                //var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // message body
                //// Use your Gmail below
                var message = new MailMessage("AddYourEmailHere@gmail.com", email);
                message.Subject = "PhotoWebApp: Invited you to join!";
                message.Body = $"Hi {email}\n\n I would like to welcome you to join PhotoWeb App and share the wonderful pictures you have clicked with us.\n\n Thankyou!";
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception while sending email: {ex}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewProfile()
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("Viewing profile.");


                if (userEmail == null)
                {
                    _logger.LogInformation("user is not found.");

                    TempData["Message"] = "Login please!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", "Auth");
                }
                else
                {
                    Users? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                    return View(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception on viewing the profile: {ex.Message}");

                TempData["Message"] = "Exception on viewing the profile";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Error", "Auth");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? Id)
        {
            try
            {
                _logger.LogInformation($"Viewing the user information to edit: {Id}");
                Users? user = _db.Users.Find(Id);

                if (Id == null || Id == 0 || user == null)
                {
                    _logger.LogInformation($"User not found to view edit");

                    TempData["Message"] = "Login please!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", "Auth");
                }

                // searching in DB for the userId based on the userEmail in the JWT claims
                var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (user.Email == userEmail)
                {
                    return View(user);
                }

                return RedirectToAction("Index", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception on edit the profile: {ex}");

                TempData["Message"] = "Exception on edit the profile";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Error", "Auth");

            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int? Id, Users user)
        {
            try
            {
                if (Id == null || Id == 0)
                {
                    _logger.LogInformation($"User not found to edit");

                    TempData["Message"] = "Login please!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", "Auth");
                }

                Users? userobj = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == Id);
                var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                string? _userRole = User.FindFirstValue(ClaimTypes.Role);

                if (userobj == null || userEmail == null)
                {
                    _logger.LogInformation($"User not found to edit");

                    TempData["Message"] = "Login please!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", "Auth");
                }

                if (userobj.Email == userEmail)
                {
                    // check if user already existed
                    var existingUser = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == user.Email);

                    if (existingUser != null)
                    {
                        _logger.LogInformation($"User existed to edit");

                        ViewBag.Message = $"Users with that {user.Email} already existed!";
                        ViewBag.IsSuccess = false;
                        return View();
                    }

                    Enum.TryParse(_userRole, out UserRole parsedRole);
                    user.Role = parsedRole;
                    _db.Users.Update(user);
                    await _db.SaveChangesAsync();

                    var newJWTToken = _authController.GenerateJwtToken(user);

                    // save session on server-side
                    HttpContext.Session.SetString("JwtToken", newJWTToken);

                    // create session on client-side
                    Response.Cookies.Append("JwtToken", newJWTToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                    });

                    _logger.LogInformation($"User information Updated successfully: {user.Email}");

                    TempData["Message"] = "Updated successfully!";
                    TempData["IsSuccess"] = true;
                    return RedirectToAction("ViewProfile", "Users");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex) {
                _logger.LogInformation($"Exception on edit the profile: {ex}");

                TempData["Message"] = "Exception on edit the profile";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Error", "Auth");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult EditUser(int? Id)
        {
            try
            {

                Users? user = _db.Users.Find(Id);

                if (Id == null || Id == 0 || user == null)
                {
                    _logger.LogInformation($"User does not exist: EditUser.");

                    TempData["Message"] = "User does not exist!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception on edit the user profile: {ex}");

                TempData["Message"] = "Exception to edit user profile";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Error", "Auth");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditUser(int? Id, Users? user)
        {
            try
            {
                Users? userInDb = await _db.Users.FirstOrDefaultAsync(u => u.UserId == Id);
                if (Id == null || Id == 0 || userInDb == null)
                {
                    _logger.LogInformation("User does not exist!");

                    TempData["Message"] = "User does not exist!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index");
                }

                // searching in DB for the userId based on the userEmail in the JWT claims
                var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                //grab the user obj using Email
                Users? sessionUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);


                if (sessionUser == null)
                {
                    _logger.LogInformation("User does not exist!");
                    TempData["Message"] = "Login please!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", "Auth");
                }

                if (!Enum.IsDefined(typeof(UserRole), user.Role) || user.Role == UserRole.Unknown)
                {
                    _logger.LogInformation("EditUser: Invalid role selected.");
                    ViewBag.Message = "Invalid role selected.";
                    ViewBag.IsSuccess = false;
                    return View(user);
                }


                if (sessionUser.Role == UserRole.Admin && userInDb.Role == UserRole.Admin)
                {
                    _logger.LogInformation("EditUser: cannot change another Admin Roles.");

                    ViewBag.Message = $"You cannot change another Admin Roles. Please contact {userInDb.Email}";
                    ViewBag.IsSuccess = false;
                    return View(userInDb);
                }
                _logger.LogInformation("EditUser: User Updated Successfully.");

                ViewBag.Message = "User Updated Successfully!";
                ViewBag.IsSuccess = true;

                userInDb.Role = user.Role;
                await _db.SaveChangesAsync();

                return View(userInDb);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception on edit the user profile: {ex}");

                TempData["Message"] = "Exception to edit user profile";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Error", "Auth");
            }
        }

        [Authorize(Roles = "Admin,ContentCreator")]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                Users? user = _db.Users.Find(id);

                if (id == null || id == 0 || user == null)
                {
                    _logger.LogInformation("Delete profile: User does not exist!");
                    TempData["Message"] = "User does not exist!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("ViewProfile", "Users");
                }

                // searching in DB for the userId based on the userEmail in the JWT claims
                var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Users? userSession = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                // matching the ID from DB

                if (userSession == null)
                {
                    _logger.LogInformation("Delete profile: Login please!");

                    TempData["Message"] = "Login please!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", "Auth");
                }
                else if (user.UserId == userSession.UserId)
                {
                    // Deleting the User
                    _db.Users.Remove(user);
                    _db.SaveChanges();

                    // remove cookies and redirect to homepage
                    Response.Cookies.Delete("JwtToken");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception on deleting the user profile: {ex}");

                TempData["Message"] = "Exception on deleting the user profile";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Error", "Auth");
            }
        }

        [Authorize(Roles = "Admin,ContentCreator")]
        public async Task<IActionResult> DeleteUser(int? Id)
        {
            try
            {
                // matching the ID from DB
                Users? user = _db.Users.Find(Id);

                if (Id == null || Id == 0 || user == null)
                {
                    _logger.LogInformation("Delete User: User does not exist!");

                    TempData["Message"] = "User does not exist!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", "Users");
                }

                // searching in DB for the userId based on the userEmail in the JWT claims
                var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Users? userSession = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (userSession == null)
                {
                    _logger.LogInformation("Delete User: Login please");

                    TempData["Message"] = "Login please!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", "Auth");
                }

                else if (user.Role == UserRole.Admin && userSession.Role == UserRole.Admin)
                {
                    _logger.LogInformation("Delete User: cannot DELETE another Admin account.");

                    ViewBag.Message = $"You cannot DELETE another Admin account. Please contact {user.Email}";
                    ViewBag.IsSuccess = false;
                    return View("EditUser", user);
                }
                else
                {
                    // Deleting the User
                    _db.Users.Remove(user);
                    await _db.SaveChangesAsync();
                }

                _logger.LogInformation("Delete User: Deleted user sucessfully!");

                TempData["Message"] = "Deleted user sucessfully!";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index", "Users");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception on edit the user profile: {ex}");

                TempData["Message"] = "Exception to edit user profile";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Error", "Auth");
            }
        }
    }
}
