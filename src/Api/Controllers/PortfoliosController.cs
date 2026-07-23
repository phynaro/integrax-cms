using Api.DTOs;
using Audit.Core.Enums;
using Audit.Core.Interfaces;
using Clients.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Portfolios.Core.Entities;
using Portfolios.Core.Enums;
using Portfolios.Core.Interfaces;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PortfoliosController : ControllerBase
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IAuditService _auditService;

    public PortfoliosController(
        IPortfolioRepository portfolioRepository,
        IClientRepository clientRepository,
        IAuditService auditService)
    {
        _portfolioRepository = portfolioRepository;
        _clientRepository = clientRepository;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PortfolioListDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? clientId = null,
        [FromQuery] PortfolioStatus? status = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _portfolioRepository.GetPagedAsync(
            page, pageSize, search, clientId, status, isActive, ct);
        var dtos = items.Select(MapToListDto).ToList();
        return Ok(ApiResponse<List<PortfolioListDto>>.Success(dtos, page, pageSize, totalCount));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PortfolioListDto>>> GetById(Guid id, CancellationToken ct)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(id, ct);
        if (portfolio == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Portfolio not found"));

        return Ok(ApiResponse<PortfolioListDto>.Success(MapToListDto(portfolio)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PortfolioListDto>>> Create(
        [FromBody] CreatePortfolioRequest request,
        CancellationToken ct)
    {
        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct);
        if (client == null)
            return BadRequest(ApiErrorResponse.Create("VALIDATION_ERROR", "Client not found"));

        if (await _portfolioRepository.CodeExistsAsync(request.Code, null, ct))
            return Conflict(ApiErrorResponse.Create("CONFLICT", "Portfolio code already exists"));

        var portfolio = new Portfolio
        {
            ExternalId = request.ExternalId,
            ClientId = request.ClientId,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            ReceivedDate = request.ReceivedDate,
            Status = request.Status
        };

        await _portfolioRepository.AddAsync(portfolio, ct);
        
        portfolio = await _portfolioRepository.GetByIdAsync(portfolio.Id, ct);
        await _auditService.LogAsync(AuditEventType.Create, "Portfolio", portfolio!.Id, null, portfolio, ct: ct);

        return CreatedAtAction(nameof(GetById), new { id = portfolio.Id },
            ApiResponse<PortfolioListDto>.Success(MapToListDto(portfolio)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PortfolioListDto>>> Update(
        Guid id,
        [FromBody] UpdatePortfolioRequest request,
        CancellationToken ct)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(id, ct);
        if (portfolio == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Portfolio not found"));

        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct);
        if (client == null)
            return BadRequest(ApiErrorResponse.Create("VALIDATION_ERROR", "Client not found"));

        if (await _portfolioRepository.CodeExistsAsync(request.Code, id, ct))
            return Conflict(ApiErrorResponse.Create("CONFLICT", "Portfolio code already exists"));

        var oldValues = new { portfolio.Name, portfolio.Code, portfolio.Status };

        portfolio.ExternalId = request.ExternalId;
        portfolio.ClientId = request.ClientId;
        portfolio.Name = request.Name;
        portfolio.Code = request.Code;
        portfolio.Description = request.Description;
        portfolio.ReceivedDate = request.ReceivedDate;
        portfolio.Status = request.Status;
        portfolio.UpdatedAt = DateTime.UtcNow;

        await _portfolioRepository.UpdateAsync(portfolio, ct);
        
        portfolio = await _portfolioRepository.GetByIdAsync(portfolio.Id, ct);
        await _auditService.LogAsync(AuditEventType.Update, "Portfolio", portfolio!.Id, oldValues, request, ct: ct);

        return Ok(ApiResponse<PortfolioListDto>.Success(MapToListDto(portfolio)));
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<PortfolioListDto>>> ChangeStatus(
        Guid id,
        [FromBody] ChangePortfolioStatusRequest request,
        CancellationToken ct)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(id, ct);
        if (portfolio == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Portfolio not found"));

        var oldStatus = portfolio.Status;
        portfolio.Status = request.Status;
        portfolio.UpdatedAt = DateTime.UtcNow;

        await _portfolioRepository.UpdateAsync(portfolio, ct);
        await _auditService.LogAsync(AuditEventType.Update, "Portfolio", portfolio.Id, 
            new { Status = oldStatus }, new { Status = request.Status }, ct: ct);

        return Ok(ApiResponse<PortfolioListDto>.Success(MapToListDto(portfolio)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(id, ct);
        if (portfolio == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Portfolio not found"));

        portfolio.IsActive = false;
        portfolio.UpdatedAt = DateTime.UtcNow;
        await _portfolioRepository.UpdateAsync(portfolio, ct);
        await _auditService.LogAsync(AuditEventType.Delete, "Portfolio", portfolio.Id, ct: ct);

        return NoContent();
    }

    private static PortfolioListDto MapToListDto(Portfolio p) => new(
        p.Id,
        p.ExternalId,
        p.ClientId,
        p.Client?.Name ?? "",
        p.Client?.Code ?? "",
        p.Name,
        p.Code,
        p.Status,
        p.TotalAccounts,
        p.TotalAmount,
        p.ReceivedDate,
        p.IsActive,
        p.CreatedAt
    );
}
