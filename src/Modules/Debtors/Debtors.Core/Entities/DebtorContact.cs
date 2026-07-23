using Debtors.Core.Enums;
using Shared.Kernel.Entities;

namespace Debtors.Core.Entities;

public class DebtorContact : BaseEntity
{
    public Guid DebtorId { get; set; }
    public Debtor Debtor { get; set; } = null!;
    public ContactType Type { get; set; }
    public string? Label { get; set; }
    public string Value { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
