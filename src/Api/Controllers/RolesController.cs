using Api.DTOs;
using Identity.Core.Entities;
using Identity.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;

    public RolesController(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetAll(CancellationToken ct)
    {
        var roles = await _roleRepository.GetAllAsync(ct);
        var dtos = roles.Select(MapToDto).ToList();
        return Ok(ApiResponse<List<RoleDto>>.Success(dtos));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(Guid id, CancellationToken ct)
    {
        var role = await _roleRepository.GetByIdAsync(id, ct);
        if (role == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Role not found"));

        return Ok(ApiResponse<RoleDto>.Success(MapToDto(role)));
    }

    private static RoleDto MapToDto(Role role) => new(
        role.Id,
        role.Name,
        role.Description,
        role.Permissions,
        role.IsSystem
    );
}
