using Crm.Domain.Entities;
using Crm.Domain.Enums;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Deals;

public class PipelineModel : PageModel
{
    private readonly CrmDbContext _dbContext;

    public PipelineModel(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Dictionary<DealStage, List<Deal>> Board { get; private set; } = new();
    public List<Contact> Contacts { get; private set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        var deals = await _dbContext.Deals
            .Include(x => x.Contact)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        Board = Enum.GetValues<DealStage>().ToDictionary(
            stage => stage,
            stage => deals.Where(d => d.Stage == stage).ToList());

        Contacts = await _dbContext.Contacts.OrderBy(x => x.FirstName).ToListAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var contactId = Guid.TryParse(Input.ContactId, out var parsed) ? parsed : (Guid?)null;

        if (Input.Id is null || Input.Id == Guid.Empty)
        {
            _dbContext.Deals.Add(new Deal
            {
                Title = Input.Title,
                Amount = Input.Amount,
                Stage = Input.Stage,
                ContactId = contactId,
                ExpectedCloseDate = Input.ExpectedCloseDate
            });
        }
        else
        {
            var existing = await _dbContext.Deals.FirstOrDefaultAsync(x => x.Id == Input.Id.Value);
            if (existing is not null)
            {
                existing.Title = Input.Title;
                existing.Amount = Input.Amount;
                existing.Stage = Input.Stage;
                existing.ContactId = contactId;
                existing.ExpectedCloseDate = Input.ExpectedCloseDate;
            }
        }

        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveAsync(Guid id, DealStage stage)
    {
        var deal = await _dbContext.Deals.FirstOrDefaultAsync(x => x.Id == id);
        if (deal is not null)
        {
            deal.Stage = stage;
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var deal = await _dbContext.Deals.FirstOrDefaultAsync(x => x.Id == id);
        if (deal is not null)
        {
            _dbContext.Deals.Remove(deal);
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public class InputModel
    {
        public Guid? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DealStage Stage { get; set; } = DealStage.Qualification;
        public string? ContactId { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }
    }
}
