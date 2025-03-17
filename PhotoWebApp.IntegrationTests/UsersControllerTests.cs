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
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PhotoWebApp.IntegrationTests
{
    public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _httpClient;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly ApplicationDbContext _dbCnxt;

        public UsersControllerTests(CustomWebApplicationFactory<Program> factory) 
        {

            _httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });
            _factory = factory;
            using (var scope = _factory.Services.CreateScope())
            {
                _dbCnxt = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            }
        }

        [Fact]
        public async Task RegisterUser()
        {
            // arrange

            string Username = "newregisteredUser";
            string Email = "newlyregisteredtestuser@test.com";

            var postData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Email", Email),
                new KeyValuePair<string, string>("Username", Username)
            });

            // act
            HttpResponseMessage? loginResp = await _httpClient.PostAsync("/Users/Register", postData);
            loginResp.EnsureSuccessStatusCode();
            Assert.True(loginResp.Headers.TryGetValues("Set-Cookie", out var cookies));
            var tempDataCookie = cookies.FirstOrDefault(c => c.Contains(".AspNetCore.Mvc.CookieTempDataProvider"));
            Assert.NotNull(tempDataCookie);
        }

        [Fact]
        public async Task DeleteProfile()
        {
            // create new principal
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                        new Claim(ClaimTypes.NameIdentifier, "authenticateduser@photowebapp.com"),
            }, "mock"));

            var usersController = new UsersController(_dbCnxt);

            usersController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            int userId = 99; // userId of authenticateduser

            HttpResponseMessage? loginResp = await _httpClient.GetAsync($"/Users/Delete/{userId}");
            loginResp.EnsureSuccessStatusCode();
            Assert.True(loginResp.Headers.TryGetValues("Set-Cookie", out var cookies));
            var JwtToken = cookies.FirstOrDefault(c => c.Contains("JwtToken"));
            Assert.Null(JwtToken);
        }
    }
}
