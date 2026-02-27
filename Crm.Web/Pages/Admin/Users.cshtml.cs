using Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Crm.Web.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UsersModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UsersModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public List<ApplicationUser> Users { get; private set; } = new();
    public Dictionary<Guid, IList<string>> UserRoles { get; private set; } = new();
    public List<string> AvailableRoles { get; private set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        Users = _userManager.Users.OrderBy(x => x.UserName).ToList();
        AvailableRoles = _roleManager.Roles
            .OrderBy(x => x.Name)
            .Select(x => x.Name!)
            .ToList();

        foreach (var user in Users)
        {
            UserRoles[user.Id] = await _userManager.GetRolesAsync(user);
        }
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (Input.Id is null || Input.Id == Guid.Empty)
        {
            var user = new ApplicationUser
            {
                UserName = Input.UserName,
                Email = Input.Email,
                DisplayName = Input.DisplayName,
                EmailConfirmed = true
            };
            var created = await _userManager.CreateAsync(user, string.IsNullOrWhiteSpace(Input.Password) ? "User123!" : Input.Password);
            if (!created.Succeeded) return RedirectToPage();
            if (!string.IsNullOrWhiteSpace(Input.Role)) await _userManager.AddToRoleAsync(user, Input.Role);
        }
        else
        {
            var user = await _userManager.FindByIdAsync(Input.Id.Value.ToString());
            if (user is not null)
            {
                user.UserName = Input.UserName;
                user.Email = Input.Email;
                user.DisplayName = Input.DisplayName;
                await _userManager.UpdateAsync(user);

                var existingRoles = await _userManager.GetRolesAsync(user);
                if (existingRoles.Any()) await _userManager.RemoveFromRolesAsync(user, existingRoles);
                if (!string.IsNullOrWhiteSpace(Input.Role)) await _userManager.AddToRoleAsync(user, Input.Role);

                if (!string.IsNullOrWhiteSpace(Input.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, Input.Password);
                }
            }
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is not null)
        {
            await _userManager.DeleteAsync(user);
        }

        return RedirectToPage();
    }

    public class InputModel
    {
        public Guid? Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? Role { get; set; }
    }
}
