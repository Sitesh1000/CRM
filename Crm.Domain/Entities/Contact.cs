using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class Contact : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
