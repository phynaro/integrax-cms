using Api.Data;
using Microsoft.EntityFrameworkCore;
using Portfolios.Core.Entities;
using Portfolios.Core.Enums;
using Portfolios.Core.Interfaces;

namespace Api.Repositories;

public class PortfolioRepository : BaseRepository<Portfolio>, IPortfolioRepository
{
    public PortfolioRepository(AppDbContext context) : base(context) { }

    public override async Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Portfolio?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Code == code, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _dbSet.Where(p => p.Code == code);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task<(IReadOnlyList<Portfolio> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        Guid? clientId = null,
        PortfolioStatus? status = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var query = _dbSet.Include(p => p.Client).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Code.ToLower().Contains(search));
        }

        if (clientId.HasValue)
            query = query.Where(p => p.ClientId == clientId.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Portfolio>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(p => p.Client)
            .Where(p => p.ClientId == clientId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }
}
