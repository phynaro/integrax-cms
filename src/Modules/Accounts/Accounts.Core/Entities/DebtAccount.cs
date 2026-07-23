using Accounts.Core.Enums;
using Debtors.Core.Entities;
using Portfolios.Core.Entities;
using Shared.Kernel.Entities;

namespace Accounts.Core.Entities;

public class DebtAccount : AuditableEntity
{
    public string? ExternalId { get; set; }
    public Guid DebtorId { get; set; }
    public Debtor Debtor { get; set; } = null!;
    public Guid PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;
    public Guid? ImportBatchId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string? CreditorReference { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal FeesAmount { get; set; }
    public DateOnly? DueDate { get; set; }
    public int DaysPastDue { get; set; }
    public DateOnly? LastPaymentDate { get; set; }
    public decimal? LastPaymentAmount { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.Open;
    public string? Notes { get; set; }
}
