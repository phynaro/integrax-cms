using Audit.Core.Enums;

namespace Api.DTOs;

public record AuditEventDto(
    Guid Id,
    AuditEventType EventType,
    string? EntityType,
    Guid? EntityId,
    Guid? UserId,
    string? UserEmail,
    string? IpAddress,
    string? UserAgent,
    object? OldValues,
    object? NewValues,
    object? Metadata,
    DateTime CreatedAt
);
