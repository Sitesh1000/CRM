using Crm.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Crm.Web.Pages.Modules;

public class IndexModel : PageModel
{
    [FromRoute]
    public string Module { get; set; } = string.Empty;

    public ModuleInfo? Info { get; private set; }

    public IActionResult OnGet()
    {
        if (!ModuleRegistry.Modules.TryGetValue(Module, out var info))
        {
            return NotFound();
        }

        Info = info;
        return Page();
    }
}
