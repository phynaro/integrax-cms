using Api.Auth;
using Api.Data;
using Api.DTOs;
using Accounts.Core.Entities;
using Accounts.Core.Enums;
using Accounts.Core.Interfaces;
using Cases.Core.Entities;
using Cases.Core.Enums;
using Cases.Core.Interfaces;
using Debtors.Core.Entities;
using Debtors.Core.Enums;
using Debtors.Core.Interfaces;
using Imports.Core.Entities;
using Imports.Core.Enums;
using Imports.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public interface IImportService
{
    Task<ImportResult> ProcessImportAsync(ConfirmImportRequest request, CancellationToken ct = default);
    Task<RollbackResult> RollbackImportAsync(Guid importBatchId, CancellationToken ct = default);
    string GenerateCsvTemplate();
}

public class ImportService : IImportService
{
    private readonly AppDbContext _context;
    private readonly IDebtorRepository _debtorRepository;
    private readonly IDebtAccountRepository _accountRepository;
    private readonly ICaseRepository _caseRepository;
    private readonly IImportBatchRepository _importRepository;
    private readonly ICurrentUserService _currentUser;

    public ImportService(
        AppDbContext context,
        IDebtorRepository debtorRepository,
        IDebtAccountRepository accountRepository,
        ICaseRepository caseRepository,
        IImportBatchRepository importRepository,
        ICurrentUserService currentUser)
    {
        _context = context;
        _debtorRepository = debtorRepository;
        _accountRepository = accountRepository;
        _caseRepository = caseRepository;
        _importRepository = importRepository;
        _currentUser = currentUser;
    }

