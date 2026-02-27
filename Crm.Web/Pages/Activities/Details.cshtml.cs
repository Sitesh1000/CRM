using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Activities;

public class DetailsModel : PageModel
{
    private readonly CrmDbContext _dbContext;
    public DetailsModel(CrmDbContext dbContext) { _dbContext = dbContext; }
    public Activity? Activity { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Activity = await _dbContext.Activities.Include(x => x.Contact).Include(x => x.Deal).FirstOrDefaultAsync(x => x.Id == id);
        if (Activity is null) return NotFound();
        return Page();
    }
}
