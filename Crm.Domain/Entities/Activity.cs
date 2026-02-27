using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class Activity : BaseEntity
{
    public Guid ContactId { get; set; }
    public Contact? Contact { get; set; }

    public Guid? DealId { get; set; }
    public Deal? Deal { get; set; }

    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DueAt { get; set; }
    public bool IsCompleted { get; set; }
}
