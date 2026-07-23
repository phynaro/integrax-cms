using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "debtors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DebtorType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CompanyName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_debtors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "import_batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Filename = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileSize = table.Column<int>(type: "integer", nullable: true),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    CreatedDebtors = table.Column<int>(type: "integer", nullable: false),
                    MatchedDebtors = table.Column<int>(type: "integer", nullable: false),
                    CreatedAccounts = table.Column<int>(type: "integer", nullable: false),
                    CreatedCases = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RolledBackAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RolledBackById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_batches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_import_batches_portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_import_batches_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_import_batches_users_RolledBackById",
                        column: x => x.RolledBackById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "debt_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DebtorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccountNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreditorReference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OriginalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InterestAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FeesAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DaysPastDue = table.Column<int>(type: "integer", nullable: false),
                    LastPaymentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LastPaymentAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_debt_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_debt_accounts_debtors_DebtorId",
                        column: x => x.DebtorId,
                        principalTable: "debtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_debt_accounts_portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "debtor_addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DebtorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AddressLine1 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AddressLine2 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_debtor_addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_debtor_addresses_debtors_DebtorId",
                        column: x => x.DebtorId,
                        principalTable: "debtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "debtor_contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DebtorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_debtor_contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_debtor_contacts_debtors_DebtorId",
                        column: x => x.DebtorId,
                        principalTable: "debtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "collection_cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DebtAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collection_cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_collection_cases_debt_accounts_DebtAccountId",
                        column: x => x.DebtAccountId,
                        principalTable: "debt_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_collection_cases_users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Permissions",
                value: new List<string> { "*" });

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "Permissions",
                value: new List<string> { "clients:read", "clients:write", "portfolios:read", "portfolios:write", "users:read", "audit:read" });

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "Permissions",
                value: new List<string> { "clients:read", "portfolios:read" });

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "Permissions",
                value: new List<string> { "clients:read", "portfolios:read" });

            migrationBuilder.CreateIndex(
                name: "IX_collection_cases_AssignedToId",
                table: "collection_cases",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_collection_cases_CaseNumber",
                table: "collection_cases",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_collection_cases_DebtAccountId",
                table: "collection_cases",
                column: "DebtAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_collection_cases_Status",
                table: "collection_cases",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_debt_accounts_AccountNumber",
                table: "debt_accounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_debt_accounts_DebtorId",
                table: "debt_accounts",
                column: "DebtorId");

            migrationBuilder.CreateIndex(
                name: "IX_debt_accounts_ImportBatchId",
                table: "debt_accounts",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_debt_accounts_PortfolioId",
                table: "debt_accounts",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_debt_accounts_Status",
                table: "debt_accounts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_debtor_addresses_DebtorId",
                table: "debtor_addresses",
                column: "DebtorId");

            migrationBuilder.CreateIndex(
                name: "IX_debtor_contacts_DebtorId",
                table: "debtor_contacts",
                column: "DebtorId");

            migrationBuilder.CreateIndex(
                name: "IX_debtors_DebtorType",
                table: "debtors",
                column: "DebtorType");

            migrationBuilder.CreateIndex(
                name: "IX_debtors_DisplayName",
                table: "debtors",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_debtors_ExternalId",
                table: "debtors",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_CreatedAt",
                table: "import_batches",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_CreatedById",
                table: "import_batches",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_PortfolioId",
                table: "import_batches",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_RolledBackById",
                table: "import_batches",
                column: "RolledBackById");

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_Status",
                table: "import_batches",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "collection_cases");

            migrationBuilder.DropTable(
                name: "debtor_addresses");

            migrationBuilder.DropTable(
                name: "debtor_contacts");

            migrationBuilder.DropTable(
                name: "import_batches");

            migrationBuilder.DropTable(
                name: "debt_accounts");

            migrationBuilder.DropTable(
                name: "debtors");

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Permissions",
                value: new List<string> { "*" });

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "Permissions",
                value: new List<string> { "clients:read", "clients:write", "portfolios:read", "portfolios:write", "users:read", "audit:read" });

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "Permissions",
                value: new List<string> { "clients:read", "portfolios:read" });

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "Permissions",
                value: new List<string> { "clients:read", "portfolios:read" });
        }
    }
}
