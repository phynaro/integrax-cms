using System.ComponentModel.DataAnnotations;
using Imports.Core.Enums;
using Debtors.Core.Enums;
using Accounts.Core.Enums;

namespace Api.DTOs;

public record ImportBatchDto(
    Guid Id,
    Guid PortfolioId,
    string PortfolioName,
    string Filename,
    int? FileSize,
    int TotalRows,
    int CreatedDebtors,
    int MatchedDebtors,
    int CreatedAccounts,
    int CreatedCases,
    ImportStatus Status,
    string? ErrorMessage,
    DateTime? RolledBackAt,
    string? RolledBackByName,
    DateTime CreatedAt,
    string? CreatedByName
);

public record ImportBatchListDto(
    Guid Id,
    Guid PortfolioId,
    string PortfolioName,
    string Filename,
    int TotalRows,
    int CreatedAccounts,
    ImportStatus Status,
    DateTime CreatedAt,
    string? CreatedByName
);

public record ValidateImportRequest
{
    [Required]
    public Guid PortfolioId { get; init; }
}

public record ImportValidationResult(
    bool IsValid,
    string Filename,
    int TotalRows,
    int ValidRows,
    int ErrorRows,
    List<ImportRowPreview> Rows,
    List<ImportValidationError> Errors
);

public record ImportRowPreview
{
    public int RowNumber { get; init; }
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
    
    public string? ExternalId { get; init; }
    public DebtorType DebtorType { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? TaxId { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    
    public string? AccountNumber { get; init; }
    public string? CreditorReference { get; init; }
    public decimal OriginalAmount { get; init; }
    public decimal CurrentBalance { get; init; }
    public decimal InterestAmount { get; init; }
    public decimal FeesAmount { get; init; }
    public DateOnly? DueDate { get; init; }
    public int DaysPastDue { get; init; }
    public DateOnly? LastPaymentDate { get; init; }
    public decimal? LastPaymentAmount { get; init; }
    public string? AccountNotes { get; init; }
}

public record ImportValidationError(
    int RowNumber,
    string Field,
    string Message
);

public record ConfirmImportRequest
{
    [Required]
    public Guid PortfolioId { get; init; }
    
    [Required]
    public string Filename { get; init; } = string.Empty;
    
    [Required]
    public List<ImportRowPreview> Rows { get; init; } = new();
}

public record ImportResult(
    Guid ImportBatchId,
    int TotalRows,
    int CreatedDebtors,
    int MatchedDebtors,
    int CreatedAccounts,
    int CreatedCases,
    bool Success,
    string? ErrorMessage
);

public record RollbackResult(
    bool Success,
    int DeletedCases,
    int DeletedAccounts,
    int DeletedDebtors,
    string? ErrorMessage
);