    public async Task<ImportResult> ProcessImportAsync(ConfirmImportRequest request, CancellationToken ct = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            var createdDebtors = 0;
            var matchedDebtors = 0;
            var createdAccounts = 0;
            var createdCases = 0;

            var importBatch = new ImportBatch
            {
                PortfolioId = request.PortfolioId,
                Filename = request.Filename,
                TotalRows = request.Rows.Count,
                Status = ImportStatus.Completed,
                CreatedById = _currentUser.UserId
            };

            _context.ImportBatches.Add(importBatch);
            await _context.SaveChangesAsync(ct);

            foreach (var row in request.Rows.Where(r => r.IsValid))
            {
                var debtor = await _debtorRepository.GetByExternalIdAsync(row.ExternalId!, ct);
                
                if (debtor == null)
                {
                    debtor = new Debtor
                    {
                        ExternalId = row.ExternalId!,
                        DebtorType = row.DebtorType,
                        FirstName = row.FirstName,
                        LastName = row.LastName,
                        CompanyName = row.CompanyName,
                        DateOfBirth = row.DateOfBirth,
                        TaxId = row.TaxId
                    };
                    debtor.UpdateDisplayName();

                    _context.Debtors.Add(debtor);
                    await _context.SaveChangesAsync(ct);

                    if (!string.IsNullOrWhiteSpace(row.Phone))
                    {
                        _context.DebtorContacts.Add(new DebtorContact
                        {
                            DebtorId = debtor.Id,
                            Type = ContactType.Phone,
                            Value = row.Phone,
                            IsPrimary = true
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(row.Email))
                    {
                        _context.DebtorContacts.Add(new DebtorContact
                        {
                            DebtorId = debtor.Id,
                            Type = ContactType.Email,
                            Value = row.Email,
                            IsPrimary = string.IsNullOrWhiteSpace(row.Phone)
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(row.AddressLine1))
                    {
                        _context.DebtorAddresses.Add(new DebtorAddress
                        {
                            DebtorId = debtor.Id,
                            Label = AddressLabel.Home,
                            AddressLine1 = row.AddressLine1,
                            AddressLine2 = row.AddressLine2,
                            City = row.City,
                            State = row.State,
                            PostalCode = row.PostalCode,
                            Country = row.Country,
                            IsPrimary = true
                        });
                    }

                    await _context.SaveChangesAsync(ct);
                    createdDebtors++;
                }
                else
                {
                    matchedDebtors++;
                }

                var account = new DebtAccount
                {
                    DebtorId = debtor.Id,
                    PortfolioId = request.PortfolioId,
                    ImportBatchId = importBatch.Id,
                    AccountNumber = row.AccountNumber!,
                    CreditorReference = row.CreditorReference,
                    OriginalAmount = row.OriginalAmount,
                    CurrentBalance = row.CurrentBalance,
                    InterestAmount = row.InterestAmount,
                    FeesAmount = row.FeesAmount,
                    DueDate = row.DueDate,
                    DaysPastDue = row.DaysPastDue,
                    LastPaymentDate = row.LastPaymentDate,
                    LastPaymentAmount = row.LastPaymentAmount,
                    Status = AccountStatus.Open,
                    Notes = row.AccountNotes
                };

                _context.DebtAccounts.Add(account);
                await _context.SaveChangesAsync(ct);
                createdAccounts++;

                var collectionCase = new CollectionCase
                {
                    DebtAccountId = account.Id,
                    Status = CaseStatus.New,
                    Priority = CasePriority.Medium,
                    OpenedAt = DateTime.UtcNow
                };

                _context.CollectionCases.Add(collectionCase);
                await _context.SaveChangesAsync(ct);
                createdCases++;
            }

            importBatch.CreatedDebtors = createdDebtors;
            importBatch.MatchedDebtors = matchedDebtors;
            importBatch.CreatedAccounts = createdAccounts;
            importBatch.CreatedCases = createdCases;

            _context.ImportBatches.Update(importBatch);
            await _context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            return new ImportResult(
                importBatch.Id,
                request.Rows.Count,
                createdDebtors,
                matchedDebtors,
                createdAccounts,
                createdCases,
                true,
                null
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            return new ImportResult(
                Guid.Empty,
                request.Rows.Count,
                0, 0, 0, 0,
                false,
                $"Import failed: {ex.Message}"
            );
        }
    }

    public async Task<RollbackResult> RollbackImportAsync(Guid importBatchId, CancellationToken ct = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            var importBatch = await _importRepository.GetByIdAsync(importBatchId, ct);
            if (importBatch == null)
            {
                return new RollbackResult(false, 0, 0, 0, "Import batch not found");
            }

            if (importBatch.Status == ImportStatus.RolledBack)
            {
                return new RollbackResult(false, 0, 0, 0, "Import batch has already been rolled back");
            }

            var accounts = await _accountRepository.GetByImportBatchIdAsync(importBatchId, ct);
            var accountIds = accounts.Select(a => a.Id).ToList();
            var debtorIds = accounts.Select(a => a.DebtorId).Distinct().ToList();

            var deletedCases = await _context.CollectionCases
                .Where(c => accountIds.Contains(c.DebtAccountId))
                .ExecuteDeleteAsync(ct);

            var deletedAccounts = await _context.DebtAccounts
                .Where(a => a.ImportBatchId == importBatchId)
                .ExecuteDeleteAsync(ct);

            var debtorsWithOtherAccounts = await _context.DebtAccounts
                .Where(a => debtorIds.Contains(a.DebtorId) && a.ImportBatchId != importBatchId)
                .Select(a => a.DebtorId)
                .Distinct()
                .ToListAsync(ct);

            var debtorsToDelete = debtorIds.Except(debtorsWithOtherAccounts).ToList();

            var deletedContacts = await _context.DebtorContacts
                .Where(c => debtorsToDelete.Contains(c.DebtorId))
                .ExecuteDeleteAsync(ct);

            var deletedAddresses = await _context.DebtorAddresses
                .Where(a => debtorsToDelete.Contains(a.DebtorId))
                .ExecuteDeleteAsync(ct);

            var deletedDebtors = await _context.Debtors
                .Where(d => debtorsToDelete.Contains(d.Id))
                .ExecuteDeleteAsync(ct);

            importBatch.Status = ImportStatus.RolledBack;
            importBatch.RolledBackAt = DateTime.UtcNow;
            importBatch.RolledBackById = _currentUser.UserId;

            _context.ImportBatches.Update(importBatch);
            await _context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            return new RollbackResult(true, deletedCases, deletedAccounts, deletedDebtors, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            return new RollbackResult(false, 0, 0, 0, $"Rollback failed: {ex.Message}");
        }
    }

    public string GenerateCsvTemplate()
    {
        var headers = new[]
        {
            "external_id",
            "debtor_type",
            "first_name",
            "last_name",
            "company_name",
            "date_of_birth",
            "tax_id",
            "phone",
            "email",
            "address_line1",
            "address_line2",
            "city",
            "state",
            "postal_code",
            "country",
            "account_number",
            "creditor_reference",
            "original_amount",
            "current_balance",
            "interest_amount",
            "fees_amount",
            "due_date",
            "days_past_due",
            "last_payment_date",
            "last_payment_amount",
            "account_notes"
        };

        var exampleRow = new[]
        {
            "EXT-001",
            "Individual",
            "John",
            "Doe",
            "",
            "1980-01-15",
            "123-45-6789",
            "+1234567890",
            "john.doe@example.com",
            "123 Main St",
            "Apt 4B",
            "New York",
            "NY",
            "10001",
            "USA",
            "ACC-2024-001",
            "CRED-REF-001",
            "5000.00",
            "4500.00",
            "250.00",
            "100.00",
            "2024-01-15",
            "30",
            "2024-06-01",
            "500.00",
            "Initial import note"
        };

        return string.Join(",", headers) + "\n" + string.Join(",", exampleRow);
    }
}
