using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public record ClientDto(
    Guid Id,
    string? ExternalId,
    string Name,
    string Code,
    string? ContactName,
    string? ContactEmail,
    string? ContactPhone,
    string? Address,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt,
    UserRefDto? CreatedBy,
    DateTime? UpdatedAt
);

public record UserRefDto(Guid Id, string DisplayName);

public record CreateClientRequest
{
    public string? ExternalId { get; init; }
    
    [Required]
    [StringLength(255)]
    public string Name { get; init; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Code { get; init; } = string.Empty;
    
    [StringLength(200)]
    public string? ContactName { get; init; }
    
    [EmailAddress]
    [StringLength(255)]
    public string? ContactEmail { get; init; }
    
    [StringLength(50)]
    public string? ContactPhone { get; init; }
    
    public string? Address { get; init; }
    public string? Notes { get; init; }
}

public record UpdateClientRequest : CreateClientRequest;
