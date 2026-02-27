using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public decimal Amount { get; set; }
    public DateTime PaidOn { get; set; } = DateTime.UtcNow;
    public string? Method { get; set; }
    public string? Reference { get; set; }
}
