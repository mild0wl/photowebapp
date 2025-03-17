using Microsoft.VisualStudio.TestPlatform.TestHost;
using PhotoWebApp.IntegrationTests.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotoWebApp.Models;
using PhotoWebApp.Controllers;
using Microsoft.Extensions.DependencyInjection;
using PhotoWebApp.Data;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PhotoWebApp.IntegrationTests
{
    public class CommentControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _httpClient;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public CommentControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });
            _factory = factory;
        }

        [Fact]
        public async Task PostCommentAndVeriyIt()
        {
            // arrange
            string commentVal = "Test Comment";
            int photoId = 1;

            // act 
            using (var scope = _factory.Services.CreateScope()) 
            {
                CommentController? commentController = scope.ServiceProvider.GetService<CommentController>();
                await commentController.PostComment(photoId, commentVal);
            }

            // assert
            using (var scope = _factory.Services.CreateScope())
            {
                ApplicationDbContext? dbCnxt = scope.ServiceProvider.GetService<ApplicationDbContext>();
                Comment? comment = dbCnxt.Comment.FirstOrDefault(c => c.commentValue == commentVal);

                Assert.NotNull(comment);
            }
        }

        [Fact]
        public async Task PostCommentAndDeleteIt()
        {
            // Post Comment and Verify it

            // arrange : 1
            string commentVal = "Test Comment";
            int photoId = 1;

            // act : 1
            using (var scope = _factory.Services.CreateScope())
            {
                CommentController? commentController = scope.ServiceProvider.GetService<CommentController>();
                await commentController.PostComment(photoId, commentVal);
            }

            // assert : 1
            
            using (var scope = _factory.Services.CreateScope())
            {
                ApplicationDbContext? dbCnxt = scope.ServiceProvider.GetService<ApplicationDbContext>();
                Comment? comment = dbCnxt.Comment.FirstOrDefault(c => c.commentValue == commentVal);

                // for Deleting // // arrange : 2
                //commentId = comment.CommentId;

                Assert.NotNull(comment);
            }

            // Delete Comment Test Case
            int commentId = 1;
            // act : 2
            using (var scope = _factory.Services.CreateScope())
            {
                CommentController? commentController = scope.ServiceProvider.GetService<CommentController>();
                commentController.Delete(commentId);
            }

            // assert : 2
            using (var scope = _factory.Services.CreateScope())
            {
                ApplicationDbContext? dbCnxt = scope.ServiceProvider.GetService<ApplicationDbContext>();
                Comment? comment = dbCnxt.Comment.FirstOrDefault(c => c.CommentId == commentId);

                Assert.Null(comment);
            }
        }
    }
}
