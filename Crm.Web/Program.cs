using Crm.Infrastructure;
using Crm.Web.Api;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=App_Data/crm.db";

EnsureSqliteDirectory(defaultConnection, builder.Environment.ContentRootPath);

builder.Services.AddInfrastructure(builder.Configuration);
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

static void EnsureSqliteDirectory(string connectionString, string contentRootPath)
{
    var dataSourcePart = connectionString
        .Split(';', StringSplitOptions.RemoveEmptyEntries)
        .FirstOrDefault(x => x.TrimStart().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase));

    if (string.IsNullOrWhiteSpace(dataSourcePart))
    {
        return;
    }

    var sqlitePath = dataSourcePart[(dataSourcePart.IndexOf('=') + 1)..].Trim().Trim('"');
    if (string.IsNullOrWhiteSpace(sqlitePath) || sqlitePath == ":memory:")
    {
        return;
    }

    if (!Path.IsPathRooted(sqlitePath))
    {
        sqlitePath = Path.Combine(contentRootPath, sqlitePath);
    }

    var directory = Path.GetDirectoryName(sqlitePath);
    if (!string.IsNullOrWhiteSpace(directory))
    {
        Directory.CreateDirectory(directory);
    }
}
