using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Invoices;

public class IndexModel : PageModel
{
    private readonly CrmDbContext _dbContext;
    public IndexModel(CrmDbContext dbContext) { _dbContext = dbContext; }

    public List<Invoice> Invoices { get; private set; } = new();
    public List<Quote> Quotes { get; private set; } = new();
    public List<Contact> Contacts { get; private set; } = new();

    [BindProperty] public InputModel Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        Invoices = await _dbContext.Invoices.Include(x => x.Contact).Include(x => x.Quote).OrderByDescending(x => x.CreatedAt).ToListAsync();
        Quotes = await _dbContext.Quotes.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync();
        Contacts = await _dbContext.Contacts.OrderBy(x => x.FirstName).Take(200).ToListAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var quoteId = Guid.TryParse(Input.QuoteId, out var q) ? q : (Guid?)null;
        var contactId = Guid.TryParse(Input.ContactId, out var c) ? c : (Guid?)null;

        if (Input.Id is null || Input.Id == Guid.Empty)
        {
            _dbContext.Invoices.Add(new Invoice
            {
                InvoiceNumber = string.IsNullOrWhiteSpace(Input.InvoiceNumber) ? $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}" : Input.InvoiceNumber,
                QuoteId = quoteId,
                ContactId = contactId,
                TotalAmount = Input.TotalAmount,
                PaidAmount = Input.PaidAmount,
                DueDate = Input.DueDate
            });
        }
        else
        {
            var existing = await _dbContext.Invoices.FirstOrDefaultAsync(x => x.Id == Input.Id.Value);
            if (existing is not null)
            {
                existing.InvoiceNumber = string.IsNullOrWhiteSpace(Input.InvoiceNumber) ? existing.InvoiceNumber : Input.InvoiceNumber;
                existing.QuoteId = quoteId;
                existing.ContactId = contactId;
                existing.TotalAmount = Input.TotalAmount;
                existing.PaidAmount = Input.PaidAmount;
                existing.DueDate = Input.DueDate;
            }
        }

        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(x => x.Id == id);
        if (invoice is not null)
        {
            _dbContext.Invoices.Remove(invoice);
            await _dbContext.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public class InputModel
    {
        public Guid? Id { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? QuoteId { get; set; }
        public string? ContactId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
