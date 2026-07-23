using Identity.Core.Entities;
using Identity.Core.Interfaces;

namespace Api.Auth;

public interface IUserSyncService
{
    Task<User> SyncUserAsync(
        string keycloakId,
        string email,
        string? firstName,
        string? lastName,
        string? preferredRoleName = null,
        CancellationToken ct = default);
}

public class UserSyncService : IUserSyncService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public UserSyncService(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<User> SyncUserAsync(
        string keycloakId,
        string email,
        string? firstName,
        string? lastName,
        string? preferredRoleName = null,
        CancellationToken ct = default)
    {
        var roleName = string.IsNullOrWhiteSpace(preferredRoleName)
            ? AuthConstants.Roles.Viewer
            : preferredRoleName;

        var role = await _roleRepository.GetByNameAsync(roleName, ct)
            ?? await _roleRepository.GetByNameAsync(AuthConstants.Roles.Viewer, ct)
            ?? throw new InvalidOperationException("Default role not found");

        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId, ct);

        if (user == null)
        {
            user = new User
            {
                KeycloakId = keycloakId,
                Email = email,
                FirstName = firstName ?? email.Split('@')[0],
                LastName = lastName ?? "",
                RoleId = role.Id,
                LastLoginAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, ct);
            user = await _userRepository.GetByIdAsync(user.Id, ct);
        }
        else
        {
            user.LastLoginAt = DateTime.UtcNow;
            if (user.RoleId != role.Id)
            {
                user.RoleId = role.Id;
            }
            await _userRepository.UpdateAsync(user, ct);
            user = await _userRepository.GetByIdAsync(user.Id, ct);
        }

        return user!;
    }
}
