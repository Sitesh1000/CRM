using Crm.Application.DTOs;

namespace Crm.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);
}
