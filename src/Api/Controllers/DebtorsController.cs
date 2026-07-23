using Api.Data;
using Api.DTOs;
using Audit.Core.Enums;
using Audit.Core.Interfaces;
using Debtors.Core.Entities;
using Debtors.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DebtorsController : ControllerBase
{
    private readonly IDebtorRepository _debtorRepository;
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;

    public DebtorsController(
        IDebtorRepository debtorRepository,
        AppDbContext context,
        IAuditService auditService)
    {
        _debtorRepository = debtorRepository;
        _context = context;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DebtorListDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _debtorRepository.GetPagedAsync(page, pageSize, search, ct);
        
        var debtorIds = items.Select(d => d.Id).ToList();
        var accountCounts = await _context.DebtAccounts
            .Where(a => debtorIds.Contains(a.DebtorId))
            .GroupBy(a => a.DebtorId)
            .Select(g => new { DebtorId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DebtorId, x => x.Count, ct);

        var dtos = items.Select(d => new DebtorListDto(
            d.Id,
            d.ExternalId,
            d.DebtorType,
            d.DisplayName,
            d.IsActive,
            d.CreatedAt,
            d.Contacts.Count,
            accountCounts.GetValueOrDefault(d.Id, 0)
        )).ToList();

        return Ok(ApiResponse<List<DebtorListDto>>.Success(dtos, page, pageSize, totalCount));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DebtorDto>>> GetById(Guid id, CancellationToken ct)
    {
        var debtor = await _debtorRepository.GetWithDetailsAsync(id, ct);
        if (debtor == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Debtor not found"));

        return Ok(ApiResponse<DebtorDto>.Success(MapToDto(debtor)));
    }

    [HttpGet("by-external/{externalId}")]
    public async Task<ActionResult<ApiResponse<DebtorDto>>> GetByExternalId(string externalId, CancellationToken ct)
    {
        var debtor = await _debtorRepository.GetByExternalIdAsync(externalId, ct);
        if (debtor == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Debtor not found"));

        var fullDebtor = await _debtorRepository.GetWithDetailsAsync(debtor.Id, ct);
        return Ok(ApiResponse<DebtorDto>.Success(MapToDto(fullDebtor!)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DebtorDto>>> Create(
        [FromBody] CreateDebtorRequest request,
        CancellationToken ct)
    {
        var existing = await _debtorRepository.GetByExternalIdAsync(request.ExternalId, ct);
        if (existing != null)
            return Conflict(ApiErrorResponse.Create("CONFLICT", "Debtor with this external ID already exists"));

        var debtor = new Debtor
        {
            ExternalId = request.ExternalId,
            DebtorType = request.DebtorType,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CompanyName = request.CompanyName,
            DateOfBirth = request.DateOfBirth,
            TaxId = request.TaxId,
            Notes = request.Notes
        };

        await _debtorRepository.AddAsync(debtor, ct);
        await _auditService.LogAsync(AuditEventType.Create, "Debtor", debtor.Id, null, debtor, ct: ct);

        var fullDebtor = await _debtorRepository.GetWithDetailsAsync(debtor.Id, ct);
        return CreatedAtAction(nameof(GetById), new { id = debtor.Id },
            ApiResponse<DebtorDto>.Success(MapToDto(fullDebtor!)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DebtorDto>>> Update(
        Guid id,
        [FromBody] UpdateDebtorRequest request,
        CancellationToken ct)
    {
        var debtor = await _debtorRepository.GetByIdAsync(id, ct);
        if (debtor == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Debtor not found"));

        if (debtor.ExternalId != request.ExternalId)
        {
            var existing = await _debtorRepository.GetByExternalIdAsync(request.ExternalId, ct);
            if (existing != null)
                return Conflict(ApiErrorResponse.Create("CONFLICT", "Debtor with this external ID already exists"));
        }

        var oldValues = new { debtor.ExternalId, debtor.DebtorType, debtor.FirstName, debtor.LastName, debtor.CompanyName };

        debtor.ExternalId = request.ExternalId;
        debtor.DebtorType = request.DebtorType;
        debtor.FirstName = request.FirstName;
        debtor.LastName = request.LastName;
        debtor.CompanyName = request.CompanyName;
        debtor.DateOfBirth = request.DateOfBirth;
        debtor.TaxId = request.TaxId;
        debtor.Notes = request.Notes;

        await _debtorRepository.UpdateAsync(debtor, ct);
        await _auditService.LogAsync(AuditEventType.Update, "Debtor", debtor.Id, oldValues, request, ct: ct);

        var fullDebtor = await _debtorRepository.GetWithDetailsAsync(debtor.Id, ct);
        return Ok(ApiResponse<DebtorDto>.Success(MapToDto(fullDebtor!)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var debtor = await _debtorRepository.GetByIdAsync(id, ct);
        if (debtor == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Debtor not found"));

        debtor.IsActive = false;
        await _debtorRepository.UpdateAsync(debtor, ct);
        await _auditService.LogAsync(AuditEventType.Delete, "Debtor", debtor.Id, ct: ct);

        return NoContent();
    }

    [HttpPost("{id:guid}/contacts")]
    public async Task<ActionResult<ApiResponse<DebtorContactDto>>> AddContact(
        Guid id,
        [FromBody] CreateContactRequest request,
        CancellationToken ct)
    {
        var debtor = await _debtorRepository.GetWithDetailsAsync(id, ct);
        if (debtor == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Debtor not found"));

        if (request.IsPrimary)
        {
            foreach (var c in debtor.Contacts.Where(c => c.Type == request.Type && c.IsPrimary))
                c.IsPrimary = false;
        }

        var contact = new DebtorContact
        {
            DebtorId = id,
            Type = request.Type,
            Label = request.Label,
            Value = request.Value,
            IsPrimary = request.IsPrimary
        };

        _context.DebtorContacts.Add(contact);
        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse<DebtorContactDto>.Success(MapContactToDto(contact)));
    }

    [HttpPut("{id:guid}/contacts/{contactId:guid}")]
    public async Task<ActionResult<ApiResponse<DebtorContactDto>>> UpdateContact(
        Guid id,
        Guid contactId,
        [FromBody] UpdateContactRequest request,
        CancellationToken ct)
    {
        var debtor = await _debtorRepository.GetWithDetailsAsync(id, ct);
        if (debtor == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Debtor not found"));

        var contact = debtor.Contacts.FirstOrDefault(c => c.Id == contactId);
        if (contact == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Contact not found"));

        if (request.IsPrimary && !contact.IsPrimary)
        {
            foreach (var c in debtor.Contacts.Where(c => c.Type == request.Type && c.IsPrimary))
                c.IsPrimary = false;
        }

        contact.Type = request.Type;
        contact.Label = request.Label;
        contact.Value = request.Value;
        contact.IsPrimary = request.IsPrimary;
        contact.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse<DebtorContactDto>.Success(MapContactToDto(contact)));
    }

    [HttpDelete("{id:guid}/contacts/{contactId:guid}")]
    public async Task<ActionResult> DeleteContact(Guid id, Guid contactId, CancellationToken ct)
    {
        var contact = await _context.DebtorContacts.FirstOrDefaultAsync(c => c.Id == contactId && c.DebtorId == id, ct);
        if (contact == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Contact not found"));

        _context.DebtorContacts.Remove(contact);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("{id:guid}/addresses")]
    public async Task<ActionResult<ApiResponse<DebtorAddressDto>>> AddAddress(
        Guid id,
        [FromBody] CreateAddressRequest request,
        CancellationToken ct)
    {
        var debtor = await _debtorRepository.GetWithDetailsAsync(id, ct);
        if (debtor == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Debtor not found"));

        if (request.IsPrimary)
        {
            foreach (var a in debtor.Addresses.Where(a => a.IsPrimary))
                a.IsPrimary = false;
        }

        var address = new DebtorAddress
        {
            DebtorId = id,
            Label = request.Label,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            IsPrimary = request.IsPrimary
        };

        _context.DebtorAddresses.Add(address);
        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse<DebtorAddressDto>.Success(MapAddressToDto(address)));
    }

    [HttpPut("{id:guid}/addresses/{addressId:guid}")]
    public async Task<ActionResult<ApiResponse<DebtorAddressDto>>> UpdateAddress(
        Guid id,
        Guid addressId,
        [FromBody] UpdateAddressRequest request,
        CancellationToken ct)
    {
        var debtor = await _debtorRepository.GetWithDetailsAsync(id, ct);
        if (debtor == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Debtor not found"));

        var address = debtor.Addresses.FirstOrDefault(a => a.Id == addressId);
        if (address == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Address not found"));

        if (request.IsPrimary && !address.IsPrimary)
        {
            foreach (var a in debtor.Addresses.Where(a => a.IsPrimary))
                a.IsPrimary = false;
        }

        address.Label = request.Label;
        address.AddressLine1 = request.AddressLine1;
        address.AddressLine2 = request.AddressLine2;
        address.City = request.City;
        address.State = request.State;
        address.PostalCode = request.PostalCode;
        address.Country = request.Country;
        address.IsPrimary = request.IsPrimary;
        address.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return Ok(ApiResponse<DebtorAddressDto>.Success(MapAddressToDto(address)));
    }

    [HttpDelete("{id:guid}/addresses/{addressId:guid}")]
    public async Task<ActionResult> DeleteAddress(Guid id, Guid addressId, CancellationToken ct)
    {
        var address = await _context.DebtorAddresses.FirstOrDefaultAsync(a => a.Id == addressId && a.DebtorId == id, ct);
        if (address == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Address not found"));

        _context.DebtorAddresses.Remove(address);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    private static DebtorDto MapToDto(Debtor debtor) => new(
        debtor.Id,
        debtor.ExternalId,
        debtor.DebtorType,
        debtor.FirstName,
        debtor.LastName,
        debtor.CompanyName,
        debtor.DisplayName,
        debtor.DateOfBirth,
        debtor.TaxId,
        debtor.Notes,
        debtor.IsActive,
        debtor.CreatedAt,
        debtor.Contacts.Select(MapContactToDto).ToList(),
        debtor.Addresses.Select(MapAddressToDto).ToList()
    );

    private static DebtorContactDto MapContactToDto(DebtorContact contact) => new(
        contact.Id,
        contact.Type,
        contact.Label,
        contact.Value,
        contact.IsPrimary
    );

    private static DebtorAddressDto MapAddressToDto(DebtorAddress address) => new(
        address.Id,
        address.Label,
        address.AddressLine1,
        address.AddressLine2,
        address.City,
        address.State,
        address.PostalCode,
        address.Country,
        address.IsPrimary
    );
}
