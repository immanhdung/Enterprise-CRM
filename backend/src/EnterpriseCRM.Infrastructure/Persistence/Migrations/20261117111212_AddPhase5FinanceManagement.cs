using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseCRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase5FinanceManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_requests_contracts_ContractId",
                table: "invoice_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_invoice_requests_payments_PaymentId",
                table: "invoice_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_invoices_invoice_requests_InvoiceRequestId",
                table: "invoices");

            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_quotations_QuotationNumber\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_payments_TransactionReference\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_invoices_InvoiceRequestId\";");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionReference",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledById",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConfirmedById",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmedByName",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentNumber",
                table: "payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReversalReason",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "payments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MilestoneTitle",
                table: "payment_schedules",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "payment_schedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Percentage",
                table: "payment_schedules",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNumber",
                table: "invoices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContractId",
                table: "invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CustomerAddress",
                table: "invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "invoices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerTaxCode",
                table: "invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceSeries",
                table: "invoices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LookupCode",
                table: "invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LookupUrl",
                table: "invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "invoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "invoices",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "invoice_requests",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "invoice_requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "invoice_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestDate",
                table: "invoice_requests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RequestedByName",
                table: "invoice_requests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "financial_reconciliation_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReconciliationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalContractValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalExpectedRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCollectedRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalInvoicedRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscrepancyAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ReconciledById = table.Column<Guid>(type: "uuid", nullable: false),
                    ReconciledByName = table.Column<string>(type: "text", nullable: false),
                    ApprovedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByName = table.Column<string>(type: "text", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_reconciliation_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "invoice_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invoice_items_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invoice_items_services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "receivable_collection_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActivityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    LoggedById = table.Column<Guid>(type: "uuid", nullable: false),
                    LoggedByName = table.Column<string>(type: "text", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextFollowUpDate = table.Column<DateOnly>(type: "date", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedToUserName = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receivable_collection_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_receivable_collection_logs_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_receivable_collection_logs_contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_receivable_collection_logs_payment_schedules_PaymentSchedul~",
                        column: x => x.PaymentScheduleId,
                        principalTable: "payment_schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReconciliationRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiscrepancyType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExpectedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Difference = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ResolutionStatus = table.Column<string>(type: "text", nullable: false),
                    ResolutionNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reconciliation_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reconciliation_items_contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_reconciliation_items_financial_reconciliation_records_Recon~",
                        column: x => x.ReconciliationRecordId,
                        principalTable: "financial_reconciliation_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reconciliation_items_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_reconciliation_items_payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS \"IX_quotations_QuotationNumber_RevisionNumber\" ON quotations (\"QuotationNumber\", \"RevisionNumber\");");

            migrationBuilder.CreateIndex(
                name: "IX_payments_AccountId",
                table: "payments",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_PaymentNumber",
                table: "payments",
                column: "PaymentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_AccountId",
                table: "invoices",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_ContractId",
                table: "invoices",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_InvoiceRequestId",
                table: "invoices",
                column: "InvoiceRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_requests_AccountId",
                table: "invoice_requests",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_items_InvoiceId",
                table: "invoice_items",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_items_ServiceId",
                table: "invoice_items",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_receivable_collection_logs_AccountId",
                table: "receivable_collection_logs",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_receivable_collection_logs_ContractId",
                table: "receivable_collection_logs",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_receivable_collection_logs_PaymentScheduleId",
                table: "receivable_collection_logs",
                column: "PaymentScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_reconciliation_items_ContractId",
                table: "reconciliation_items",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_reconciliation_items_InvoiceId",
                table: "reconciliation_items",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_reconciliation_items_PaymentId",
                table: "reconciliation_items",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_reconciliation_items_ReconciliationRecordId",
                table: "reconciliation_items",
                column: "ReconciliationRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_requests_accounts_AccountId",
                table: "invoice_requests",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_requests_contracts_ContractId",
                table: "invoice_requests",
                column: "ContractId",
                principalTable: "contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_requests_payments_PaymentId",
                table: "invoice_requests",
                column: "PaymentId",
                principalTable: "payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_accounts_AccountId",
                table: "invoices",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_contracts_ContractId",
                table: "invoices",
                column: "ContractId",
                principalTable: "contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_invoice_requests_InvoiceRequestId",
                table: "invoices",
                column: "InvoiceRequestId",
                principalTable: "invoice_requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_accounts_AccountId",
                table: "payments",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invoice_requests_accounts_AccountId",
                table: "invoice_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_invoice_requests_contracts_ContractId",
                table: "invoice_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_invoice_requests_payments_PaymentId",
                table: "invoice_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_invoices_accounts_AccountId",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_invoices_contracts_ContractId",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_invoices_invoice_requests_InvoiceRequestId",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_accounts_AccountId",
                table: "payments");

            migrationBuilder.DropTable(
                name: "invoice_items");

            migrationBuilder.DropTable(
                name: "receivable_collection_logs");

            migrationBuilder.DropTable(
                name: "reconciliation_items");

            migrationBuilder.DropTable(
                name: "financial_reconciliation_records");

            migrationBuilder.DropIndex(
                name: "IX_quotations_QuotationNumber_RevisionNumber",
                table: "quotations");

            migrationBuilder.DropIndex(
                name: "IX_payments_AccountId",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payments_PaymentNumber",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_invoices_AccountId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_ContractId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_InvoiceRequestId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoice_requests_AccountId",
                table: "invoice_requests");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "CancelledById",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "ConfirmedById",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "ConfirmedByName",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "PaymentNumber",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "ReversalReason",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "MilestoneTitle",
                table: "payment_schedules");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "payment_schedules");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "payment_schedules");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "CustomerAddress",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "CustomerTaxCode",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceSeries",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "LookupCode",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "LookupUrl",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "invoice_requests");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "invoice_requests");

            migrationBuilder.DropColumn(
                name: "RequestDate",
                table: "invoice_requests");

            migrationBuilder.DropColumn(
                name: "RequestedByName",
                table: "invoice_requests");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionReference",
                table: "payments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNumber",
                table: "invoices",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "invoice_requests",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.CreateIndex(
                name: "IX_quotations_QuotationNumber",
                table: "quotations",
                column: "QuotationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_TransactionReference",
                table: "payments",
                column: "TransactionReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_InvoiceRequestId",
                table: "invoices",
                column: "InvoiceRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_requests_contracts_ContractId",
                table: "invoice_requests",
                column: "ContractId",
                principalTable: "contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_invoice_requests_payments_PaymentId",
                table: "invoice_requests",
                column: "PaymentId",
                principalTable: "payments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_invoice_requests_InvoiceRequestId",
                table: "invoices",
                column: "InvoiceRequestId",
                principalTable: "invoice_requests",
                principalColumn: "Id");
        }
    }
}
