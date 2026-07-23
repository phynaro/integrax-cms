using Api.DTOs;
using Api.Services;
using Imports.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolios.Core.Interfaces;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ImportsController : ControllerBase
{
    private readonly IImportBatchRepository _importRepository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ICsvParserService _csvParser;
    private readonly IImportService _importService;

    public ImportsController(
        IImportBatchRepository importRepository,
        IPortfolioRepository portfolioRepository,
        ICsvParserService csvParser,
        IImportService importService)
    {
        _importRepository = importRepository;
        _portfolioRepository = portfolioRepository;
        _csvParser = csvParser;
        _importService = importService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ImportBatchListDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? portfolioId = null,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _importRepository.GetPagedAsync(page, pageSize, portfolioId, null, ct);

        var dtos = items.Select(i => new ImportBatchListDto(
            i.Id,
            i.PortfolioId,
            i.Portfolio?.Name ?? "",
            i.Filename,
            i.TotalRows,
            i.CreatedAccounts,
            i.Status,
            i.CreatedAt,
            i.CreatedBy?.FullName
        )).ToList();

        return Ok(ApiResponse<List<ImportBatchListDto>>.Success(dtos, page, pageSize, totalCount));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ImportBatchDto>>> GetById(Guid id, CancellationToken ct)
    {
        var import = await _importRepository.GetWithDetailsAsync(id, ct);
        if (import == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Import batch not found"));

        return Ok(ApiResponse<ImportBatchDto>.Success(new ImportBatchDto(
            import.Id,
            import.PortfolioId,
            import.Portfolio?.Name ?? "",
            import.Filename,
            import.FileSize,
            import.TotalRows,
            import.CreatedDebtors,
            import.MatchedDebtors,
            import.CreatedAccounts,
            import.CreatedCases,
            import.Status,
            import.ErrorMessage,
            import.RolledBackAt,
            import.RolledBackBy?.FullName,
            import.CreatedAt,
            import.CreatedBy?.FullName
        )));
    }

    [HttpGet("template")]
    [AllowAnonymous]
    public IActionResult GetTemplate()
    {
        var template = _importService.GenerateCsvTemplate();
        return File(System.Text.Encoding.UTF8.GetBytes(template), "text/csv", "import_template.csv");
    }

    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<ImportValidationResult>>> Validate(
        [FromQuery] Guid portfolioId,
        IFormFile file,
        CancellationToken ct)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(portfolioId, ct);
        if (portfolio == null)
            return BadRequest(ApiErrorResponse.Create("BAD_REQUEST", "Portfolio not found"));

        if (file == null || file.Length == 0)
            return BadRequest(ApiErrorResponse.Create("BAD_REQUEST", "No file provided"));

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiErrorResponse.Create("BAD_REQUEST", "Only CSV files are supported"));

        using var stream = file.OpenReadStream();
        var result = await _csvParser.ParseAndValidateAsync(stream, file.FileName);

        return Ok(ApiResponse<ImportValidationResult>.Success(result));
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<ApiResponse<ImportResult>>> Confirm(
        [FromBody] ConfirmImportRequest request,
        CancellationToken ct)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(request.PortfolioId, ct);
        if (portfolio == null)
            return BadRequest(ApiErrorResponse.Create("BAD_REQUEST", "Portfolio not found"));

        if (request.Rows == null || !request.Rows.Any())
            return BadRequest(ApiErrorResponse.Create("BAD_REQUEST", "No rows to import"));

        var invalidRows = request.Rows.Where(r => !r.IsValid).ToList();
        if (invalidRows.Any())
            return BadRequest(ApiErrorResponse.Create("BAD_REQUEST", $"Cannot import with {invalidRows.Count} invalid rows"));

        var result = await _importService.ProcessImportAsync(request, ct);

        if (!result.Success)
            return BadRequest(ApiErrorResponse.Create("IMPORT_FAILED", result.ErrorMessage ?? "Import failed"));

        return Ok(ApiResponse<ImportResult>.Success(result));
    }

    [HttpPost("{id:guid}/rollback")]
    public async Task<ActionResult<ApiResponse<RollbackResult>>> Rollback(Guid id, CancellationToken ct)
    {
        var import = await _importRepository.GetByIdAsync(id, ct);
        if (import == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Import batch not found"));

        var result = await _importService.RollbackImportAsync(id, ct);

        if (!result.Success)
            return BadRequest(ApiErrorResponse.Create("ROLLBACK_FAILED", result.ErrorMessage ?? "Rollback failed"));

        return Ok(ApiResponse<RollbackResult>.Success(result));
    }
}
