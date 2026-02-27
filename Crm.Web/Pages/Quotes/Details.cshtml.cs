using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Quotes;

public class DetailsModel : PageModel
{
    private readonly CrmDbContext _dbContext;
    public DetailsModel(CrmDbContext dbContext) { _dbContext = dbContext; }
    public Quote? Quote { get; private set; }
    public List<QuoteItem> Items { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Quote = await _dbContext.Quotes.Include(x => x.Contact).Include(x => x.Deal).FirstOrDefaultAsync(x => x.Id == id);
        if (Quote is null) return NotFound();
        Items = await _dbContext.QuoteItems.Include(x => x.Product).Where(x => x.QuoteId == id).ToListAsync();
        return Page();
    }
}
