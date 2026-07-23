using Api.Data;
using Clients.Core.Entities;
using Clients.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class ClientRepository : BaseRepository<Client>, IClientRepository
{
    public ClientRepository(AppDbContext context) : base(context) { }

    public async Task<Client?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Code == code, ct);
    }

    public async Task<Client?> GetByExternalIdAsync(string externalId, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.ExternalId == externalId, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _dbSet.Where(c => c.Code == code);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task<(IReadOnlyList<Client> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, bool? isActive = null, CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c => 
                c.Name.ToLower().Contains(search) ||
                c.Code.ToLower().Contains(search) ||
                (c.ContactName != null && c.ContactName.ToLower().Contains(search)));
        }

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
