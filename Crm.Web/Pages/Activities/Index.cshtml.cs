using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Activities;

public class IndexModel : PageModel
{
    private readonly CrmDbContext _dbContext;
    public IndexModel(CrmDbContext dbContext) { _dbContext = dbContext; }

    public List<Activity> Activities { get; private set; } = new();
    public List<Contact> Contacts { get; private set; } = new();
    public List<Deal> Deals { get; private set; } = new();

    [BindProperty] public InputModel Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        Activities = await _dbContext.Activities.Include(x => x.Contact).Include(x => x.Deal).OrderBy(x => x.IsCompleted).ThenBy(x => x.DueAt).ToListAsync();
        Contacts = await _dbContext.Contacts.OrderBy(x => x.FirstName).Take(200).ToListAsync();
        Deals = await _dbContext.Deals.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var contactId = Guid.TryParse(Input.ContactId, out var c) ? c : (Guid?)null;
        if (contactId is null) return RedirectToPage();
        var dealId = Guid.TryParse(Input.DealId, out var d) ? d : (Guid?)null;

        if (Input.Id is null || Input.Id == Guid.Empty)
        {
            _dbContext.Activities.Add(new Activity
            {
                ContactId = contactId.Value,
                DealId = dealId,
                Type = Input.Type,
                Description = Input.Description,
                DueAt = Input.DueAt,
                IsCompleted = Input.IsCompleted
            });
        }
        else
        {
            var existing = await _dbContext.Activities.FirstOrDefaultAsync(x => x.Id == Input.Id.Value);
            if (existing is not null)
            {
                existing.ContactId = contactId.Value;
                existing.DealId = dealId;
                existing.Type = Input.Type;
                existing.Description = Input.Description;
                existing.DueAt = Input.DueAt;
                existing.IsCompleted = Input.IsCompleted;
            }
        }

        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var a = await _dbContext.Activities.FirstOrDefaultAsync(x => x.Id == id);
        if (a is not null) { _dbContext.Activities.Remove(a); await _dbContext.SaveChangesAsync(); }
        return RedirectToPage();
    }

    public class InputModel
    {
        public Guid? Id { get; set; }
        public string? ContactId { get; set; }
        public string? DealId { get; set; }
        public string Type { get; set; } = "Task";
        public string Description { get; set; } = string.Empty;
        public DateTime? DueAt { get; set; }
        public bool IsCompleted { get; set; }
    }
}
