using Api.Data;
using Identity.Core.Entities;
using Identity.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public override async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId, ct);
    }

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, CancellationToken ct = default)
    {
        var query = _dbSet.Include(u => u.Role).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(u => 
                u.Email.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
