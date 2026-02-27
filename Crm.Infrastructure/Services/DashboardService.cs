using Crm.Application.DTOs;
using Crm.Application.Interfaces;
using Crm.Domain.Enums;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly CrmDbContext _dbContext;

    public DashboardService(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var pipelineValue = await _dbContext.Deals
            .Where(x => x.Stage != DealStage.Won && x.Stage != DealStage.Lost)
            .SumAsync(x => (double?)x.Amount, cancellationToken) ?? 0d;

        var invoicedAmount = await _dbContext.Invoices
            .SumAsync(x => (double?)x.TotalAmount, cancellationToken) ?? 0d;

        var receivedPayments = await _dbContext.Payments
            .SumAsync(x => (double?)x.Amount, cancellationToken) ?? 0d;

        return new DashboardMetricsDto
        {
            TotalCompanies = await _dbContext.Companies.CountAsync(cancellationToken),
            TotalContacts = await _dbContext.Contacts.CountAsync(cancellationToken),
            OpenLeads = await _dbContext.Leads.CountAsync(x => !x.IsConverted, cancellationToken),
            ActiveDeals = await _dbContext.Deals.CountAsync(x => x.Stage != DealStage.Won && x.Stage != DealStage.Lost, cancellationToken),
            PipelineValue = Convert.ToDecimal(pipelineValue),
            InvoicedAmount = Convert.ToDecimal(invoicedAmount),
            ReceivedPayments = Convert.ToDecimal(receivedPayments)
        };
    }
}
