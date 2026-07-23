using Identity.Core.Entities;
using Shared.Kernel.Interfaces;

namespace Identity.Core.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
}
