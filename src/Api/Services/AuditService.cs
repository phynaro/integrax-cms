using System.Text.Json;
using Api.Data;
using Audit.Core.Entities;
using Audit.Core.Enums;
using Audit.Core.Interfaces;

namespace Api.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        AuditEventType eventType,
        string? entityType = null,
        Guid? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? metadata = null,
        CancellationToken ct = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = GetCurrentUserId();
        var userEmail = GetCurrentUserEmail();

        var auditEvent = new AuditEvent
        {
            EventType = eventType,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            UserEmail = userEmail,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            Metadata = metadata
        };

        _context.AuditEvents.Add(auditEvent);
        await _context.SaveChangesAsync(ct);
    }

    public async Task LogLoginAsync(Guid userId, string email, bool success, CancellationToken ct = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var auditEvent = new AuditEvent
        {
            EventType = success ? AuditEventType.Login : AuditEventType.LoginFailed,
            UserId = userId,
            UserEmail = email,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString()
        };

        _context.AuditEvents.Add(auditEvent);
        await _context.SaveChangesAsync(ct);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private string? GetCurrentUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst("email")?.Value;
    }
}
