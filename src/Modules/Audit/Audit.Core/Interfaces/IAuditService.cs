using Audit.Core.Enums;

namespace Audit.Core.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditEventType eventType, string? entityType = null, Guid? entityId = null,
        object? oldValues = null, object? newValues = null, string? metadata = null,
        CancellationToken ct = default);
    
    Task LogLoginAsync(Guid userId, string email, bool success, CancellationToken ct = default);
}
