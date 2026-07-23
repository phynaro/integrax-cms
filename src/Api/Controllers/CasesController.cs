using Api.DTOs;
using Cases.Core.Entities;
using Cases.Core.Enums;
using Cases.Core.Interfaces;
using Audit.Core.Enums;
using Audit.Core.Interfaces;
using Identity.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Auth;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CasesController : ControllerBase
{
    private readonly ICaseRepository _caseRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public CasesController(
        ICaseRepository caseRepository,
        IUserRepository userRepository,
        IAuditService auditService,
        ICurrentUserService currentUser)
    {
        _caseRepository = caseRepository;
        _userRepository = userRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CaseListDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? assignedToId = null,
        [FromQuery] CaseStatus? status = null,
        [FromQuery] CasePriority? priority = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _caseRepository.GetPagedAsync(
            page, pageSize, assignedToId, status, priority, search, ct);

        var dtos = items.Select(c => new CaseListDto(
            c.Id,
            c.CaseNumber,
            c.DebtAccount?.Debtor?.DisplayName ?? "",
            c.DebtAccount?.AccountNumber ?? "",
            c.DebtAccount?.CurrentBalance ?? 0,
            c.AssignedTo?.FullName,
            c.Status,
            c.Priority,
            c.OpenedAt
        )).ToList();

        return Ok(ApiResponse<List<CaseListDto>>.Success(dtos, page, pageSize, totalCount));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CaseDto>>> GetById(Guid id, CancellationToken ct)
    {
        var caseEntity = await _caseRepository.GetWithDetailsAsync(id, ct);
        if (caseEntity == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Case not found"));

        return Ok(ApiResponse<CaseDto>.Success(MapToDto(caseEntity)));
    }

    [HttpGet("by-case-number/{caseNumber}")]
    public async Task<ActionResult<ApiResponse<CaseDto>>> GetByCaseNumber(string caseNumber, CancellationToken ct)
    {
        var caseEntity = await _caseRepository.GetByCaseNumberAsync(caseNumber, ct);
        if (caseEntity == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Case not found"));

        var fullCase = await _caseRepository.GetWithDetailsAsync(caseEntity.Id, ct);
        return Ok(ApiResponse<CaseDto>.Success(MapToDto(fullCase!)));
    }

    [HttpGet("my-cases")]
    public async Task<ActionResult<ApiResponse<List<CaseListDto>>>> GetMyCases(CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue)
            return Unauthorized(ApiErrorResponse.Create("UNAUTHORIZED", "User not authenticated"));

        var cases = await _caseRepository.GetByAssignedToAsync(userId.Value, ct);

        var dtos = cases.Select(c => new CaseListDto(
            c.Id,
            c.CaseNumber,
            c.DebtAccount?.Debtor?.DisplayName ?? "",
            c.DebtAccount?.AccountNumber ?? "",
            c.DebtAccount?.CurrentBalance ?? 0,
            c.AssignedTo?.FullName,
            c.Status,
            c.Priority,
            c.OpenedAt
        )).ToList();

        return Ok(ApiResponse<List<CaseListDto>>.Success(dtos));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CaseDto>>> Update(
        Guid id,
        [FromBody] UpdateCaseRequest request,
        CancellationToken ct)
    {
        var caseEntity = await _caseRepository.GetByIdAsync(id, ct);
        if (caseEntity == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Case not found"));

        var oldValues = new { caseEntity.Status, caseEntity.Priority, caseEntity.Notes };

        caseEntity.Status = request.Status;
        caseEntity.Priority = request.Priority;
        caseEntity.Notes = request.Notes;

        if (request.Status == CaseStatus.Closed && caseEntity.ClosedAt == null)
            caseEntity.ClosedAt = DateTime.UtcNow;

        await _caseRepository.UpdateAsync(caseEntity, ct);
        await _auditService.LogAsync(AuditEventType.Update, "CollectionCase", caseEntity.Id, oldValues, request, ct: ct);

        var fullCase = await _caseRepository.GetWithDetailsAsync(caseEntity.Id, ct);
        return Ok(ApiResponse<CaseDto>.Success(MapToDto(fullCase!)));
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<ActionResult<ApiResponse<CaseDto>>> Assign(
        Guid id,
        [FromBody] AssignCaseRequest request,
        CancellationToken ct)
    {
        var caseEntity = await _caseRepository.GetByIdAsync(id, ct);
        if (caseEntity == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Case not found"));

        var user = await _userRepository.GetByIdAsync(request.AssignedToId, ct);
        if (user == null)
            return BadRequest(ApiErrorResponse.Create("BAD_REQUEST", "User not found"));

        var oldAssignee = caseEntity.AssignedToId;

        caseEntity.AssignedToId = request.AssignedToId;
        if (caseEntity.Status == CaseStatus.New)
            caseEntity.Status = CaseStatus.InProgress;

        await _caseRepository.UpdateAsync(caseEntity, ct);
        await _auditService.LogAsync(AuditEventType.Update, "CollectionCase", caseEntity.Id, 
            new { AssignedToId = oldAssignee }, 
            new { request.AssignedToId }, 
            ct: ct);

        var fullCase = await _caseRepository.GetWithDetailsAsync(caseEntity.Id, ct);
        return Ok(ApiResponse<CaseDto>.Success(MapToDto(fullCase!)));
    }

    [HttpPost("{id:guid}/unassign")]
    public async Task<ActionResult<ApiResponse<CaseDto>>> Unassign(Guid id, CancellationToken ct)
    {
        var caseEntity = await _caseRepository.GetByIdAsync(id, ct);
        if (caseEntity == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Case not found"));

        var oldAssignee = caseEntity.AssignedToId;

        caseEntity.AssignedToId = null;

        await _caseRepository.UpdateAsync(caseEntity, ct);
        await _auditService.LogAsync(AuditEventType.Update, "CollectionCase", caseEntity.Id,
            new { AssignedToId = oldAssignee },
            new { AssignedToId = (Guid?)null },
            ct: ct);

        var fullCase = await _caseRepository.GetWithDetailsAsync(caseEntity.Id, ct);
        return Ok(ApiResponse<CaseDto>.Success(MapToDto(fullCase!)));
    }

    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<ApiResponse<CaseDto>>> Close(Guid id, CancellationToken ct)
    {
        var caseEntity = await _caseRepository.GetByIdAsync(id, ct);
        if (caseEntity == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Case not found"));

        if (caseEntity.Status == CaseStatus.Closed)
            return BadRequest(ApiErrorResponse.Create("BAD_REQUEST", "Case is already closed"));

        var oldStatus = caseEntity.Status;

        caseEntity.Status = CaseStatus.Closed;
        caseEntity.ClosedAt = DateTime.UtcNow;

        await _caseRepository.UpdateAsync(caseEntity, ct);
        await _auditService.LogAsync(AuditEventType.Update, "CollectionCase", caseEntity.Id,
            new { Status = oldStatus },
            new { Status = CaseStatus.Closed, caseEntity.ClosedAt },
            ct: ct);

        var fullCase = await _caseRepository.GetWithDetailsAsync(caseEntity.Id, ct);
        return Ok(ApiResponse<CaseDto>.Success(MapToDto(fullCase!)));
    }

    [HttpPost("{id:guid}/reopen")]
    public async Task<ActionResult<ApiResponse<CaseDto>>> Reopen(Guid id, CancellationToken ct)
    {
        var caseEntity = await _caseRepository.GetByIdAsync(id, ct);
        if (caseEntity == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Case not found"));

        if (caseEntity.Status != CaseStatus.Closed)
            return BadRequest(ApiErrorResponse.Create("BAD_REQUEST", "Case is not closed"));

        var oldStatus = caseEntity.Status;

        caseEntity.Status = caseEntity.AssignedToId.HasValue ? CaseStatus.InProgress : CaseStatus.New;
        caseEntity.ClosedAt = null;

        await _caseRepository.UpdateAsync(caseEntity, ct);
        await _auditService.LogAsync(AuditEventType.Update, "CollectionCase", caseEntity.Id,
            new { Status = oldStatus },
            new { caseEntity.Status },
            ct: ct);

        var fullCase = await _caseRepository.GetWithDetailsAsync(caseEntity.Id, ct);
        return Ok(ApiResponse<CaseDto>.Success(MapToDto(fullCase!)));
    }

    private static CaseDto MapToDto(CollectionCase c) => new(
        c.Id,
        c.CaseNumber,
        c.DebtAccountId,
        c.DebtAccount?.AccountNumber ?? "",
        c.DebtAccount?.DebtorId ?? Guid.Empty,
        c.DebtAccount?.Debtor?.DisplayName ?? "",
        c.DebtAccount?.CurrentBalance ?? 0,
        c.AssignedToId,
        c.AssignedTo?.FullName,
        c.Status,
        c.Priority,
        c.OpenedAt,
        c.ClosedAt,
        c.Notes,
        c.CreatedAt
    );
}
