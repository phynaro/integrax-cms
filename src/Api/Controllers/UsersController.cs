using Api.DTOs;
using Audit.Core.Enums;
using Audit.Core.Interfaces;
using Identity.Core.Entities;
using Identity.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditService _auditService;

    public UsersController(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IAuditService auditService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserListDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _userRepository.GetPagedAsync(page, pageSize, search, ct);
        var dtos = items.Select(MapToListDto).ToList();
        return Ok(ApiResponse<List<UserListDto>>.Success(dtos, page, pageSize, totalCount));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "User not found"));

        return Ok(ApiResponse<UserDto>.Success(MapToDto(user)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser != null)
            return Conflict(ApiErrorResponse.Create("CONFLICT", "User with this email already exists"));

        var role = await _roleRepository.GetByIdAsync(request.RoleId, ct);
        if (role == null)
            return BadRequest(ApiErrorResponse.Create("VALIDATION_ERROR", "Role not found"));

        var user = new User
        {
            KeycloakId = Guid.NewGuid().ToString(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DisplayName = request.DisplayName,
            RoleId = request.RoleId
        };

        await _userRepository.AddAsync(user, ct);
        
        user = await _userRepository.GetByIdAsync(user.Id, ct);
        await _auditService.LogAsync(AuditEventType.Create, "User", user!.Id, null, 
            new { user.Email, user.FirstName, user.LastName, user.RoleId }, ct: ct);

        return CreatedAtAction(nameof(GetById), new { id = user.Id },
            ApiResponse<UserDto>.Success(MapToDto(user)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "User not found"));

        var role = await _roleRepository.GetByIdAsync(request.RoleId, ct);
        if (role == null)
            return BadRequest(ApiErrorResponse.Create("VALIDATION_ERROR", "Role not found"));

        var oldValues = new { user.FirstName, user.LastName, user.DisplayName, user.RoleId };

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.DisplayName = request.DisplayName;
        user.RoleId = request.RoleId;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, ct);
        
        user = await _userRepository.GetByIdAsync(user.Id, ct);
        await _auditService.LogAsync(AuditEventType.Update, "User", user!.Id, oldValues, request, ct: ct);

        return Ok(ApiResponse<UserDto>.Success(MapToDto(user)));
    }

    [HttpPut("{id:guid}/role")]
    public async Task<ActionResult<ApiResponse<UserDto>>> ChangeRole(
        Guid id,
        [FromBody] ChangeRoleRequest request,
        CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "User not found"));

        var role = await _roleRepository.GetByIdAsync(request.RoleId, ct);
        if (role == null)
            return BadRequest(ApiErrorResponse.Create("VALIDATION_ERROR", "Role not found"));

        var oldRoleId = user.RoleId;
        user.RoleId = request.RoleId;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, ct);
        
        user = await _userRepository.GetByIdAsync(user.Id, ct);
        await _auditService.LogAsync(AuditEventType.RoleChange, "User", user!.Id,
            new { RoleId = oldRoleId }, new { RoleId = request.RoleId }, ct: ct);

        return Ok(ApiResponse<UserDto>.Success(MapToDto(user)));
    }

    [HttpPut("{id:guid}/activate")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Activate(Guid id, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "User not found"));

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, ct);
        await _auditService.LogAsync(AuditEventType.UserActivate, "User", user.Id, ct: ct);

        return Ok(ApiResponse<UserDto>.Success(MapToDto(user)));
    }

    [HttpPut("{id:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Deactivate(Guid id, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "User not found"));

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, ct);
        await _auditService.LogAsync(AuditEventType.UserDeactivate, "User", user.Id, ct: ct);

        return Ok(ApiResponse<UserDto>.Success(MapToDto(user)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "User not found"));

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, ct);
        await _auditService.LogAsync(AuditEventType.Delete, "User", user.Id, ct: ct);

        return NoContent();
    }

    private static UserDto MapToDto(User user) => new(
        user.Id,
        user.KeycloakId,
        user.Email,
        user.FirstName,
        user.LastName,
        user.DisplayName,
        user.RoleId,
        new RoleDto(user.Role.Id, user.Role.Name, user.Role.Description, user.Role.Permissions, user.Role.IsSystem),
        user.IsActive,
        user.LastLoginAt,
        user.CreatedAt,
        null,
        user.UpdatedAt
    );

    private static UserListDto MapToListDto(User user) => new(
        user.Id,
        user.Email,
        user.FirstName,
        user.LastName,
        user.DisplayName,
        user.Role?.Name ?? "",
        user.IsActive,
        user.LastLoginAt,
        user.CreatedAt
    );
}
