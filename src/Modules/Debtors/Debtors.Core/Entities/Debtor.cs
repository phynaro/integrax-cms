using Debtors.Core.Enums;
using Shared.Kernel.Entities;

namespace Debtors.Core.Entities;

public class Debtor : AuditableEntity
{
    public string ExternalId { get; set; } = string.Empty;
    public DebtorType DebtorType { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? TaxId { get; set; }
    public string? Notes { get; set; }

    public ICollection<DebtorContact> Contacts { get; set; } = new List<DebtorContact>();
    public ICollection<DebtorAddress> Addresses { get; set; } = new List<DebtorAddress>();

    public void UpdateDisplayName()
    {
        DisplayName = DebtorType == DebtorType.Company
            ? CompanyName ?? string.Empty
            : $"{FirstName} {LastName}".Trim();
    }
}
