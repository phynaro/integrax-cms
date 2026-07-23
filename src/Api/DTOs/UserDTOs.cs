using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    List<string> Permissions,
    bool IsSystem
);

public record UserDto(
    Guid Id,
    string KeycloakId,
    string Email,
    string FirstName,
    string LastName,
    string? DisplayName,
    Guid RoleId,
    RoleDto Role,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    UserRefDto? CreatedBy,
    DateTime? UpdatedAt
);

public record UserListDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? DisplayName,
    string RoleName,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt
);

public record CreateUserRequest
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; init; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; init; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; init; } = string.Empty;
    
    [StringLength(200)]
    public string? DisplayName { get; init; }
    
    [Required]
    public Guid RoleId { get; init; }
}

public record UpdateUserRequest
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; init; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; init; } = string.Empty;
    
    [StringLength(200)]
    public string? DisplayName { get; init; }
    
    [Required]
    public Guid RoleId { get; init; }
}

public record ChangeRoleRequest
{
    [Required]
    public Guid RoleId { get; init; }
}
