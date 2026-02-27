using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal BasePrice { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<SpecialPrice> SpecialPrices { get; set; } = new List<SpecialPrice>();
    public ICollection<QuoteItem> QuoteItems { get; set; } = new List<QuoteItem>();
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
