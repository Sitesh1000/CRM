using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Domain.Enums;
using Crm.Infrastructure.Identity;
using Crm.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Crm.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, string contentRootPath)
    {
        var connectionString = ResolveSqliteConnectionString(configuration.GetConnectionString("DefaultConnection"), contentRootPath);

        services.AddDbContext<CrmDbContext>(options => options.UseSqlite(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<CrmDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }

    private static string ResolveSqliteConnectionString(string? configuredConnectionString, string contentRootPath)
    {
        var sqlite = new SqliteConnectionStringBuilder(configuredConnectionString ?? "Data Source=App_Data/crm.db");

        if (string.IsNullOrWhiteSpace(sqlite.DataSource))
        {
            sqlite.DataSource = Path.Combine(contentRootPath, "App_Data", "crm.db");
        }
        else if (!Path.IsPathRooted(sqlite.DataSource))
        {
            sqlite.DataSource = Path.GetFullPath(Path.Combine(contentRootPath, sqlite.DataSource));
        }

        var dbDirectory = Path.GetDirectoryName(sqlite.DataSource);
        if (!string.IsNullOrWhiteSpace(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        return sqlite.ToString();
    }

    public static async Task SeedDemoDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        await db.Database.MigrateAsync();

        var roles = new[] { "Admin", "Sales" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
        }

        if (await userManager.FindByNameAsync("admin") is null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@crm.local",
                DisplayName = "System Admin",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (await userManager.FindByNameAsync("sales") is null)
        {
            var sales = new ApplicationUser
            {
                UserName = "sales",
                Email = "sales@crm.local",
                DisplayName = "Sales Demo",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(sales, "Sales123!");
            await userManager.AddToRoleAsync(sales, "Sales");
        }

        if (await db.Companies.AnyAsync())
        {
            return;
        }

        var company = new Company
        {
            Name = "Northwind Traders",
            Industry = "Retail",
            Website = "https://northwind.example",
            Phone = "+1-555-0100"
        };

        var contact = new Contact
        {
            FirstName = "Ava",
            LastName = "Johnson",
            Email = "ava.johnson@northwind.example",
            Phone = "+1-555-0101",
            Company = company
        };

        var lead = new Lead
        {
            Name = "Liam Carter",
            Email = "liam.carter@example.com",
            CompanyName = "Contoso",
            Source = "Website"
        };

        var category = new Category { Name = "Software", Description = "Software subscriptions" };
        var product = new Product { Name = "CRM Pro", Sku = "CRM-PRO", BasePrice = 99, Category = category };

        var deal = new Deal
        {
            Title = "Northwind Expansion",
            Amount = 5000,
            Contact = contact,
            Stage = DealStage.Proposal,
            ExpectedCloseDate = DateTime.UtcNow.AddDays(21)
        };

        var quote = new Quote
        {
            QuoteNumber = "Q-1001",
            Contact = contact,
            Deal = deal,
            TotalAmount = 990,
            Items =
            {
                new QuoteItem { Product = product, Quantity = 10, UnitPrice = 99, LineTotal = 990 }
            }
        };

        var invoice = new Invoice
        {
            InvoiceNumber = "INV-1001",
            Quote = quote,
            Contact = contact,
            TotalAmount = 990,
            PaidAmount = 300,
            DueDate = DateTime.UtcNow.AddDays(15),
            Items =
            {
                new InvoiceItem { Product = product, Quantity = 10, UnitPrice = 99, LineTotal = 990 }
            }
        };

        var payment = new Payment
        {
            Invoice = invoice,
            Amount = 300,
            Method = "Card",
            Reference = "PAY-1001"
        };

        var activity = new Activity
        {
            Contact = contact,
            Deal = deal,
            Type = "Call",
            Description = "Initial discovery call completed",
            DueAt = DateTime.UtcNow.AddDays(2)
        };

        db.AddRange(company, contact, lead, category, product, deal, quote, invoice, payment, activity);
        await db.SaveChangesAsync();
    }
}
