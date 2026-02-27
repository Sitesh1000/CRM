using Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Crm.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UserDetailsModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserDetailsModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public ApplicationUser? UserEntity { get; private set; }
    public IList<string> Roles { get; private set; } = new List<string>();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        UserEntity = await _userManager.FindByIdAsync(id.ToString());
        if (UserEntity is null) return NotFound();
        Roles = await _userManager.GetRolesAsync(UserEntity);
        return Page();
    }
}
