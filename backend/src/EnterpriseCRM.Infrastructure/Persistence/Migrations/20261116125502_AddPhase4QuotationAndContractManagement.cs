using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseCRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase4QuotationAndContractManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_contract_items_services_ServiceId",
                table: "contract_items");

            migrationBuilder.DropForeignKey(
                name: "FK_contracts_accounts_AccountId1",
                table: "contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_contracts_quotations_QuotationId",
                table: "contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_quotation_items_services_ServiceId",
                table: "quotation_items");

            migrationBuilder.DropForeignKey(
                name: "FK_quotations_deals_DealId",
                table: "quotations");

            migrationBuilder.RenameColumn(
                name: "AccountId1",
                table: "contracts",
                newName: "TemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_contracts_AccountId1",
                table: "contracts",
                newName: "IX_contracts_TemplateId");

            migrationBuilder.Sql("UPDATE contracts SET \"TemplateId\" = NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "services",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "services",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "services",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "services",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "QuotationNumber",
                table: "quotations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "quotations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "quotations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "quotations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "quotations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentQuotationId",
                table: "quotations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfUrl",
                table: "quotations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PriceListId",
                table: "quotations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "quotations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedBy",
                table: "quotations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "quotations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevisionNumber",
                table: "quotations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "REV-01");

            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "quotations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SentById",
                table: "quotations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SentToEmail",
                table: "quotations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "quotations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedById",
                table: "quotations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsAndConditions",
                table: "quotations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "quotations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "quotation_items",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "quotation_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceCode",
                table: "quotation_items",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                table: "quotation_items",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "quotation_items",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "quotation_items",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "quotation_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "ContractNumber",
                table: "contracts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "contracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentContractId",
                table: "contracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTerms",
                table: "contracts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RenewalCount",
                table: "contracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ScopeOfWork",
                table: "contracts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SignedAt",
                table: "contracts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignedByClientEmail",
                table: "contracts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignedByClientName",
                table: "contracts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignerIpAddress",
                table: "contracts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialTerms",
                table: "contracts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "contract_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceCode",
                table: "contract_items",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                table: "contract_items",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "contract_items",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "contract_appendices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppendixNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AdjustedValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DocumentUrl = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_contract_appendices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contract_appendices_contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contract_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OldStatus = table.Column<string>(type: "text", nullable: true),
                    NewStatus = table.Column<string>(type: "text", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorName = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_contract_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contract_histories_contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contract_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_contract_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "price_lists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "USD"),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_price_lists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "quotation_approval_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorName = table.Column<string>(type: "text", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_quotation_approval_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quotation_approval_histories_quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_service_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "service_price_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NewPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ChangedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedByName = table.Column<string>(type: "text", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_service_price_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_service_price_histories_services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "signature_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExternalEnvelopeId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SignerName = table.Column<string>(type: "text", nullable: false),
                    SignerEmail = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SigningUrl = table.Column<string>(type: "text", nullable: true),
                    WebhookLog = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_signature_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_signature_requests_contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "price_list_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PriceListId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MinQuantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxDiscountRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_price_list_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_price_list_items_price_lists_PriceListId",
                        column: x => x.PriceListId,
                        principalTable: "price_lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_price_list_items_services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_services_CategoryId",
                table: "services",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_quotations_AccountId",
                table: "quotations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_quotations_ContactId",
                table: "quotations",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_quotations_ParentQuotationId",
                table: "quotations",
                column: "ParentQuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_quotations_PriceListId",
                table: "quotations",
                column: "PriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_ContactId",
                table: "contracts",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_ParentContractId",
                table: "contracts",
                column: "ParentContractId");

            migrationBuilder.CreateIndex(
                name: "IX_contract_appendices_ContractId",
                table: "contract_appendices",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_contract_histories_ContractId",
                table: "contract_histories",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_contract_templates_Code",
                table: "contract_templates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_price_list_items_PriceListId",
                table: "price_list_items",
                column: "PriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_price_list_items_ServiceId",
                table: "price_list_items",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_price_lists_Code",
                table: "price_lists",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quotation_approval_histories_QuotationId",
                table: "quotation_approval_histories",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_service_categories_Code",
                table: "service_categories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_price_histories_ServiceId",
                table: "service_price_histories",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_signature_requests_ContractId",
                table: "signature_requests",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_contract_items_services_ServiceId",
                table: "contract_items",
                column: "ServiceId",
                principalTable: "services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_contracts_contacts_ContactId",
                table: "contracts",
                column: "ContactId",
                principalTable: "contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_contracts_contract_templates_TemplateId",
                table: "contracts",
                column: "TemplateId",
                principalTable: "contract_templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_contracts_contracts_ParentContractId",
                table: "contracts",
                column: "ParentContractId",
                principalTable: "contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_contracts_quotations_QuotationId",
                table: "contracts",
                column: "QuotationId",
                principalTable: "quotations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_quotation_items_services_ServiceId",
                table: "quotation_items",
                column: "ServiceId",
                principalTable: "services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_quotations_accounts_AccountId",
                table: "quotations",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_quotations_contacts_ContactId",
                table: "quotations",
                column: "ContactId",
                principalTable: "contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_quotations_deals_DealId",
                table: "quotations",
                column: "DealId",
                principalTable: "deals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_quotations_price_lists_PriceListId",
                table: "quotations",
                column: "PriceListId",
                principalTable: "price_lists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_quotations_quotations_ParentQuotationId",
                table: "quotations",
                column: "ParentQuotationId",
                principalTable: "quotations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_services_service_categories_CategoryId",
                table: "services",
                column: "CategoryId",
                principalTable: "service_categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_contract_items_services_ServiceId",
                table: "contract_items");

            migrationBuilder.DropForeignKey(
                name: "FK_contracts_contacts_ContactId",
                table: "contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_contracts_contract_templates_TemplateId",
                table: "contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_contracts_contracts_ParentContractId",
                table: "contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_contracts_quotations_QuotationId",
                table: "contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_quotation_items_services_ServiceId",
                table: "quotation_items");

            migrationBuilder.DropForeignKey(
                name: "FK_quotations_accounts_AccountId",
                table: "quotations");

            migrationBuilder.DropForeignKey(
                name: "FK_quotations_contacts_ContactId",
                table: "quotations");

            migrationBuilder.DropForeignKey(
                name: "FK_quotations_deals_DealId",
                table: "quotations");

            migrationBuilder.DropForeignKey(
                name: "FK_quotations_price_lists_PriceListId",
                table: "quotations");

            migrationBuilder.DropForeignKey(
                name: "FK_quotations_quotations_ParentQuotationId",
                table: "quotations");

            migrationBuilder.DropForeignKey(
                name: "FK_services_service_categories_CategoryId",
                table: "services");

            migrationBuilder.DropTable(
                name: "contract_appendices");

            migrationBuilder.DropTable(
                name: "contract_histories");

            migrationBuilder.DropTable(
                name: "contract_templates");

            migrationBuilder.DropTable(
                name: "price_list_items");

            migrationBuilder.DropTable(
                name: "quotation_approval_histories");

            migrationBuilder.DropTable(
                name: "service_categories");

            migrationBuilder.DropTable(
                name: "service_price_histories");

            migrationBuilder.DropTable(
                name: "signature_requests");

            migrationBuilder.DropTable(
                name: "price_lists");

            migrationBuilder.DropIndex(
                name: "IX_services_CategoryId",
                table: "services");

            migrationBuilder.DropIndex(
                name: "IX_quotations_AccountId",
                table: "quotations");

            migrationBuilder.DropIndex(
                name: "IX_quotations_ContactId",
                table: "quotations");

            migrationBuilder.DropIndex(
                name: "IX_quotations_ParentQuotationId",
                table: "quotations");

            migrationBuilder.DropIndex(
                name: "IX_quotations_PriceListId",
                table: "quotations");

            migrationBuilder.DropIndex(
                name: "IX_contracts_ContactId",
                table: "contracts");

            migrationBuilder.DropIndex(
                name: "IX_contracts_ParentContractId",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "services");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "services");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "ParentQuotationId",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "PdfUrl",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "PriceListId",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "RevisionNumber",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "SentById",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "SentToEmail",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "SubmittedById",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "TermsAndConditions",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "quotation_items");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "quotation_items");

            migrationBuilder.DropColumn(
                name: "ServiceCode",
                table: "quotation_items");

            migrationBuilder.DropColumn(
                name: "ServiceName",
                table: "quotation_items");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "quotation_items");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "quotation_items");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "quotation_items");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "ParentContractId",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "PaymentTerms",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "RenewalCount",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "ScopeOfWork",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "SignedAt",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "SignedByClientEmail",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "SignedByClientName",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "SignerIpAddress",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "SpecialTerms",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "contract_items");

            migrationBuilder.DropColumn(
                name: "ServiceCode",
                table: "contract_items");

            migrationBuilder.DropColumn(
                name: "ServiceName",
                table: "contract_items");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "contract_items");

            migrationBuilder.RenameColumn(
                name: "TemplateId",
                table: "contracts",
                newName: "AccountId1");

            migrationBuilder.RenameIndex(
                name: "IX_contracts_TemplateId",
                table: "contracts",
                newName: "IX_contracts_AccountId1");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "services",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "services",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "QuotationNumber",
                table: "quotations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ContractNumber",
                table: "contracts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_contract_items_services_ServiceId",
                table: "contract_items",
                column: "ServiceId",
                principalTable: "services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_contracts_accounts_AccountId1",
                table: "contracts",
                column: "AccountId1",
                principalTable: "accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_contracts_quotations_QuotationId",
                table: "contracts",
                column: "QuotationId",
                principalTable: "quotations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_quotation_items_services_ServiceId",
                table: "quotation_items",
                column: "ServiceId",
                principalTable: "services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_quotations_deals_DealId",
                table: "quotations",
                column: "DealId",
                principalTable: "deals",
                principalColumn: "Id");
        }
    }
}
