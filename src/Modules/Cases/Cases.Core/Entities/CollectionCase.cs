using Accounts.Core.Entities;
using Cases.Core.Enums;
using Identity.Core.Entities;
using Shared.Kernel.Entities;

namespace Cases.Core.Entities;

public class CollectionCase : AuditableEntity
{
    public string CaseNumber { get; set; } = string.Empty;
    public Guid DebtAccountId { get; set; }
    public DebtAccount DebtAccount { get; set; } = null!;
    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
    public CaseStatus Status { get; set; } = CaseStatus.New;
    public CasePriority Priority { get; set; } = CasePriority.Medium;
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public string? Notes { get; set; }
}
