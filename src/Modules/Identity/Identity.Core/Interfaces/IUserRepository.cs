using Identity.Core.Entities;
using Shared.Kernel.Interfaces;

namespace Identity.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken ct = default);
    Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, CancellationToken ct = default);
}
