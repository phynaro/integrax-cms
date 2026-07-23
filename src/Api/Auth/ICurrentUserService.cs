namespace Api.Auth;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? KeycloakId { get; }
    string? RoleName { get; }
    IEnumerable<string> Permissions { get; }
    bool IsAuthenticated { get; }
    bool HasPermission(string permission);
}
