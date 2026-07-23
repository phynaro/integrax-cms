using Clients.Core.Entities;
using Portfolios.Core.Enums;
using Shared.Kernel.Entities;

namespace Portfolios.Core.Entities;

public class Portfolio : AuditableEntity
{
    public string? ExternalId { get; set; }
    public Guid ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? ReceivedDate { get; set; }
    public PortfolioStatus Status { get; set; } = PortfolioStatus.Draft;
    public int TotalAccounts { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Metadata { get; set; }

    public virtual Client Client { get; set; } = null!;
}
