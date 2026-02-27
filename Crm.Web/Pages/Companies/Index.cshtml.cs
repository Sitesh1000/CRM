using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Companies;

public class IndexModel : PageModel
{
    private readonly CrmDbContext _dbContext;

    public IndexModel(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<Company> Companies { get; private set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        Companies = await _dbContext.Companies
            .Include(x => x.Contacts)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        if (Input.Id is null || Input.Id == Guid.Empty)
        {
            _dbContext.Companies.Add(new Company
            {
                Name = Input.Name,
                Industry = Input.Industry,
                Website = Input.Website,
                Phone = Input.Phone,
                Address = Input.Address
            });
        }
        else
        {
            var existing = await _dbContext.Companies.FirstOrDefaultAsync(x => x.Id == Input.Id.Value);
            if (existing is not null)
            {
                existing.Name = Input.Name;
                existing.Industry = Input.Industry;
                existing.Website = Input.Website;
                existing.Phone = Input.Phone;
                existing.Address = Input.Address;
            }
        }

        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var company = await _dbContext.Companies.FirstOrDefaultAsync(x => x.Id == id);
        if (company is not null)
        {
            _dbContext.Companies.Remove(company);
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public class InputModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? Website { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
