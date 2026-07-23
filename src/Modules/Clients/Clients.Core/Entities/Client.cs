using Shared.Kernel.Entities;

namespace Clients.Core.Entities;

public class Client : AuditableEntity
{
    public string? ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}
