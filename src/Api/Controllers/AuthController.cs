using Api.Auth;
using Api.DTOs;
using Identity.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserSyncService _userSyncService;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserSyncService userSyncService,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        ILogger<AuthController> logger)
    {
        _userSyncService = userSyncService;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [HttpPost("sync")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AuthUserDto>>> SyncUser(CancellationToken ct)
    {
        var keycloakId = _currentUserService.KeycloakId;
        var email = _currentUserService.Email;

        if (string.IsNullOrEmpty(keycloakId) || string.IsNullOrEmpty(email))
        {
            return BadRequest(ApiErrorResponse.Create("INVALID_TOKEN", "Invalid token claims"));
        }

        var firstName = User.FindFirst("given_name")?.Value;
        var lastName = User.FindFirst("family_name")?.Value;

        var user = await _userSyncService.SyncUserAsync(keycloakId, email, firstName, lastName, ct);

        _logger.LogInformation("User {UserId} synced from Keycloak", user.Id);

        return Ok(ApiResponse<AuthUserDto>.Success(MapToAuthDto(user)));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CurrentUserDto>>> GetCurrentUser(CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(ApiErrorResponse.Create("UNAUTHORIZED", "User not authenticated"));
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, ct);
        if (user == null)
        {
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "User not found"));
        }

        var dto = new CurrentUserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Role?.Name ?? AuthConstants.Roles.Viewer,
            user.Role?.Permissions?.ToArray() ?? Array.Empty<string>(),
            user.IsActive,
            user.LastLoginAt
        );

        return Ok(ApiResponse<CurrentUserDto>.Success(dto));
    }

    [HttpGet("check")]
    [Authorize]
    public ActionResult<ApiResponse<AuthCheckDto>> CheckAuth()
    {
        var dto = new AuthCheckDto(
            IsAuthenticated: _currentUserService.IsAuthenticated,
            UserId: _currentUserService.UserId,
            Email: _currentUserService.Email,
            Role: _currentUserService.RoleName,
            Permissions: _currentUserService.Permissions.ToList()
        );

        return Ok(ApiResponse<AuthCheckDto>.Success(dto));
    }

    private static AuthUserDto MapToAuthDto(Identity.Core.Entities.User user)
    {
        return new AuthUserDto(
            user.Id,
            user.KeycloakId,
            user.Email,
            user.FirstName,
            user.LastName,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.RoleId,
            user.Role?.Name ?? "",
            user.Role?.Permissions?.ToArray() ?? Array.Empty<string>(),
            user.IsActive,
            user.LastLoginAt,
            user.CreatedAt
        );
    }
}

public record AuthUserDto(
    Guid Id,
    string KeycloakId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    Guid RoleId,
    string RoleName,
    string[] Permissions,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt
);

public record CurrentUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string Role,
    string[] Permissions,
    bool IsActive,
    DateTime? LastLoginAt
);

public record AuthCheckDto(
    bool IsAuthenticated,
    Guid? UserId,
    string? Email,
    string? Role,
    List<string> Permissions
);
