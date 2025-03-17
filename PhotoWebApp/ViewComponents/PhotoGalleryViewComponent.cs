using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoWebApp.Data;
using PhotoWebApp.Models;
using System.Security.Claims;

namespace PhotoWebApp.ViewComponents
{
    public class PhotoGalleryViewComponent: ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public PhotoGalleryViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsPrincipal = User as ClaimsPrincipal;
            var userEmail = claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);
            Users? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            List<Photo> photos = await _db.Photo.ToListAsync();

            PhotoGalleryViewModel viewModel = new PhotoGalleryViewModel
            {
                User = user,
                Photos = photos
            };

            return View(viewModel);
        }
    }
}
