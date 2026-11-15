using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseCRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase3CoreCrmEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeadId",
                table: "tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Campaign",
                table: "leads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "leads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextFollowUpDate",
                table: "leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "leads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "leads",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "deals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProbabilityOverride",
                table: "deals",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "deals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId1",
                table: "contracts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "contacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "contacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeadId",
                table: "appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "accounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "crm_activities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContactId = table.Column<Guid>(type: "uuid", nullable: true),
                    DealId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActivityType = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformedByName = table.Column<string>(type: "text", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_crm_activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crm_activities_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_crm_activities_contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "contacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_crm_activities_deals_DealId",
                        column: x => x.DealId,
                        principalTable: "deals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_crm_activities_leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "leads",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "deal_stage_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DealId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStageId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToStageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedByName = table.Column<string>(type: "text", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_deal_stage_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_deal_stage_histories_deals_DealId",
                        column: x => x.DealId,
                        principalTable: "deals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deal_stage_histories_pipeline_stages_FromStageId",
                        column: x => x.FromStageId,
                        principalTable: "pipeline_stages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_deal_stage_histories_pipeline_stages_ToStageId",
                        column: x => x.ToStageId,
                        principalTable: "pipeline_stages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tasks_ContactId",
                table: "tasks",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_LeadId",
                table: "tasks",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_AccountId",
                table: "projects",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_leads_ConvertedAccountId",
                table: "leads",
                column: "ConvertedAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_leads_ConvertedContactId",
                table: "leads",
                column: "ConvertedContactId");

            migrationBuilder.CreateIndex(
                name: "IX_leads_ConvertedDealId",
                table: "leads",
                column: "ConvertedDealId");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_AccountId1",
                table: "contracts",
                column: "AccountId1");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_AccountId",
                table: "appointments",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_LeadId",
                table: "appointments",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_crm_activities_AccountId",
                table: "crm_activities",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_crm_activities_ActivityType",
                table: "crm_activities",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_crm_activities_ContactId",
                table: "crm_activities",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_crm_activities_CreatedAt",
                table: "crm_activities",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_crm_activities_DealId",
                table: "crm_activities",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_crm_activities_LeadId",
                table: "crm_activities",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_deal_stage_histories_ChangedAt",
                table: "deal_stage_histories",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_deal_stage_histories_DealId",
                table: "deal_stage_histories",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_deal_stage_histories_FromStageId",
                table: "deal_stage_histories",
                column: "FromStageId");

            migrationBuilder.CreateIndex(
                name: "IX_deal_stage_histories_ToStageId",
                table: "deal_stage_histories",
                column: "ToStageId");

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_accounts_AccountId",
                table: "appointments",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_leads_LeadId",
                table: "appointments",
                column: "LeadId",
                principalTable: "leads",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_contracts_accounts_AccountId1",
                table: "contracts",
                column: "AccountId1",
                principalTable: "accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_leads_accounts_ConvertedAccountId",
                table: "leads",
                column: "ConvertedAccountId",
                principalTable: "accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_leads_contacts_ConvertedContactId",
                table: "leads",
                column: "ConvertedContactId",
                principalTable: "contacts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_leads_deals_ConvertedDealId",
                table: "leads",
                column: "ConvertedDealId",
                principalTable: "deals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_accounts_AccountId",
                table: "projects",
                column: "AccountId",
                principalTable: "accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_contacts_ContactId",
                table: "tasks",
                column: "ContactId",
                principalTable: "contacts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_leads_LeadId",
                table: "tasks",
                column: "LeadId",
                principalTable: "leads",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointments_accounts_AccountId",
                table: "appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_appointments_leads_LeadId",
                table: "appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_contracts_accounts_AccountId1",
                table: "contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_leads_accounts_ConvertedAccountId",
                table: "leads");

            migrationBuilder.DropForeignKey(
                name: "FK_leads_contacts_ConvertedContactId",
                table: "leads");

            migrationBuilder.DropForeignKey(
                name: "FK_leads_deals_ConvertedDealId",
                table: "leads");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_accounts_AccountId",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_tasks_contacts_ContactId",
                table: "tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tasks_leads_LeadId",
                table: "tasks");

            migrationBuilder.DropTable(
                name: "crm_activities");

            migrationBuilder.DropTable(
                name: "deal_stage_histories");

            migrationBuilder.DropIndex(
                name: "IX_tasks_ContactId",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "IX_tasks_LeadId",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "IX_projects_AccountId",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_leads_ConvertedAccountId",
                table: "leads");

            migrationBuilder.DropIndex(
                name: "IX_leads_ConvertedContactId",
                table: "leads");

            migrationBuilder.DropIndex(
                name: "IX_leads_ConvertedDealId",
                table: "leads");

            migrationBuilder.DropIndex(
                name: "IX_contracts_AccountId1",
                table: "contracts");

            migrationBuilder.DropIndex(
                name: "IX_appointments_AccountId",
                table: "appointments");

            migrationBuilder.DropIndex(
                name: "IX_appointments_LeadId",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "Campaign",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "NextFollowUpDate",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "ProbabilityOverride",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "AccountId1",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "accounts");
        }
    }
}
