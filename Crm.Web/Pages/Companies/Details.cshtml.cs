using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Companies;

public class DetailsModel : PageModel
{
    private readonly CrmDbContext _dbContext;

    public DetailsModel(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Company? Company { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Company = await _dbContext.Companies
            .Include(x => x.Contacts)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (Company is null)
        {
            return NotFound();
        }

        return Page();
    }
}
