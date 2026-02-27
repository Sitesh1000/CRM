using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Contacts;

public class DetailsModel : PageModel
{
    private readonly CrmDbContext _dbContext;

    public DetailsModel(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Contact? Contact { get; private set; }
    public List<Activity> Activities { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Contact = await _dbContext.Contacts
            .Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (Contact is null)
        {
            return NotFound();
        }

        Activities = await _dbContext.Activities
            .Where(x => x.ContactId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Page();
    }
}
