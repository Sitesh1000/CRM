using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Leads;

public class DetailsModel : PageModel
{
    private readonly CrmDbContext _dbContext;
    public DetailsModel(CrmDbContext dbContext) { _dbContext = dbContext; }

    public Lead? Lead { get; private set; }
    public List<Deal> Deals { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Lead = await _dbContext.Leads.Include(x => x.ConvertedContact).FirstOrDefaultAsync(x => x.Id == id);
        if (Lead is null) return NotFound();
        Deals = await _dbContext.Deals.Where(x => x.LeadId == id).OrderByDescending(x => x.CreatedAt).ToListAsync();
        return Page();
    }
}
