using Api.DTOs;
using Audit.Core.Enums;
using Audit.Core.Interfaces;
using Clients.Core.Entities;
using Clients.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientRepository _clientRepository;
    private readonly IAuditService _auditService;

    public ClientsController(IClientRepository clientRepository, IAuditService auditService)
    {
        _clientRepository = clientRepository;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ClientDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _clientRepository.GetPagedAsync(page, pageSize, search, isActive, ct);
        var dtos = items.Select(MapToDto).ToList();
        return Ok(ApiResponse<List<ClientDto>>.Success(dtos, page, pageSize, totalCount));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ClientDto>>> GetById(Guid id, CancellationToken ct)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct);
        if (client == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Client not found"));

        return Ok(ApiResponse<ClientDto>.Success(MapToDto(client)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ClientDto>>> Create(
        [FromBody] CreateClientRequest request,
        CancellationToken ct)
    {
        if (await _clientRepository.CodeExistsAsync(request.Code, null, ct))
            return Conflict(ApiErrorResponse.Create("CONFLICT", "Client code already exists"));

        var client = new Client
        {
            ExternalId = request.ExternalId,
            Name = request.Name,
            Code = request.Code,
            ContactName = request.ContactName,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            Address = request.Address,
            Notes = request.Notes
        };

        await _clientRepository.AddAsync(client, ct);
        await _auditService.LogAsync(AuditEventType.Create, "Client", client.Id, null, client, ct: ct);

        return CreatedAtAction(nameof(GetById), new { id = client.Id }, 
            ApiResponse<ClientDto>.Success(MapToDto(client)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ClientDto>>> Update(
        Guid id,
        [FromBody] UpdateClientRequest request,
        CancellationToken ct)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct);
        if (client == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Client not found"));

        if (await _clientRepository.CodeExistsAsync(request.Code, id, ct))
            return Conflict(ApiErrorResponse.Create("CONFLICT", "Client code already exists"));

        var oldValues = new { client.Name, client.Code, client.ContactName, client.ContactEmail };

        client.ExternalId = request.ExternalId;
        client.Name = request.Name;
        client.Code = request.Code;
        client.ContactName = request.ContactName;
        client.ContactEmail = request.ContactEmail;
        client.ContactPhone = request.ContactPhone;
        client.Address = request.Address;
        client.Notes = request.Notes;
        client.UpdatedAt = DateTime.UtcNow;

        await _clientRepository.UpdateAsync(client, ct);
        await _auditService.LogAsync(AuditEventType.Update, "Client", client.Id, oldValues, request, ct: ct);

        return Ok(ApiResponse<ClientDto>.Success(MapToDto(client)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct);
        if (client == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Client not found"));

        client.IsActive = false;
        client.UpdatedAt = DateTime.UtcNow;
        await _clientRepository.UpdateAsync(client, ct);
        await _auditService.LogAsync(AuditEventType.Delete, "Client", client.Id, ct: ct);

        return NoContent();
    }

    private static ClientDto MapToDto(Client client) => new(
        client.Id,
        client.ExternalId,
        client.Name,
        client.Code,
        client.ContactName,
        client.ContactEmail,
        client.ContactPhone,
        client.Address,
        client.Notes,
        client.IsActive,
        client.CreatedAt,
        null,
        client.UpdatedAt
    );
}
