using Audit.Core.Entities;
using Audit.Core.Enums;
using Shared.Kernel.Interfaces;

namespace Audit.Core.Interfaces;

public interface IAuditEventRepository : IRepository<AuditEvent>
{
    Task<(IReadOnlyList<AuditEvent> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        AuditEventType? eventType = null,
        string? entityType = null,
        Guid? entityId = null,
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default);
    
    Task<IReadOnlyList<AuditEvent>> GetEntityHistoryAsync(
        string entityType, Guid entityId, CancellationToken ct = default);
    
    Task<IReadOnlyList<AuditEvent>> GetUserActivityAsync(
        Guid userId, int limit = 100, CancellationToken ct = default);
}
