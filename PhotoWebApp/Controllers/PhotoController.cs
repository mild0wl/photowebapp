using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoWebApp.Data;
using PhotoWebApp.Models;
using System.ComponentModel.Design;
using System.Security.Claims;

namespace PhotoWebApp.Controllers
{
    public class PhotoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<CommentController> _logger;
        public PhotoController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, ILogger<CommentController> logger) { 
            _db = db;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        }
        /*
        public async Task<IActionResult> IndexAsync()
        {
            // List<Photo> photoList = _db.Photo.ToList();
            //return View(photoList);
        }*/

        [Authorize(Roles = "ContentCreator")]
        // GET Create : Calling the Create.cshtml
        public IActionResult Create() {

            return View();
        }

        [Authorize(Roles = "ContentCreator")]
        // HTTP POST Create
        [HttpPost]
        public async Task<IActionResult> Create(Photo obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Uploading photo....");

                    string guidFileName = null;

                    string[]? allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                    // Get the file extension
                    string fileExtension = Path.GetExtension(obj.PhotoFile.FileName).ToLowerInvariant();

                    // Check if the file extension is allowed
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        _logger.LogInformation("Uploading photo: Invalid file type");
                        ModelState.AddModelError("PhotoFile", "Invalid file type. Please upload an image file (jpg, jpeg, png, gif).");
                        return View(obj);  // Return the view with error message
                    }

                    // tagging userId of the user uploaded the file
                    var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    Users? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                    if (user != null)
                    {
                        obj.userId = user.UserId;
                    }
                    else
                    {
                        _logger.LogInformation("Uploading photo: Invalid user.");
                        ModelState.AddModelError("PhotoFile", $"User ID not assigned, ERROR! UserId: {userEmail}");
                        return View(obj);
                    }

                    long maxFileSizeInBytes = 1 * 1024 * 1024;  // 1 MB max size
                    if (obj.PhotoFile.Length > maxFileSizeInBytes)
                    {
                        _logger.LogInformation("Uploading photo: file size exceeds the maximum allowed size of 1 MB.");

                        ModelState.AddModelError("PhotoFile", "The file size exceeds the maximum allowed size of 1 MB.");
                        return View(obj);  // Return the view with an error message
                    }


                    if (obj.PhotoFile != null)
                    {
                        _logger.LogInformation("Uploading photo.");
                        var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder);
                        }

                        _logger.LogInformation("Uploading photo: creating filename");
                        guidFileName = Guid.NewGuid().ToString() + "_" + obj.PhotoFile.FileName;
                        var filePath = Path.Combine(uploadFolder, guidFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            _logger.LogInformation("Uploading photo: CopyToAsync");
                            await obj.PhotoFile.CopyToAsync(fileStream);
                        }

                        obj.FilePath = "/uploads/" + guidFileName;

                    }

                    obj.DatePosted = DateTime.Now;
                    _db.Photo.Add(obj);
                    await _db.SaveChangesAsync();

                    _logger.LogInformation("Uploading photo: Photo uploaded successfully");

                    TempData["Message"] = "Photo uploaded successfully!.";
                    TempData["IsSuccess"] = true;
                    return RedirectToAction("Index", "Home");
                }
                return View(obj);
            }
            catch (Exception ex) {

                _logger.LogInformation($"Exception while uploading the photo: {ex}");

                TempData["Message"] = "Exception while uploading the photo.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Error", "Auth");
            }
        }

        [Authorize(Roles = "ContentCreator")]
        // GET Edit
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                Photo? photoFromDb = _db.Photo.Find(id);

                if (id == null || id == 0 || photoFromDb == null)
                {
                    _logger.LogInformation("Edit photo: Photo not found");

                    // Error 404 will show the prompt Photo not found
                    TempData["Message"] = "Photo not found!";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", "Home");
                }

                // searching in DB for the userId based on the userEmail in the JWT claims
                var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Users? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                // checks who owns the uploaded photo
                if (photoFromDb.userId == user.UserId)
                {
                    return View(photoFromDb);
                }

                // If the above conditions doesnot match the unauthorized is trying to access the edit page
                // Redirects to AccessDenied page
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception on edit photo.");

                TempData["Message"] = "Exception to edit the photo.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Error", "Auth");
            }
        }

        [Authorize(Roles = "ContentCreator")]
        // Post HTTP Edit
        [HttpPost]
        public async Task<IActionResult> Edit(int? id, Photo obj)
        {

            try
            {
                _logger.LogInformation("Edit photo.....");

                Photo? photoFromDb = await _db.Photo.AsNoTracking().FirstOrDefaultAsync(p => p.photoId == id);

                if (id == null || id == 0 || photoFromDb == null)
                {
                    _logger.LogInformation("Edit photo: Photo not found!");
                    ModelState.AddModelError("photoId", "Photo not found!");
                    return View(obj);  // Return the view with an error message
                }

                // searching in DB for the userId based on the userEmail in the JWT claims
                var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Users? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (ModelState.IsValid && photoFromDb.userId == user.UserId)
                {
                    string guidFileName = null;

                    obj.userId = user.UserId;

                    string[]? allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                    // Get the file extension
                    string fileExtension = Path.GetExtension(obj.PhotoFile.FileName).ToLowerInvariant();

                    // Check if the file extension is allowed
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        _logger.LogInformation("Edit photo: Invalid file type!");

                        ModelState.AddModelError("PhotoFile", "Invalid file type. Please upload an image file (jpg, jpeg, png, gif).");
                        return View(obj);  // Return the view with error message
                    }

                    long maxFileSizeInBytes = 1 * 1024 * 1024;  // 1 MB max size
                    if (obj.PhotoFile.Length > maxFileSizeInBytes)
                    {
                        _logger.LogInformation("Edit photo: The file size exceeds the maximum allowed size of 1 MB.");

                        ModelState.AddModelError("PhotoFile", "The file size exceeds the maximum allowed size of 1 MB.");
                        return View(obj);  // Return the view with an error message
                    }

                    if (obj.PhotoFile != null)
                    {

                        var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder);
                        }
                        _logger.LogInformation("Edit photo: file name creation.");

                        //random file name creation
                        guidFileName = Guid.NewGuid().ToString() + "_" + obj.PhotoFile.FileName;
                        var filePath = Path.Combine(uploadFolder, guidFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await obj.PhotoFile.CopyToAsync(fileStream);
                        }

                        obj.FilePath = "/uploads/" + guidFileName;

                    }

                    obj.DatePosted = DateTime.Now;
                    _db.Photo.Update(obj);
                    await _db.SaveChangesAsync();

                    _logger.LogInformation("Edit photo: Photo modified successfully.");


                    TempData["Message"] = "Photo modified successfully!";
                    TempData["IsSuccess"] = true;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return Forbid();
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"An error occurred while editing the photo with ID PhotoId {id}");
            }
            return Forbid();
        }

        [Authorize(Roles = "Admin,ContentCreator")]
        public async Task<IActionResult> Delete(int? id)
        {

            try
            {
                // matching the ID from DB
                Photo? photo = _db.Photo.Find(id);

                _logger.LogInformation("Delete photo!");

                // searching in DB for the userId based on the userEmail in the JWT claims
                var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Users? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (id == null || id == 0 || photo == null)
                {
                    _logger.LogInformation("Delete photo: Photo not found.");

                    // if the file is not found, instead of 404, it will be redirected to Home page with error shown
                    TempData["Message"] = "Photo not found.";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", "Home");
                }
                else if (photo.userId == user.UserId || User.IsInRole("Admin"))
                {
                    // Deleting the Comment
                    _db.Photo.Remove(photo);
                    _db.SaveChanges();

                    // delete file from server: not working as expected will make changes later
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", photo.FilePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    _logger.LogInformation("Delete photo: Photo deleted successfully.");

                    TempData["Message"] = "Photo deleted successfully!.";
                    TempData["IsSuccess"] = true;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // if an unauthorized user is performing an action of Delete will be redirected to AccessDenied page
                    return Forbid();
                }
            }
            catch (Exception ex) {
                _logger.LogInformation($"Error occurred while deleting the photo: {ex}");

                TempData["Message"] = "Error occurred while deleting the photo.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ViewPhotoAsync(int? id) {

            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Users? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                _logger.LogInformation("View photo..");

                if (user != null)
                {
                    ViewBag.UserId = user.UserId;
                }
                else
                {
                    ViewBag.UserId = null;
                }

                if (id == null || id == 0)
                {
                    _logger.LogInformation("View photo: Photo not found");

                    TempData["Message"] = "Photo not found";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", "Home");
                }

                // matching the ID from DB
                var photo = _db.Photo
                   .Include(p => p.Comment)  // loading comments
                   .Where(p => p.User != null)
                   .FirstOrDefault(p => p.photoId == id);

                if (photo == null)
                {

                    _logger.LogInformation("View photo: Photo not found");

                    TempData["Message"] = "Photo not found";
                    TempData["IsSuccess"] = false;
                    return RedirectToAction("Index", "Home");
                }
                return View(photo);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error occurred while viewing the photo: {ex}");

                TempData["Message"] = "Error occurred while viewing the photo.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Error", "Auth");
            }
        }

        public IActionResult Like(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }

                // matching the ID from DB
                Photo? photo = _db.Photo.Find(id);
                if (photo == null)
                {
                    return NotFound();
                }
                else
                {
                    photo.LikesCount++;
                    // Deleting the Comment
                    _db.Photo.Update(photo);
                    _db.SaveChanges();
                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception on like photo action: {ex}");

                TempData["Message"] = $"Exception on like photo {id}";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Error", "Auth");
            }
        }
    }
}
