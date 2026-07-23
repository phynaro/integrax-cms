using Api.DTOs;
using Debtors.Core.Enums;

namespace Api.Services;

public interface ICsvParserService
{
    Task<ImportValidationResult> ParseAndValidateAsync(Stream fileStream, string filename);
}

public class CsvParserService : ICsvParserService
{
    private static readonly string[] RequiredHeaders = new[]
    {
        "external_id", "debtor_type", "account_number", "original_amount", "current_balance"
    };

    public async Task<ImportValidationResult> ParseAndValidateAsync(Stream fileStream, string filename)
    {
        var rows = new List<ImportRowPreview>();
        var errors = new List<ImportValidationError>();
        var validRows = 0;
        var errorRows = 0;

        using var reader = new StreamReader(fileStream);
        var headerLine = await reader.ReadLineAsync();
        
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return new ImportValidationResult(
                false, filename, 0, 0, 0, rows,
                new List<ImportValidationError> { new(0, "file", "File is empty or has no header row") }
            );
        }

        var headers = ParseCsvLine(headerLine).Select(h => h.ToLowerInvariant().Trim()).ToArray();
        var headerMap = new Dictionary<string, int>();
        for (int i = 0; i < headers.Length; i++)
        {
            headerMap[headers[i]] = i;
        }

        var missingHeaders = RequiredHeaders.Where(h => !headerMap.ContainsKey(h)).ToList();
        if (missingHeaders.Any())
        {
            return new ImportValidationResult(
                false, filename, 0, 0, 0, rows,
                new List<ImportValidationError> { new(0, "headers", $"Missing required headers: {string.Join(", ", missingHeaders)}") }
            );
        }

        var rowNumber = 1;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            rowNumber++;
            var values = ParseCsvLine(line);
            var rowErrors = new List<string>();

            var row = new ImportRowPreview
            {
                RowNumber = rowNumber,
                ExternalId = GetValue(values, headerMap, "external_id"),
                DebtorType = ParseDebtorType(GetValue(values, headerMap, "debtor_type"), rowErrors),
                FirstName = GetValue(values, headerMap, "first_name"),
                LastName = GetValue(values, headerMap, "last_name"),
                CompanyName = GetValue(values, headerMap, "company_name"),
                DateOfBirth = ParseDate(GetValue(values, headerMap, "date_of_birth")),
                TaxId = GetValue(values, headerMap, "tax_id"),
                Phone = GetValue(values, headerMap, "phone"),
                Email = GetValue(values, headerMap, "email"),
                AddressLine1 = GetValue(values, headerMap, "address_line1"),
                AddressLine2 = GetValue(values, headerMap, "address_line2"),
                City = GetValue(values, headerMap, "city"),
                State = GetValue(values, headerMap, "state"),
                PostalCode = GetValue(values, headerMap, "postal_code"),
                Country = GetValue(values, headerMap, "country"),
                AccountNumber = GetValue(values, headerMap, "account_number"),
                CreditorReference = GetValue(values, headerMap, "creditor_reference"),
                OriginalAmount = ParseDecimal(GetValue(values, headerMap, "original_amount"), "original_amount", rowErrors),
                CurrentBalance = ParseDecimal(GetValue(values, headerMap, "current_balance"), "current_balance", rowErrors),
                InterestAmount = ParseDecimal(GetValue(values, headerMap, "interest_amount"), "interest_amount", rowErrors, allowEmpty: true),
                FeesAmount = ParseDecimal(GetValue(values, headerMap, "fees_amount"), "fees_amount", rowErrors, allowEmpty: true),
                DueDate = ParseDateOnly(GetValue(values, headerMap, "due_date")),
                DaysPastDue = ParseInt(GetValue(values, headerMap, "days_past_due"), "days_past_due", rowErrors, allowEmpty: true),
                LastPaymentDate = ParseDateOnly(GetValue(values, headerMap, "last_payment_date")),
                LastPaymentAmount = ParseNullableDecimal(GetValue(values, headerMap, "last_payment_amount")),
                AccountNotes = GetValue(values, headerMap, "account_notes"),
                IsValid = true,
                Errors = rowErrors
            };

            ValidateRow(row, rowErrors);

            var rowPreview = row with { IsValid = rowErrors.Count == 0, Errors = rowErrors };
            rows.Add(rowPreview);

            if (rowErrors.Count == 0)
                validRows++;
            else
            {
                errorRows++;
                errors.AddRange(rowErrors.Select(e => new ImportValidationError(rowNumber, "row", e)));
            }
        }

        return new ImportValidationResult(
            errorRows == 0,
            filename,
            rows.Count,
            validRows,
            errorRows,
            rows,
            errors
        );
    }

    private static void ValidateRow(ImportRowPreview row, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(row.ExternalId))
            errors.Add("external_id is required");

        if (string.IsNullOrWhiteSpace(row.AccountNumber))
            errors.Add("account_number is required");

        if (row.DebtorType == DebtorType.Individual)
        {
            if (string.IsNullOrWhiteSpace(row.FirstName) && string.IsNullOrWhiteSpace(row.LastName))
                errors.Add("first_name or last_name is required for Individual debtor");
        }
        else if (row.DebtorType == DebtorType.Company)
        {
            if (string.IsNullOrWhiteSpace(row.CompanyName))
                errors.Add("company_name is required for Company debtor");
        }

        if (row.OriginalAmount <= 0)
            errors.Add("original_amount must be greater than 0");

        if (row.CurrentBalance < 0)
            errors.Add("current_balance cannot be negative");
    }

    private static string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var inQuotes = false;
        var currentValue = "";

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentValue += '"';
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.Trim());
                currentValue = "";
            }
            else
            {
                currentValue += c;
            }
        }
        values.Add(currentValue.Trim());

        return values.ToArray();
    }

    private static string? GetValue(string[] values, Dictionary<string, int> headerMap, string header)
    {
        if (!headerMap.TryGetValue(header, out var index) || index >= values.Length)
            return null;
        var value = values[index];
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static DebtorType ParseDebtorType(string? value, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DebtorType.Individual;

        if (Enum.TryParse<DebtorType>(value, true, out var result))
            return result;

        if (value.Equals("person", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("individual", StringComparison.OrdinalIgnoreCase))
            return DebtorType.Individual;

        if (value.Equals("company", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("business", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("organization", StringComparison.OrdinalIgnoreCase))
            return DebtorType.Company;

        errors.Add($"Invalid debtor_type: {value}");
        return DebtorType.Individual;
    }

    private static decimal ParseDecimal(string? value, string fieldName, List<string> errors, bool allowEmpty = false)
    {
        if (string.IsNullOrWhiteSpace(value))
            return allowEmpty ? 0 : 0;

        if (decimal.TryParse(value.Replace("$", "").Replace(",", ""), out var result))
            return result;

        if (!allowEmpty)
            errors.Add($"Invalid {fieldName}: {value}");
        return 0;
    }

    private static decimal? ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (decimal.TryParse(value.Replace("$", "").Replace(",", ""), out var result))
            return result;

        return null;
    }

    private static int ParseInt(string? value, string fieldName, List<string> errors, bool allowEmpty = false)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        if (int.TryParse(value, out var result))
            return result;

        if (!allowEmpty)
            errors.Add($"Invalid {fieldName}: {value}");
        return 0;
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateTime.TryParse(value, out var result))
            return result;

        return null;
    }

    private static DateOnly? ParseDateOnly(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateOnly.TryParse(value, out var result))
            return result;

        return null;
    }
}
