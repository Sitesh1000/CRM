using Crm.Application.DTOs;
using Crm.Application.Interfaces;
using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Crm.Web.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly IDashboardService _dashboardService;
    private readonly CrmDbContext _dbContext;

    public IndexModel(IDashboardService dashboardService, CrmDbContext dbContext)
    {
        _dashboardService = dashboardService;
        _dbContext = dbContext;
    }

    public DashboardMetricsDto Metrics { get; private set; } = new();
    public List<Deal> ActiveDeals { get; private set; } = new();
    public List<Lead> LatestLeads { get; private set; } = new();
    public List<Activity> UpcomingActivities { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Metrics = await _dashboardService.GetMetricsAsync();

        ActiveDeals = await _dbContext.Deals
            .Include(x => x.Contact)
            .Where(x => x.Stage != Domain.Enums.DealStage.Won && x.Stage != Domain.Enums.DealStage.Lost)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .ToListAsync();

        LatestLeads = await _dbContext.Leads
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .ToListAsync();

        UpcomingActivities = await _dbContext.Activities
            .Include(x => x.Contact)
            .Where(x => !x.IsCompleted)
            .OrderBy(x => x.DueAt ?? DateTime.MaxValue)
            .Take(6)
            .ToListAsync();
    }
}
