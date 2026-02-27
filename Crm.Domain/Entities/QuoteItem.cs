using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class QuoteItem : BaseEntity
{
    public Guid QuoteId { get; set; }
    public Quote? Quote { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
