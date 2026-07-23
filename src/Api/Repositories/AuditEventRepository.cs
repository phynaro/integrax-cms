using Api.Data;
using Audit.Core.Entities;
using Audit.Core.Enums;
using Audit.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class AuditEventRepository : BaseRepository<AuditEvent>, IAuditEventRepository
{
    public AuditEventRepository(AppDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<AuditEvent> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        AuditEventType? eventType = null,
        string? entityType = null,
        Guid? entityId = null,
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();

        if (eventType.HasValue)
            query = query.Where(e => e.EventType == eventType.Value);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(e => e.EntityType == entityType);

        if (entityId.HasValue)
            query = query.Where(e => e.EntityId == entityId.Value);

        if (userId.HasValue)
            query = query.Where(e => e.UserId == userId.Value);

        if (fromDate.HasValue)
            query = query.Where(e => e.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.CreatedAt <= toDate.Value.AddDays(1));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<AuditEvent>> GetEntityHistoryAsync(
        string entityType, Guid entityId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => e.EntityType == entityType && e.EntityId == entityId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AuditEvent>> GetUserActivityAsync(
        Guid userId, int limit = 100, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }
}
