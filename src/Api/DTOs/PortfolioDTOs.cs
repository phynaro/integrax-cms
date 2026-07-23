using System.ComponentModel.DataAnnotations;
using Portfolios.Core.Enums;

namespace Api.DTOs;

public record PortfolioDto(
    Guid Id,
    string? ExternalId,
    Guid ClientId,
    ClientDto Client,
    string Name,
    string Code,
    string? Description,
    DateOnly? ReceivedDate,
    PortfolioStatus Status,
    int TotalAccounts,
    decimal TotalAmount,
    string? Metadata,
    bool IsActive,
    DateTime CreatedAt,
    UserRefDto? CreatedBy,
    DateTime? UpdatedAt
);

public record PortfolioListDto(
    Guid Id,
    string? ExternalId,
    Guid ClientId,
    string ClientName,
    string ClientCode,
    string Name,
    string Code,
    PortfolioStatus Status,
    int TotalAccounts,
    decimal TotalAmount,
    DateOnly? ReceivedDate,
    bool IsActive,
    DateTime CreatedAt
);

public record CreatePortfolioRequest
{
    public string? ExternalId { get; init; }
    
    [Required]
    public Guid ClientId { get; init; }
    
    [Required]
    [StringLength(255)]
    public string Name { get; init; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Code { get; init; } = string.Empty;
    
    public string? Description { get; init; }
    public DateOnly? ReceivedDate { get; init; }
    public PortfolioStatus Status { get; init; } = PortfolioStatus.Draft;
}

public record UpdatePortfolioRequest : CreatePortfolioRequest;

public record ChangePortfolioStatusRequest
{
    [Required]
    public PortfolioStatus Status { get; init; }
}
