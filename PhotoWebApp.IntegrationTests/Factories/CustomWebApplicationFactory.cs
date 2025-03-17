using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoWebApp.Controllers;
using PhotoWebApp.Data;
using PhotoWebApp.IntegrationTests.Mocks;
using PhotoWebApp.Models;
using PhotoWebApp.Services;
using System.Security.Claims;


namespace PhotoWebApp.IntegrationTests.Factories
{
    public class CustomWebApplicationFactory<TStartup>: WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // removing the Db context for integration testing
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // creating the in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(Options =>
                {
                    Options.UseInMemoryDatabase("UsingInMemoryDbForIntegrationTesting");
                });

                services.AddScoped<AuthController>();
                services.AddScoped<CommentController>();
                services.AddScoped<PhotoController>();
                services.AddScoped<UsersController>();
                services.AddScoped<IEmailSender, MockEmailSender>();

                // check the database existed
                var serviceProvider = services.BuildServiceProvider();
                using (IServiceScope? scope = serviceProvider.CreateScope()) {

                    IServiceProvider? scopedServices = scope.ServiceProvider;
                    ApplicationDbContext? db = scopedServices.GetRequiredService<ApplicationDbContext>();
                    var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

                    db.Database.EnsureCreated();

                    try
                    {
                        // seeding the data into in-memory database
                        SeedTestData(db);
                    }
                    catch (Exception ex) {
                        logger.LogError(ex, "An error occurred seeding the in-memory database.");
                    }
                }
            });
        } 

        private void SeedTestData(ApplicationDbContext db)
        {
            // creating authenticated user for testing

            db.Users.Add(new Users
            {
                UserId = 99,
                Email = "authenticateduser@photowebapp.com",
                Username = "authenticateduser",
                Role = UserRole.ContentCreator,
            });
            db.Users.Add(new Users
            {
                UserId = 100,
                Email = "sai44532@gmail.com",
                Username = "admintest1",
                Role = UserRole.Admin,
            });
            db.Users.Add(new Users
            {
                UserId = 101,
                Email = "ccuserfortesting1@photowebapp.com",
                Username = "ccuserfortesting1",
                Role = UserRole.ContentCreator,
            });
            db.Photo.Add( new Photo
            {
                photoId = 1,
                userId = 100,
                photoTitle = "Test Photo Title",
                description = "TEst Photo description",
                tags = "#testtag",
                CommentMode = true,
                DatePosted = DateTime.Now,
                IsPublic = true,
            });
            db.Photo.Add(new Photo
            {
                photoId = 2,
                userId = 101,
                photoTitle = "Test Photo Title2",
                description = "TEst Photo description2",
                tags = "#testtag2",
                CommentMode = true,
                DatePosted = DateTime.Now,
                IsPublic = true,
            });
            db.Comment.Add(new Comment
            {
                commentValue = "testcomment",
                CommentId = 1,
                DatePosted = DateTime.Now,
                Flagged = true,
                PhotoId = 1,
            });
            db.Comment.Add(new Comment
            {
                commentValue = "testcomment2",
                CommentId = 2,
                DatePosted = DateTime.Now,
                Flagged = false,
                PhotoId = 2,
            });

            db.SaveChanges();
        }
    }
}
