using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Contacts;

public class IndexModel : PageModel
{
    private readonly CrmDbContext _dbContext;

    public IndexModel(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<Contact> Contacts { get; private set; } = new();
    public List<Company> Companies { get; private set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        Contacts = await _dbContext.Contacts
            .Include(x => x.Company)
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .ToListAsync();

        Companies = await _dbContext.Companies.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var companyId = Guid.TryParse(Input.CompanyId, out var parsedCompanyId) ? parsedCompanyId : (Guid?)null;

        if (Input.Id is null || Input.Id == Guid.Empty)
        {
            _dbContext.Contacts.Add(new Contact
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Email = Input.Email,
                Phone = Input.Phone,
                CompanyId = companyId
            });
        }
        else
        {
            var existing = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == Input.Id.Value);
            if (existing is not null)
            {
                existing.FirstName = Input.FirstName;
                existing.LastName = Input.LastName;
                existing.Email = Input.Email;
                existing.Phone = Input.Phone;
                existing.CompanyId = companyId;
            }
        }

        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var contact = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == id);
        if (contact is not null)
        {
            _dbContext.Contacts.Remove(contact);
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public class InputModel
    {
        public Guid? Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? CompanyId { get; set; }
    }
}
