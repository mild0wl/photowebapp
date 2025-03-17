using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoWebApp.Data;
using PhotoWebApp.Models;

namespace PhotoWebApp.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _db;
        // static logger
        private static readonly ILogger<AuthController> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AuthController>();
        public CommentController(ApplicationDbContext db) { 

            _db = db; 
        }

        [Authorize(Roles = "Admin,ContentCreator")]
        public IActionResult Index()
        {
            try
            {
                _logger.LogInformation("List of comments logging.");
                List<Comment> commentList = _db.Comment.ToList();
                return View(commentList);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("List of comments error.");
                return View(ex);
            }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int? id)
        {
            try
            {
                // matching the ID from DB
                Comment? comment = _db.Comment.Find(id);

                // check if the ID is null or zero
                if (id == null || id == 0 || comment == null)
                {
                    // Error handling of comment returning 404 when Comment not found
                    _logger.LogInformation($"Deleteing comment failed: Comment not found.");
                    ViewBag.Message = "Comment is Not found!";
                    ViewBag.isSucess = false;
                    _logger.LogInformation($"Comment Not Found on Delete.");

                    return View("Index", GetComments());
                }
                else
                {
                    ViewBag.Message = "Comment is deleted sucessfully!";
                    ViewBag.isSucess = true;
                    // Deleting the Comment
                    _db.Comment.Remove(comment);
                    _db.SaveChanges();
                    _logger.LogInformation($"Comment delete sucessfully: {comment}.");

                    return View("Index", GetComments());
                }
            }
            catch (Exception ex) {
                ViewBag.Message = "Error on comment delete.";
                ViewBag.isSucess = false;
                _logger.LogInformation($"Error on Comment delete: {ex.Message}");
                return View("Index", GetComments());
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostComment(int photoId, string commentValue) {

            if (string.IsNullOrWhiteSpace(commentValue))
            {
                _logger.LogInformation("Comment is Null.");
                return RedirectToAction("ViewPhoto", "Photo", new { id = photoId });
            }

            Comment comment = new Comment
            {
                PhotoId = photoId,
                commentValue = commentValue,
                DatePosted = DateTime.Now,
            };

            _db.Comment.Add(comment);
            _db.SaveChanges();

            _logger.LogInformation($"Comment posted: {commentValue}");

            TempData["Message"] = "Comment posted succesfully.";
            
            TempData["IsSuccess"] = true;
            return RedirectToAction("ViewPhoto", "Photo", new {id = photoId});
        }

        [Authorize(Roles = "ContentCreator")]
        public IActionResult Flag(int id) 
        {
            // matching the ID from DB
            Comment? comment = _db.Comment.Find(id);

            // check if the ID is null or zero
            if (id == null || id == 0 || comment == null)
            {
                ViewBag.Message = "Comment is Not found!";
                ViewBag.isSucess = false;
                _logger.LogInformation("Comment is not found.");

                return View("Index", GetComments());
            }
            else if (comment.Flagged)
            {
                ViewBag.Message = "Comment is already Flagged!";
                ViewBag.isSucess = false;
                _logger.LogInformation($"Comment flagged already: {comment}");

                return View("Index", GetComments());
            }
            else
            {
                ViewBag.Message = "Comment successfully flagged!";
                ViewBag.isSuccess = true;
                // Deleting the Comment
                comment.Flagged = true;
                _db.SaveChanges();

                _logger.LogInformation($"Flagged {comment.Flagged}");
                return View("Index", GetComments());
            }
        }

        private List<Comment> GetComments()
        {
            _logger.LogInformation($"List of comment being called.");
            return _db.Comment.ToList();
        }
    }
}
