using Crm.Infrastructure.Identity;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class RoleDetailsModel : PageModel
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly CrmDbContext _dbContext;

    public RoleDetailsModel(RoleManager<ApplicationRole> roleManager, CrmDbContext dbContext)
    {
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    public ApplicationRole? RoleEntity { get; private set; }
    public int UserCount { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        RoleEntity = await _roleManager.FindByIdAsync(id.ToString());
        if (RoleEntity is null) return NotFound();
        UserCount = await _dbContext.UserRoles.CountAsync(x => x.RoleId == id);
        return Page();
    }
}
