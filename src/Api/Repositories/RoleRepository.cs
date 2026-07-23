using Api.Data;
using Identity.Core.Entities;
using Identity.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class RoleRepository : BaseRepository<Role>, IRoleRepository
{
    public RoleRepository(AppDbContext context) : base(context) { }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Name == name, ct);
    }
}
