using Crm.Infrastructure;
using Crm.Web.Api;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
var contentRootPath = builder.Environment.ContentRootPath;
Directory.CreateDirectory(Path.Combine(contentRootPath, "App_Data"));

// 🔥 ADD THIS FOR RENDER
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddInfrastructure(builder.Configuration, contentRootPath);
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.AddAuthorization();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Auth/Login");
    options.Conventions.AllowAnonymousToPage("/Auth/AccessDenied");
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

var resolvedDbPath = DependencyInjection.ResolveSqliteDataSource(app.Configuration, app.Environment.ContentRootPath);
app.Logger.LogInformation("SQLite database path: {DatabasePath}", resolvedDbPath);

await DependencyInjection.SeedDemoDataAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapCrmApis();

app.MapGet("/", () => Results.Redirect("/Dashboard"));

app.Run();
