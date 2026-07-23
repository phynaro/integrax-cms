using Identity.Core.Entities;
using Imports.Core.Enums;
using Portfolios.Core.Entities;
using Shared.Kernel.Entities;

namespace Imports.Core.Entities;

public class ImportBatch : BaseEntity
{
    public Guid PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;
    public string Filename { get; set; } = string.Empty;
    public int? FileSize { get; set; }
    public int TotalRows { get; set; }
    public int CreatedDebtors { get; set; }
    public int MatchedDebtors { get; set; }
    public int CreatedAccounts { get; set; }
    public int CreatedCases { get; set; }
    public ImportStatus Status { get; set; } = ImportStatus.Completed;
    public string? ErrorMessage { get; set; }
    public DateTime? RolledBackAt { get; set; }
    public Guid? RolledBackById { get; set; }
    public User? RolledBackBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedById { get; set; }
    public User? CreatedBy { get; set; }
}
