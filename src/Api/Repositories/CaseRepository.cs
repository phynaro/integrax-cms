using Api.Data;
using Cases.Core.Entities;
using Cases.Core.Enums;
using Cases.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class CaseRepository : BaseRepository<CollectionCase>, ICaseRepository
{
    public CaseRepository(AppDbContext context) : base(context) { }

    public async Task<CollectionCase?> GetByCaseNumberAsync(string caseNumber, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.CaseNumber == caseNumber, ct);
    }

    public async Task<CollectionCase?> GetByDebtAccountIdAsync(Guid debtAccountId, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.DebtAccountId == debtAccountId, ct);
    }

    public async Task<CollectionCase?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.DebtAccount)
                .ThenInclude(a => a.Debtor)
            .Include(x => x.DebtAccount)
                .ThenInclude(a => a.Portfolio)
            .Include(x => x.AssignedTo)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<CollectionCase>> GetByAssignedToAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.DebtAccount)
                .ThenInclude(a => a.Debtor)
            .Where(x => x.AssignedToId == userId && x.Status != CaseStatus.Closed)
            .OrderBy(x => x.Priority)
            .ThenByDescending(x => x.OpenedAt)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<CollectionCase> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? assignedToId = null, CaseStatus? status = null,
        CasePriority? priority = null, string? search = null, CancellationToken ct = default)
    {
        var query = _dbSet
            .Include(x => x.DebtAccount)
                .ThenInclude(a => a.Debtor)
            .Include(x => x.AssignedTo)
            .AsQueryable();

        if (assignedToId.HasValue)
            query = query.Where(x => x.AssignedToId == assignedToId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(x => x.Priority == priority.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(x => 
                x.CaseNumber.ToLower().Contains(search) ||
                x.DebtAccount.Debtor.DisplayName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.OpenedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<string> GenerateCaseNumberAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"CASE-{today}-";
        
        var lastCase = await _dbSet
            .Where(x => x.CaseNumber.StartsWith(prefix))
            .OrderByDescending(x => x.CaseNumber)
            .FirstOrDefaultAsync(ct);

        if (lastCase == null)
            return $"{prefix}00001";

        var lastNumber = int.Parse(lastCase.CaseNumber.Substring(prefix.Length));
        return $"{prefix}{(lastNumber + 1):D5}";
    }

    public override async Task<CollectionCase> AddAsync(CollectionCase entity, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(entity.CaseNumber))
            entity.CaseNumber = await GenerateCaseNumberAsync(ct);
            
        return await base.AddAsync(entity, ct);
    }
}
