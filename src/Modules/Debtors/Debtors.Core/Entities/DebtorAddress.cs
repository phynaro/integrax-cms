using Debtors.Core.Enums;
using Shared.Kernel.Entities;

namespace Debtors.Core.Entities;

public class DebtorAddress : BaseEntity
{
    public Guid DebtorId { get; set; }
    public Debtor Debtor { get; set; } = null!;
    public AddressLabel Label { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
