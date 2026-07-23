using System.ComponentModel.DataAnnotations;
using Cases.Core.Enums;

namespace Api.DTOs;

public record CaseDto(
    Guid Id,
    string CaseNumber,
    Guid DebtAccountId,
    string AccountNumber,
    Guid DebtorId,
    string DebtorDisplayName,
    decimal CurrentBalance,
    Guid? AssignedToId,
    string? AssignedToName,
    CaseStatus Status,
    CasePriority Priority,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    string? Notes,
    DateTime CreatedAt
);

public record CaseListDto(
    Guid Id,
    string CaseNumber,
    string DebtorDisplayName,
    string AccountNumber,
    decimal CurrentBalance,
    string? AssignedToName,
    CaseStatus Status,
    CasePriority Priority,
    DateTime OpenedAt
);

public record UpdateCaseRequest
{
    public CaseStatus Status { get; init; }
    public CasePriority Priority { get; init; }
    public string? Notes { get; init; }
}

public record AssignCaseRequest
{
    [Required]
    public Guid AssignedToId { get; init; }
}
