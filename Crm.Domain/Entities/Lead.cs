using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class Lead : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CompanyName { get; set; }
    public string? Source { get; set; }

    public bool IsConverted { get; set; }
    public Guid? ConvertedContactId { get; set; }
    public Contact? ConvertedContact { get; set; }
}
