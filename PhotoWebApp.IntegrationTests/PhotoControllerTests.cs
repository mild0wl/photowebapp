using PhotoWebApp.IntegrationTests.Factories;
using PhotoWebApp.Models;
using PhotoWebApp.Controllers;
using Microsoft.Extensions.DependencyInjection;
using PhotoWebApp.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PhotoWebApp.IntegrationTests
{
    public class PhotoControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _httpClient;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private ApplicationDbContext _dbContext;

        public PhotoControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });
            _factory = factory;
        }

        [Fact] // testing the photo upload and verify the photo existed
        public async Task UploadAndVerifyPhoto()
        {

            // Arrange: Mock IWebHostEnvironment and ILogger<CommentController>
            var mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            var mockLogger = new Mock<ILogger<CommentController>>();

            // Mock the content root path or other properties if needed
            mockWebHostEnvironment.Setup(m => m.ContentRootPath).Returns("TestRootPath");
            mockWebHostEnvironment.Setup(m => m.WebRootPath).Returns("TestWebRootPath");

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                        new Claim(ClaimTypes.NameIdentifier, "authenticateduser@photowebapp.com"),

            }, "mock"));

            using (var scope = _factory.Services.CreateScope())
            {

                var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var photoController = new PhotoController(_dbContext, mockWebHostEnvironment.Object, mockLogger.Object);

                photoController.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = user }
                };

                // Mock TempData for the controller
                photoController.TempData = new TempDataDictionary(
                    photoController.ControllerContext.HttpContext,
                    Mock.Of<ITempDataProvider>()
                );

                // arrange
                // Simulate the file upload using IFormFile
                var fileMock = new Mock<IFormFile>();
                var content = "Test file content";
                var fileName = "test_image.jpg";
                var ms = new MemoryStream();
                var writer = new StreamWriter(ms);
                writer.Write(content);
                writer.Flush();
                ms.Position = 0;

                fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
                fileMock.Setup(_ => _.FileName).Returns(fileName);
                fileMock.Setup(_ => _.Length).Returns(ms.Length);

                IFormFile file = fileMock.Object;

                Photo photo = new Photo
                {
                    photoId = 101,
                    photoTitle = "Test",
                    description = "Test",
                    DatePosted = DateTime.Now,
                    userId = 99,
                    tags = "#test",
                    CommentMode = true,
                    LikesCount = 20,
                    IsPublic = true,
                    PhotoFile = file
                };

                await photoController.Create(photo);
                Photo? photo101 = _dbContext.Photo.FirstOrDefault(u => u.photoId == photo.photoId);
                Assert.NotNull(photo101);
            }
        }
    }
}

   