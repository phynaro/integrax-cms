using Audit.Core.Enums;
using Shared.Kernel.Entities;

namespace Audit.Core.Entities;

public class AuditEvent : BaseEntity
{
    public AuditEventType EventType { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
