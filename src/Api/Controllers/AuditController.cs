using System.Text.Json;
using Api.DTOs;
using Audit.Core.Entities;
using Audit.Core.Enums;
using Audit.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditEventRepository _auditRepository;

    public AuditController(IAuditEventRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AuditEventDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] AuditEventType? eventType = null,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _auditRepository.GetPagedAsync(
            page, pageSize, eventType, entityType, entityId, userId, fromDate, toDate, ct);
        var dtos = items.Select(MapToDto).ToList();
        return Ok(ApiResponse<List<AuditEventDto>>.Success(dtos, page, pageSize, totalCount));
    }

    [HttpGet("entity/{entityType}/{entityId:guid}")]
    public async Task<ActionResult<ApiResponse<List<AuditEventDto>>>> GetEntityHistory(
        string entityType,
        Guid entityId,
        CancellationToken ct)
    {
        var events = await _auditRepository.GetEntityHistoryAsync(entityType, entityId, ct);
        var dtos = events.Select(MapToDto).ToList();
        return Ok(ApiResponse<List<AuditEventDto>>.Success(dtos));
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<List<AuditEventDto>>>> GetUserActivity(
        Guid userId,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        var events = await _auditRepository.GetUserActivityAsync(userId, limit, ct);
        var dtos = events.Select(MapToDto).ToList();
        return Ok(ApiResponse<List<AuditEventDto>>.Success(dtos));
    }

    private static AuditEventDto MapToDto(AuditEvent e) => new(
        e.Id,
        e.EventType,
        e.EntityType,
        e.EntityId,
        e.UserId,
        e.UserEmail,
        e.IpAddress,
        e.UserAgent,
        ParseJson(e.OldValues),
        ParseJson(e.NewValues),
        ParseJson(e.Metadata),
        e.CreatedAt
    );

    private static object? ParseJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            return JsonSerializer.Deserialize<object>(json);
        }
        catch
        {
            return json;
        }
    }
}
