using Crm.Application.Interfaces;
using Crm.Domain.Common;
using Crm.Domain.Entities;
using Crm.Domain.Enums;
using Crm.Infrastructure.Identity;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Api;

public static class CrmApiExtensions
{
    public static void MapCrmApis(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").RequireAuthorization();

        MapCrud<Company>(api, "companies");
        MapCrud<Contact>(api, "contacts");
        MapCrud<Lead>(api, "leads");
        MapCrud<Deal>(api, "deals");
        MapCrud<Category>(api, "categories");
        MapCrud<Product>(api, "products");
        MapCrud<SpecialPrice>(api, "special-prices");
        MapCrud<Quote>(api, "quotes");
        MapCrud<QuoteItem>(api, "quote-items");
        MapCrud<Invoice>(api, "invoices");
        MapCrud<InvoiceItem>(api, "invoice-items");
        MapCrud<Payment>(api, "payments");
        MapCrud<Activity>(api, "activities");

        api.MapGet("/dashboard/metrics", async (IDashboardService dashboardService, CancellationToken ct) =>
            Results.Ok(await dashboardService.GetMetricsAsync(ct)));

        api.MapPost("/leads/{id:guid}/convert", async (Guid id, CrmDbContext db, CancellationToken ct) =>
        {
            var lead = await db.Leads.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (lead is null)
            {
                return Results.NotFound();
            }

            if (lead.IsConverted)
            {
                return Results.BadRequest(new { message = "Lead is already converted." });
            }

            Company? company = null;
            if (!string.IsNullOrWhiteSpace(lead.CompanyName))
            {
                company = await db.Companies.FirstOrDefaultAsync(x => x.Name == lead.CompanyName, ct);
                if (company is null)
                {
                    company = new Company { Name = lead.CompanyName! };
                    db.Companies.Add(company);
                }
            }

            var nameParts = lead.Name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var contact = new Contact
            {
                FirstName = nameParts.ElementAtOrDefault(0) ?? lead.Name,
                LastName = nameParts.ElementAtOrDefault(1) ?? string.Empty,
                Email = lead.Email,
                Phone = lead.Phone,
                Company = company
            };

            db.Contacts.Add(contact);

            var deal = new Deal
            {
                Title = $"Opportunity - {lead.Name}",
                Amount = 0,
                Stage = DealStage.Qualification,
                Contact = contact,
                Lead = lead
            };
            db.Deals.Add(deal);

            lead.IsConverted = true;
            lead.ConvertedContact = contact;

            await db.SaveChangesAsync(ct);

            return Results.Ok(new { lead.Id, ContactId = contact.Id, DealId = deal.Id });
        });

        api.MapGet("/contacts/{id:guid}/timeline", async (Guid id, CrmDbContext db, CancellationToken ct) =>
        {
            var timeline = await db.Activities
                .Where(x => x.ContactId == id)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new { x.Id, x.Type, x.Description, x.DueAt, x.IsCompleted, x.CreatedAt })
                .ToListAsync(ct);

            return Results.Ok(timeline);
        });

        var admin = api.MapGroup("/admin").RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        admin.MapGet("/users", async (UserManager<ApplicationUser> userManager) =>
        {
            var users = await userManager.Users.Select(u => new { u.Id, u.UserName, u.Email, u.DisplayName }).ToListAsync();
            return Results.Ok(users);
        });

        admin.MapPost("/users", async (CreateUserRequest request, UserManager<ApplicationUser> userManager) =>
        {
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                DisplayName = request.DisplayName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return Results.BadRequest(result.Errors);
            }

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                await userManager.AddToRoleAsync(user, request.Role);
            }

            return Results.Ok(new { user.Id, user.UserName });
        });

        admin.MapDelete("/users/{id:guid}", async (Guid id, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user is null)
            {
                return Results.NotFound();
            }

            await userManager.DeleteAsync(user);
            return Results.NoContent();
        });

        admin.MapGet("/roles", async (RoleManager<ApplicationRole> roleManager) =>
        {
            var roles = await roleManager.Roles.Select(r => new { r.Id, r.Name }).ToListAsync();
            return Results.Ok(roles);
        });

        admin.MapPost("/roles", async (CreateRoleRequest request, RoleManager<ApplicationRole> roleManager) =>
        {
            var result = await roleManager.CreateAsync(new ApplicationRole { Name = request.Name });
            if (!result.Succeeded)
            {
                return Results.BadRequest(result.Errors);
            }

            return Results.Ok();
        });

        admin.MapDelete("/roles/{id:guid}", async (Guid id, RoleManager<ApplicationRole> roleManager) =>
        {
            var role = await roleManager.FindByIdAsync(id.ToString());
            if (role is null)
            {
                return Results.NotFound();
            }

            await roleManager.DeleteAsync(role);
            return Results.NoContent();
        });
    }

    private static void MapCrud<TEntity>(RouteGroupBuilder group, string route)
        where TEntity : BaseEntity
    {
        var endpoints = group.MapGroup($"/{route}");

        endpoints.MapGet("/", async (CrmDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Set<TEntity>().OrderByDescending(x => x.CreatedAt).ToListAsync(ct)));

        endpoints.MapGet("/{id:guid}", async (Guid id, CrmDbContext db, CancellationToken ct) =>
        {
            var entity = await db.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id, ct);
            return entity is null ? Results.NotFound() : Results.Ok(entity);
        });

        endpoints.MapPost("/", async (TEntity entity, CrmDbContext db, CancellationToken ct) =>
        {
            entity.Id = Guid.NewGuid();
            db.Set<TEntity>().Add(entity);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/{route}/{entity.Id}", entity);
        });

        endpoints.MapPut("/{id:guid}", async (Guid id, TEntity payload, CrmDbContext db, CancellationToken ct) =>
        {
            var current = await db.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (current is null)
            {
                return Results.NotFound();
            }

            db.Entry(current).CurrentValues.SetValues(payload);
            current.Id = id;
            await db.SaveChangesAsync(ct);
            return Results.Ok(current);
        });

        endpoints.MapDelete("/{id:guid}", async (Guid id, CrmDbContext db, CancellationToken ct) =>
        {
            var current = await db.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (current is null)
            {
                return Results.NotFound();
            }

            db.Set<TEntity>().Remove(current);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }

    public record CreateUserRequest(string UserName, string Email, string DisplayName, string Password, string Role);
    public record CreateRoleRequest(string Name);
}
