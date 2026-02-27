using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Invoices;

public class DetailsModel : PageModel
{
    private readonly CrmDbContext _dbContext;
    public DetailsModel(CrmDbContext dbContext) { _dbContext = dbContext; }
    public Invoice? Invoice { get; private set; }
    public List<Payment> Payments { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Invoice = await _dbContext.Invoices.Include(x => x.Contact).Include(x => x.Quote).FirstOrDefaultAsync(x => x.Id == id);
        if (Invoice is null) return NotFound();
        Payments = await _dbContext.Payments.Where(x => x.InvoiceId == id).OrderByDescending(x => x.PaidOn).ToListAsync();
        return Page();
    }
}
