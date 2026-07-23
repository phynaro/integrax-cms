using Api.DTOs;
using Accounts.Core.Entities;
using Accounts.Core.Enums;
using Accounts.Core.Interfaces;
using Audit.Core.Enums;
using Audit.Core.Interfaces;
using Debtors.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolios.Core.Interfaces;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DebtAccountsController : ControllerBase
{
    private readonly IDebtAccountRepository _accountRepository;
    private readonly IDebtorRepository _debtorRepository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IAuditService _auditService;

    public DebtAccountsController(
        IDebtAccountRepository accountRepository,
        IDebtorRepository debtorRepository,
        IPortfolioRepository portfolioRepository,
        IAuditService auditService)
    {
        _accountRepository = accountRepository;
        _debtorRepository = debtorRepository;
        _portfolioRepository = portfolioRepository;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DebtAccountListDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? portfolioId = null,
        [FromQuery] Guid? debtorId = null,
        [FromQuery] AccountStatus? status = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _accountRepository.GetPagedAsync(
            page, pageSize, portfolioId, debtorId, status, search, ct);

        var dtos = items.Select(a => new DebtAccountListDto(
            a.Id,
            a.DebtorId,
            a.Debtor?.DisplayName ?? "",
            a.PortfolioId,
            a.Portfolio?.Name ?? "",
            a.AccountNumber,
            a.OriginalAmount,
            a.CurrentBalance,
            a.Status,
            a.IsActive,
            a.CreatedAt
        )).ToList();

        return Ok(ApiResponse<List<DebtAccountListDto>>.Success(dtos, page, pageSize, totalCount));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DebtAccountDto>>> GetById(Guid id, CancellationToken ct)
    {
        var account = await _accountRepository.GetWithDetailsAsync(id, ct);
        if (account == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Account not found"));

        return Ok(ApiResponse<DebtAccountDto>.Success(MapToDto(account)));
    }

    [HttpGet("by-account-number/{accountNumber}")]
    public async Task<ActionResult<ApiResponse<DebtAccountDto>>> GetByAccountNumber(string accountNumber, CancellationToken ct)
    {
        var account = await _accountRepository.GetByAccountNumberAsync(accountNumber, ct);
        if (account == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Account not found"));

        var fullAccount = await _accountRepository.GetWithDetailsAsync(account.Id, ct);
        return Ok(ApiResponse<DebtAccountDto>.Success(MapToDto(fullAccount!)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DebtAccountDto>>> Create(
        [FromBody] CreateDebtAccountRequest request,
        CancellationToken ct)
    {
        var debtor = await _debtorRepository.GetByIdAsync(request.DebtorId, ct);
        if (debtor == null)
            return BadRequest(ApiErrorResponse.Create("BAD_REQUEST", "Debtor not found"));

        var portfolio = await _portfolioRepository.GetByIdAsync(request.PortfolioId, ct);
        if (portfolio == null)
            return BadRequest(ApiErrorResponse.Create("BAD_REQUEST", "Portfolio not found"));

        if (await _accountRepository.AccountNumberExistsAsync(request.AccountNumber, ct))
            return Conflict(ApiErrorResponse.Create("CONFLICT", "Account number already exists"));

        var account = new DebtAccount
        {
            ExternalId = request.ExternalId,
            DebtorId = request.DebtorId,
            PortfolioId = request.PortfolioId,
            AccountNumber = request.AccountNumber,
            CreditorReference = request.CreditorReference,
            OriginalAmount = request.OriginalAmount,
            CurrentBalance = request.CurrentBalance,
            InterestAmount = request.InterestAmount,
            FeesAmount = request.FeesAmount,
            DueDate = request.DueDate,
            DaysPastDue = request.DaysPastDue,
            Status = AccountStatus.Open,
            Notes = request.Notes
        };

        await _accountRepository.AddAsync(account, ct);
        await _auditService.LogAsync(AuditEventType.Create, "DebtAccount", account.Id, null, account, ct: ct);

        var fullAccount = await _accountRepository.GetWithDetailsAsync(account.Id, ct);
        return CreatedAtAction(nameof(GetById), new { id = account.Id },
            ApiResponse<DebtAccountDto>.Success(MapToDto(fullAccount!)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DebtAccountDto>>> Update(
        Guid id,
        [FromBody] UpdateDebtAccountRequest request,
        CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(id, ct);
        if (account == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Account not found"));

        var oldValues = new { account.CurrentBalance, account.Status, account.Notes };

        account.ExternalId = request.ExternalId;
        account.CreditorReference = request.CreditorReference;
        account.CurrentBalance = request.CurrentBalance;
        account.InterestAmount = request.InterestAmount;
        account.FeesAmount = request.FeesAmount;
        account.DueDate = request.DueDate;
        account.DaysPastDue = request.DaysPastDue;
        account.Status = request.Status;
        account.Notes = request.Notes;

        await _accountRepository.UpdateAsync(account, ct);
        await _auditService.LogAsync(AuditEventType.Update, "DebtAccount", account.Id, oldValues, request, ct: ct);

        var fullAccount = await _accountRepository.GetWithDetailsAsync(account.Id, ct);
        return Ok(ApiResponse<DebtAccountDto>.Success(MapToDto(fullAccount!)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(id, ct);
        if (account == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Account not found"));

        account.IsActive = false;
        await _accountRepository.UpdateAsync(account, ct);
        await _auditService.LogAsync(AuditEventType.Delete, "DebtAccount", account.Id, ct: ct);

        return NoContent();
    }

    [HttpPost("{id:guid}/record-payment")]
    public async Task<ActionResult<ApiResponse<DebtAccountDto>>> RecordPayment(
        Guid id,
        [FromBody] RecordPaymentRequest request,
        CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(id, ct);
        if (account == null)
            return NotFound(ApiErrorResponse.Create("NOT_FOUND", "Account not found"));

        var oldBalance = account.CurrentBalance;

        account.CurrentBalance -= request.Amount;
        account.LastPaymentDate = request.PaymentDate;
        account.LastPaymentAmount = request.Amount;

        if (account.CurrentBalance <= 0)
        {
            account.CurrentBalance = 0;
            account.Status = AccountStatus.Paid;
        }

        await _accountRepository.UpdateAsync(account, ct);
        await _auditService.LogAsync(AuditEventType.Update, "DebtAccount", account.Id, 
            new { OldBalance = oldBalance }, 
            new { request.Amount, request.PaymentDate, NewBalance = account.CurrentBalance }, 
            ct: ct);

        var fullAccount = await _accountRepository.GetWithDetailsAsync(account.Id, ct);
        return Ok(ApiResponse<DebtAccountDto>.Success(MapToDto(fullAccount!)));
    }

    private static DebtAccountDto MapToDto(DebtAccount account) => new(
        account.Id,
        account.ExternalId,
        account.DebtorId,
        account.Debtor?.DisplayName ?? "",
        account.PortfolioId,
        account.Portfolio?.Name ?? "",
        account.ImportBatchId,
        account.AccountNumber,
        account.CreditorReference,
        account.OriginalAmount,
        account.CurrentBalance,
        account.InterestAmount,
        account.FeesAmount,
        account.DueDate,
        account.DaysPastDue,
        account.LastPaymentDate,
        account.LastPaymentAmount,
        account.Status,
        account.Notes,
        account.IsActive,
        account.CreatedAt
    );
}

public record RecordPaymentRequest
{
    public decimal Amount { get; init; }
    public DateOnly PaymentDate { get; init; }
}
