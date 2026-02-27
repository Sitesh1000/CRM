using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Deals;

public class DetailsModel : PageModel
{
    private readonly CrmDbContext _dbContext;

    public DetailsModel(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Deal? Deal { get; private set; }
    public List<Activity> Activities { get; private set; } = new();
    public List<Quote> Quotes { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Deal = await _dbContext.Deals
            .Include(x => x.Contact)
            .Include(x => x.Lead)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (Deal is null)
        {
            return NotFound();
        }

        Activities = await _dbContext.Activities
            .Where(x => x.DealId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        Quotes = await _dbContext.Quotes
            .Where(x => x.DealId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Page();
    }
}
