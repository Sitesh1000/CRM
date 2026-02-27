using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class SpecialPrice : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }

    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public decimal Price { get; set; }
}
