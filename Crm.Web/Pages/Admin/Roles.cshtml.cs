using Crm.Infrastructure.Identity;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class RolesModel : PageModel
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly CrmDbContext _dbContext;

    public RolesModel(RoleManager<ApplicationRole> roleManager, CrmDbContext dbContext)
    {
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    public List<ApplicationRole> Roles { get; private set; } = new();
    public Dictionary<Guid, int> UserCounts { get; private set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        Roles = await _roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
        UserCounts = await _dbContext.UserRoles
            .GroupBy(x => x.RoleId)
            .Select(g => new { RoleId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoleId, x => x.Count);
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (Input.Id is null || Input.Id == Guid.Empty)
        {
            if (!string.IsNullOrWhiteSpace(Input.Name))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = Input.Name });
            }
        }
        else
        {
            var role = await _roleManager.FindByIdAsync(Input.Id.Value.ToString());
            if (role is not null && !string.IsNullOrWhiteSpace(Input.Name))
            {
                role.Name = Input.Name;
                role.NormalizedName = Input.Name.ToUpperInvariant();
                await _roleManager.UpdateAsync(role);
            }
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role is not null)
        {
            await _roleManager.DeleteAsync(role);
        }

        return RedirectToPage();
    }

    public class InputModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
