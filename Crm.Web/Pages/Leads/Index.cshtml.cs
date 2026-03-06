using Crm.Domain.Entities;
using Crm.Domain.Enums;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Leads;

public class IndexModel : PageModel
{
    private readonly CrmDbContext _dbContext;

    public IndexModel(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<Lead> Leads { get; private set; } = new();
    public List<string> CompanyOptions { get; private set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        Leads = await _dbContext.Leads
            .Include(x => x.ConvertedContact)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        await LoadCompanyOptionsAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var companyName = Input.CompanyName?.Trim();

        if (!string.IsNullOrWhiteSpace(companyName))
        {
            var companyExists = await _dbContext.Companies.AnyAsync(x => x.Name == companyName);
            if (!companyExists)
            {
                _dbContext.Companies.Add(new Company { Name = companyName });
            }
        }

        if (Input.Id is null || Input.Id == Guid.Empty)
        {
            _dbContext.Leads.Add(new Lead
            {
                Name = Input.Name,
                Email = Input.Email,
                Phone = Input.Phone,
                CompanyName = companyName,
                Source = Input.Source
            });
        }
        else
        {
            var existing = await _dbContext.Leads.FirstOrDefaultAsync(x => x.Id == Input.Id.Value);
            if (existing is not null)
            {
                existing.Name = Input.Name;
                existing.Email = Input.Email;
                existing.Phone = Input.Phone;
                existing.CompanyName = companyName;
                existing.Source = Input.Source;
            }
        }

        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var lead = await _dbContext.Leads.FirstOrDefaultAsync(x => x.Id == id);
        if (lead is not null)
        {
            _dbContext.Leads.Remove(lead);
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostConvertAsync(Guid id)
    {
        var lead = await _dbContext.Leads.FirstOrDefaultAsync(x => x.Id == id);
        if (lead is null || lead.IsConverted)
        {
            return RedirectToPage();
        }

        Company? company = null;
        if (!string.IsNullOrWhiteSpace(lead.CompanyName))
        {
            company = await _dbContext.Companies.FirstOrDefaultAsync(x => x.Name == lead.CompanyName);
            if (company is null)
            {
                company = new Company { Name = lead.CompanyName };
                _dbContext.Companies.Add(company);
            }
        }

        var names = lead.Name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var contact = new Contact
        {
            FirstName = names.ElementAtOrDefault(0) ?? lead.Name,
            LastName = names.ElementAtOrDefault(1) ?? string.Empty,
            Email = lead.Email,
            Phone = lead.Phone,
            Company = company
        };
        _dbContext.Contacts.Add(contact);

        _dbContext.Deals.Add(new Deal
        {
            Title = $"Opportunity - {lead.Name}",
            Amount = 0,
            Stage = DealStage.Qualification,
            Contact = contact,
            Lead = lead
        });

        lead.IsConverted = true;
        lead.ConvertedContact = contact;

        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public class InputModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? CompanyName { get; set; }
        public string? Source { get; set; }
    }

    private async Task LoadCompanyOptionsAsync()
    {
        var companyNames = await _dbContext.Companies
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToListAsync();

        var leadCompanyNames = await _dbContext.Leads
            .Select(x => x.CompanyName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToListAsync();

        CompanyOptions = companyNames
            .Concat(leadCompanyNames)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
    }
}
