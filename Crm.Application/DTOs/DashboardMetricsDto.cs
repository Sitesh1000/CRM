namespace Crm.Application.DTOs;

public class DashboardMetricsDto
{
    public int TotalCompanies { get; set; }
    public int TotalContacts { get; set; }
    public int OpenLeads { get; set; }
    public int ActiveDeals { get; set; }
    public decimal PipelineValue { get; set; }
    public decimal InvoicedAmount { get; set; }
    public decimal ReceivedPayments { get; set; }
}
