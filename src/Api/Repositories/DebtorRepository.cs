using Api.Data;
using Debtors.Core.Entities;
using Debtors.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class DebtorRepository : BaseRepository<Debtor>, IDebtorRepository
{
    public DebtorRepository(AppDbContext context) : base(context) { }

    public async Task<Debtor?> GetByExternalIdAsync(string externalId, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.ExternalId == externalId, ct);
    }

    public async Task<Debtor?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Contacts)
            .Include(x => x.Addresses)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Debtor> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(x => 
                x.DisplayName.ToLower().Contains(search) || 
                x.ExternalId.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public override async Task<Debtor> AddAsync(Debtor entity, CancellationToken ct = default)
    {
        entity.UpdateDisplayName();
        return await base.AddAsync(entity, ct);
    }

    public override async Task UpdateAsync(Debtor entity, CancellationToken ct = default)
    {
        entity.UpdateDisplayName();
        await base.UpdateAsync(entity, ct);
    }
}
