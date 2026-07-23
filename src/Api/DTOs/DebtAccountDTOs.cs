using System.ComponentModel.DataAnnotations;
using Accounts.Core.Enums;

namespace Api.DTOs;

public record DebtAccountDto(
    Guid Id,
    string? ExternalId,
    Guid DebtorId,
    string DebtorDisplayName,
    Guid PortfolioId,
    string PortfolioName,
    Guid? ImportBatchId,
    string AccountNumber,
    string? CreditorReference,
    decimal OriginalAmount,
    decimal CurrentBalance,
    decimal InterestAmount,
    decimal FeesAmount,
    DateOnly? DueDate,
    int DaysPastDue,
    DateOnly? LastPaymentDate,
    decimal? LastPaymentAmount,
    AccountStatus Status,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt
);

public record DebtAccountListDto(
    Guid Id,
    Guid DebtorId,
    string DebtorDisplayName,
    Guid PortfolioId,
    string PortfolioName,
    string AccountNumber,
    decimal OriginalAmount,
    decimal CurrentBalance,
    AccountStatus Status,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateDebtAccountRequest
{
    [StringLength(255)]
    public string? ExternalId { get; init; }

    [Required]
    public Guid DebtorId { get; init; }

    [Required]
    public Guid PortfolioId { get; init; }

    [Required]
    [StringLength(100)]
    public string AccountNumber { get; init; } = string.Empty;

    [StringLength(255)]
    public string? CreditorReference { get; init; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal OriginalAmount { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal CurrentBalance { get; init; }

    [Range(0, double.MaxValue)]
    public decimal InterestAmount { get; init; }

    [Range(0, double.MaxValue)]
    public decimal FeesAmount { get; init; }

    public DateOnly? DueDate { get; init; }

    [Range(0, int.MaxValue)]
    public int DaysPastDue { get; init; }

    public string? Notes { get; init; }
}

public record UpdateDebtAccountRequest
{
    [StringLength(255)]
    public string? ExternalId { get; init; }

    [StringLength(255)]
    public string? CreditorReference { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal CurrentBalance { get; init; }

    [Range(0, double.MaxValue)]
    public decimal InterestAmount { get; init; }

    [Range(0, double.MaxValue)]
    public decimal FeesAmount { get; init; }

    public DateOnly? DueDate { get; init; }

    [Range(0, int.MaxValue)]
    public int DaysPastDue { get; init; }

    public AccountStatus Status { get; init; }

    public string? Notes { get; init; }
}
