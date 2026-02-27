using Crm.Domain.Common;
using Crm.Domain.Enums;

namespace Crm.Domain.Entities;

public class Deal : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DealStage Stage { get; set; } = DealStage.Qualification;
    public DateTime? ExpectedCloseDate { get; set; }

    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }

    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
