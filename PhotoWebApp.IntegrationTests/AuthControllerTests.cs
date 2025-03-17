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
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _httpClient;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public AuthControllerTests(CustomWebApplicationFactory<Program> factory) 
        {
            _httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });
            _factory = factory;
        }

        [Fact]
        public async Task SendTokenAndStoreToken()
        {
            // arrange
            string email = "sai44532@gmail.com";

            // act
            using (var scope = _factory.Services.CreateScope())
            {
                AuthController authController = scope.ServiceProvider.GetService<AuthController>();
                await authController.SendToken(email);
            }

            // assert
            using (var scope = _factory.Services.CreateScope())
            {
                ApplicationDbContext? dbCnxt = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Users? user = dbCnxt.Users.FirstOrDefault(u => u.Email == email);

                Assert.NotNull(user);
                Assert.NotNull(user.Token);
            }
        }

        [Fact]
        public async Task VerifyTokenAndReturnJwt()
        {
            // arrage : 1
            string email = "sai44532@gmail.com";

            using (var scope = _factory.Services.CreateScope())
            {
                AuthController? authController = scope.ServiceProvider.GetService<AuthController>();
                await authController.SendToken(email);
            }
            // act : 1
            string generatedToken = "";
            using (var scope = _factory.Services.CreateScope())
            {
                ApplicationDbContext? dbCnxt = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Users? user = dbCnxt.Users.FirstOrDefault(u => u.Email == email);
                generatedToken = user.Token;
            }

            // assert : 1
            Assert.NotNull(generatedToken);

            // arrage : 2
            var postData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Email", email),
                new KeyValuePair<string, string>("Token", generatedToken)
            });

            // act : 2
            HttpResponseMessage? loginResp = await _httpClient.PostAsync("/Auth/VerifyToken", postData);

            // assert : 2
            loginResp.EnsureSuccessStatusCode();

            Assert.True(loginResp.Headers.TryGetValues("Set-Cookie", out var cookies));

            var jwtCookie = cookies.FirstOrDefault(c => c.Contains("JwtToken"));
            Assert.NotNull(jwtCookie);

        }
    }
}
