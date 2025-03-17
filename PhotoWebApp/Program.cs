using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using PhotoWebApp.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using PhotoWebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure file upload limit
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1 * 1024 * 1024; // 1 MB
});


// Logging service
var logDirPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");

if (!Directory.Exists(logDirPath))
{
    Directory.CreateDirectory(logDirPath);
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() 
    .WriteTo.Debug()
    .WriteTo.File(Path.Combine(logDirPath, "app-.log"), rollingInterval: RollingInterval.Day) 
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options=> 
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CSRF protection
builder.Services.AddAntiforgery(options => {
    options.HeaderName = "X-CSRF-TOKEN";
});


builder.Services.AddSession(options => 
{ 
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => 
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "photowebapp.com",
        ValidAudience = "photowebapp.com",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Y0uC@n00tF1ndMyK3y!myk3y1s$tr0ng"))
    };
 


    // allow to read the token from cookies
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["JwtToken"];
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            context.Response.StatusCode = 401;
            context.Response.Redirect("/Auth/");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.Redirect("/Auth/");
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.Redirect("/Auth/AccessDenied");
            return Task.CompletedTask;
        }
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Error implementation
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == StatusCodes.Status405MethodNotAllowed)
    {
        context.HttpContext.Response.Redirect("/Auth/");
    }
    if (response.StatusCode == StatusCodes.Status401Unauthorized)
    {
        context.HttpContext.Response.Redirect("/Auth/");
    }
    if (response.StatusCode == StatusCodes.Status403Forbidden)
    {
        context.HttpContext.Response.Redirect("/Auth/AccessDenied");
    }
    if (response.StatusCode == StatusCodes.Status404NotFound)
    {
        context.HttpContext.Response.Redirect("/Auth/");
    }
    if (response.StatusCode == StatusCodes.Status500InternalServerError)
    {
        context.HttpContext.Response.Redirect("/Auth/Error");
    }

});
// to redirect from http to https
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }