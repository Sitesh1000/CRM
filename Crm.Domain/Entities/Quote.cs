using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class Quote : BaseEntity
{
    public string QuoteNumber { get; set; } = string.Empty;

    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public Guid? DealId { get; set; }
    public Deal? Deal { get; set; }

    public decimal TotalAmount { get; set; }

    public ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
}
