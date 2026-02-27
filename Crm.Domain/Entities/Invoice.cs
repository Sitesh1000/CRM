using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;

    public Guid? QuoteId { get; set; }
    public Quote? Quote { get; set; }

    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? DueDate { get; set; }

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
