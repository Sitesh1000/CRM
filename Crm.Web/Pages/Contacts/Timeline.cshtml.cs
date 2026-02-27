using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Contacts;

public class TimelineModel : PageModel
{
    private readonly CrmDbContext _dbContext;

    public TimelineModel(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty(SupportsGet = true)]
    public string? ContactId { get; set; }

    public Contact? Contact { get; private set; }
    public List<Activity> Items { get; private set; } = new();

    public async Task OnGetAsync()
    {
        if (!Guid.TryParse(ContactId, out var id))
        {
            return;
        }

        Contact = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == id);
        if (Contact is null)
        {
            return;
        }

        Items = await _dbContext.Activities
            .Where(x => x.ContactId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}
