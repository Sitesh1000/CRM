using Crm.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Crm.Web.Pages;

[AllowAnonymous]
public class PrivacyModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public PrivacyModel(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await _signInManager.SignOutAsync();
        return RedirectToPage("/Auth/Login");
    }
}
