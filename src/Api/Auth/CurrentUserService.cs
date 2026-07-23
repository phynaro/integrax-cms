using System.Security.Claims;

namespace Api.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var claim = User?.FindFirst("userId")?.Value 
                     ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirst("email")?.Value 
                         ?? User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? KeycloakId => User?.FindFirst("sub")?.Value;

    public string? RoleName => User?.FindFirst("role")?.Value 
                            ?? User?.FindFirst(ClaimTypes.Role)?.Value;

    public IEnumerable<string> Permissions
    {
        get
        {
            var permissionsClaim = User?.FindFirst("permissions")?.Value;
            if (string.IsNullOrEmpty(permissionsClaim))
                return Enumerable.Empty<string>();
            return permissionsClaim.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool HasPermission(string permission)
    {
        if (!IsAuthenticated)
            return false;

        var perms = Permissions.ToList();
        return perms.Contains(AuthConstants.Permissions.All) || perms.Contains(permission);
    }
}
