using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Quotes;

public class IndexModel : PageModel
{
    private readonly CrmDbContext _dbContext;
    public IndexModel(CrmDbContext dbContext) { _dbContext = dbContext; }

    public List<Quote> Quotes { get; private set; } = new();
    public List<Deal> Deals { get; private set; } = new();
    public List<Contact> Contacts { get; private set; } = new();

    [BindProperty] public InputModel Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        Quotes = await _dbContext.Quotes.Include(x => x.Contact).Include(x => x.Deal).OrderByDescending(x => x.CreatedAt).ToListAsync();
        Deals = await _dbContext.Deals.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync();
        Contacts = await _dbContext.Contacts.OrderBy(x => x.FirstName).Take(200).ToListAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var dealId = Guid.TryParse(Input.DealId, out var d) ? d : (Guid?)null;
        var contactId = Guid.TryParse(Input.ContactId, out var c) ? c : (Guid?)null;

        if (Input.Id is null || Input.Id == Guid.Empty)
        {
            _dbContext.Quotes.Add(new Quote
            {
                QuoteNumber = string.IsNullOrWhiteSpace(Input.QuoteNumber) ? $"Q-{DateTime.UtcNow:yyyyMMddHHmmss}" : Input.QuoteNumber,
                DealId = dealId,
                ContactId = contactId,
                TotalAmount = Input.TotalAmount
            });
        }
        else
        {
            var existing = await _dbContext.Quotes.FirstOrDefaultAsync(x => x.Id == Input.Id.Value);
            if (existing is not null)
            {
                existing.QuoteNumber = string.IsNullOrWhiteSpace(Input.QuoteNumber) ? existing.QuoteNumber : Input.QuoteNumber;
                existing.DealId = dealId;
                existing.ContactId = contactId;
                existing.TotalAmount = Input.TotalAmount;
            }
        }

        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var quote = await _dbContext.Quotes.FirstOrDefaultAsync(x => x.Id == id);
        if (quote is not null)
        {
            _dbContext.Quotes.Remove(quote);
            await _dbContext.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public class InputModel
    {
        public Guid? Id { get; set; }
        public string? QuoteNumber { get; set; }
        public string? DealId { get; set; }
        public string? ContactId { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
