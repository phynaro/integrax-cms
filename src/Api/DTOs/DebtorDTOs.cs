using System.ComponentModel.DataAnnotations;
using Debtors.Core.Enums;

namespace Api.DTOs;

public record DebtorDto(
    Guid Id,
    string ExternalId,
    DebtorType DebtorType,
    string? FirstName,
    string? LastName,
    string? CompanyName,
    string DisplayName,
    DateTime? DateOfBirth,
    string? TaxId,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt,
    List<DebtorContactDto> Contacts,
    List<DebtorAddressDto> Addresses
);

public record DebtorListDto(
    Guid Id,
    string ExternalId,
    DebtorType DebtorType,
    string DisplayName,
    bool IsActive,
    DateTime CreatedAt,
    int ContactCount,
    int AccountCount
);

public record DebtorContactDto(
    Guid Id,
    ContactType Type,
    string? Label,
    string Value,
    bool IsPrimary
);

public record DebtorAddressDto(
    Guid Id,
    AddressLabel Label,
    string AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    bool IsPrimary
);

public record CreateDebtorRequest
{
    [Required]
    [StringLength(255)]
    public string ExternalId { get; init; } = string.Empty;

    [Required]
    public DebtorType DebtorType { get; init; }

    [StringLength(100)]
    public string? FirstName { get; init; }

    [StringLength(100)]
    public string? LastName { get; init; }

    [StringLength(255)]
    public string? CompanyName { get; init; }

    public DateTime? DateOfBirth { get; init; }

    [StringLength(50)]
    public string? TaxId { get; init; }

    public string? Notes { get; init; }
}

public record UpdateDebtorRequest : CreateDebtorRequest;

public record CreateContactRequest
{
    [Required]
    public ContactType Type { get; init; }

    [StringLength(50)]
    public string? Label { get; init; }

    [Required]
    [StringLength(255)]
    public string Value { get; init; } = string.Empty;

    public bool IsPrimary { get; init; }
}

public record UpdateContactRequest : CreateContactRequest;

public record CreateAddressRequest
{
    public AddressLabel Label { get; init; }

    [Required]
    [StringLength(255)]
    public string AddressLine1 { get; init; } = string.Empty;

    [StringLength(255)]
    public string? AddressLine2 { get; init; }

    [StringLength(100)]
    public string? City { get; init; }

    [StringLength(100)]
    public string? State { get; init; }

    [StringLength(20)]
    public string? PostalCode { get; init; }

    [StringLength(100)]
    public string? Country { get; init; }

    public bool IsPrimary { get; init; }
}

public record UpdateAddressRequest : CreateAddressRequest;
