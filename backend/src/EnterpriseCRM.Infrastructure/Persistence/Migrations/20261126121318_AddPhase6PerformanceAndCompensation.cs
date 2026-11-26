using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseCRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase6PerformanceAndCompensation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_commissions_commission_policies_PolicyId",
                table: "commissions");

            migrationBuilder.DropForeignKey(
                name: "FK_commissions_contracts_ContractId",
                table: "commissions");

            migrationBuilder.DropForeignKey(
                name: "FK_commissions_deals_DealId",
                table: "commissions");

            migrationBuilder.DropForeignKey(
                name: "FK_payroll_records_employees_EmployeeId",
                table: "payroll_records");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "payroll_records",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<decimal>(
                name: "Allowances",
                table: "payroll_records",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "payroll_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "payroll_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionApproved",
                table: "payroll_records",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionEarned",
                table: "payroll_records",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionPaid",
                table: "payroll_records",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionPayable",
                table: "payroll_records",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossSalary",
                table: "payroll_records",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "payroll_records",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PaymentDate",
                table: "payroll_records",
                type: "date",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "payroll_periods",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "payroll_periods",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "payroll_periods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "payroll_periods",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "payroll_periods",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalizedAt",
                table: "payroll_periods",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAllowances",
                table: "payroll_periods",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBaseSalary",
                table: "payroll_periods",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBonuses",
                table: "payroll_periods",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCommission",
                table: "payroll_periods",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeductions",
                table: "payroll_periods",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalGross",
                table: "payroll_periods",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalNet",
                table: "payroll_periods",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "payroll_adjustments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "payroll_adjustments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AchievedCalls",
                table: "kpis",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "AchievedConversionRate",
                table: "kpis",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AchievedMeetings",
                table: "kpis",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AchievedNewLeads",
                table: "kpis",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AchievedTasksCompleted",
                table: "kpis",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "AchievementScore",
                table: "kpis",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "kpis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "kpis",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "kpis",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefinitionId",
                table: "kpis",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "kpis",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EvaluatedAt",
                table: "kpis",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EvaluatedBy",
                table: "kpis",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PeriodQuarter",
                table: "kpis",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "kpis",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "kpis",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TargetCalls",
                table: "kpis",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetConversionRate",
                table: "kpis",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TargetMeetings",
                table: "kpis",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetTasksCompleted",
                table: "kpis",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "kpis",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightPercent",
                table: "kpis",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "commissions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<decimal>(
                name: "AdjustmentAmount",
                table: "commissions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "commissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "commissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseAmount",
                table: "commissions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CalculationBasis",
                table: "commissions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "NetCommission",
                table: "commissions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "commissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                table: "commissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PeriodMonth",
                table: "commissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PeriodYear",
                table: "commissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PolicyVersion",
                table: "commissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RateApplied",
                table: "commissions",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "commissions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PolicyType",
                table: "commission_policies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "commission_policies",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "CalculationBasis",
                table: "commission_policies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerType",
                table: "commission_policies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "commission_policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "commission_policies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxCap",
                table: "commission_policies",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceId",
                table: "commission_policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "commission_policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TierRulesJson",
                table: "commission_policies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "commission_policies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "bonus_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Period = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetThreshold = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BonusAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_bonus_policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compensation_audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldValueJson = table.Column<string>(type: "text", nullable: true),
                    NewValueJson = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_compensation_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "kpi_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MetricType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DefaultTarget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_kpi_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bonus_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BonusPolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PayrollRecordId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_bonus_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bonus_records_bonus_policies_BonusPolicyId",
                        column: x => x.BonusPolicyId,
                        principalTable: "bonus_policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_bonus_records_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bonus_records_payroll_records_PayrollRecordId",
                        column: x => x.PayrollRecordId,
                        principalTable: "payroll_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_kpis_DefinitionId",
                table: "kpis",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_kpis_DepartmentId",
                table: "kpis",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_kpis_TeamId",
                table: "kpis",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_commissions_PaymentId",
                table: "commissions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_commission_policies_DepartmentId",
                table: "commission_policies",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_commission_policies_ServiceId",
                table: "commission_policies",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_commission_policies_TeamId",
                table: "commission_policies",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_bonus_records_BonusPolicyId",
                table: "bonus_records",
                column: "BonusPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_bonus_records_EmployeeId",
                table: "bonus_records",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_bonus_records_PayrollRecordId",
                table: "bonus_records",
                column: "PayrollRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_kpi_definitions_Code",
                table: "kpi_definitions",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_commission_policies_departments_DepartmentId",
                table: "commission_policies",
                column: "DepartmentId",
                principalTable: "departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_commission_policies_services_ServiceId",
                table: "commission_policies",
                column: "ServiceId",
                principalTable: "services",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_commission_policies_teams_TeamId",
                table: "commission_policies",
                column: "TeamId",
                principalTable: "teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_commissions_commission_policies_PolicyId",
                table: "commissions",
                column: "PolicyId",
                principalTable: "commission_policies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_commissions_contracts_ContractId",
                table: "commissions",
                column: "ContractId",
                principalTable: "contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_commissions_deals_DealId",
                table: "commissions",
                column: "DealId",
                principalTable: "deals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_commissions_payments_PaymentId",
                table: "commissions",
                column: "PaymentId",
                principalTable: "payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_kpis_departments_DepartmentId",
                table: "kpis",
                column: "DepartmentId",
                principalTable: "departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_kpis_kpi_definitions_DefinitionId",
                table: "kpis",
                column: "DefinitionId",
                principalTable: "kpi_definitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_kpis_teams_TeamId",
                table: "kpis",
                column: "TeamId",
                principalTable: "teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_payroll_records_employees_EmployeeId",
                table: "payroll_records",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_commission_policies_departments_DepartmentId",
                table: "commission_policies");

            migrationBuilder.DropForeignKey(
                name: "FK_commission_policies_services_ServiceId",
                table: "commission_policies");

            migrationBuilder.DropForeignKey(
                name: "FK_commission_policies_teams_TeamId",
                table: "commission_policies");

            migrationBuilder.DropForeignKey(
                name: "FK_commissions_commission_policies_PolicyId",
                table: "commissions");

            migrationBuilder.DropForeignKey(
                name: "FK_commissions_contracts_ContractId",
                table: "commissions");

            migrationBuilder.DropForeignKey(
                name: "FK_commissions_deals_DealId",
                table: "commissions");

            migrationBuilder.DropForeignKey(
                name: "FK_commissions_payments_PaymentId",
                table: "commissions");

            migrationBuilder.DropForeignKey(
                name: "FK_kpis_departments_DepartmentId",
                table: "kpis");

            migrationBuilder.DropForeignKey(
                name: "FK_kpis_kpi_definitions_DefinitionId",
                table: "kpis");

            migrationBuilder.DropForeignKey(
                name: "FK_kpis_teams_TeamId",
                table: "kpis");

            migrationBuilder.DropForeignKey(
                name: "FK_payroll_records_employees_EmployeeId",
                table: "payroll_records");

            migrationBuilder.DropTable(
                name: "bonus_records");

            migrationBuilder.DropTable(
                name: "compensation_audit_logs");

            migrationBuilder.DropTable(
                name: "kpi_definitions");

            migrationBuilder.DropTable(
                name: "bonus_policies");

            migrationBuilder.DropIndex(
                name: "IX_kpis_DefinitionId",
                table: "kpis");

            migrationBuilder.DropIndex(
                name: "IX_kpis_DepartmentId",
                table: "kpis");

            migrationBuilder.DropIndex(
                name: "IX_kpis_TeamId",
                table: "kpis");

            migrationBuilder.DropIndex(
                name: "IX_commissions_PaymentId",
                table: "commissions");

            migrationBuilder.DropIndex(
                name: "IX_commission_policies_DepartmentId",
                table: "commission_policies");

            migrationBuilder.DropIndex(
                name: "IX_commission_policies_ServiceId",
                table: "commission_policies");

            migrationBuilder.DropIndex(
                name: "IX_commission_policies_TeamId",
                table: "commission_policies");

            migrationBuilder.DropColumn(
                name: "Allowances",
                table: "payroll_records");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "payroll_records");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "payroll_records");

            migrationBuilder.DropColumn(
                name: "CommissionApproved",
                table: "payroll_records");

            migrationBuilder.DropColumn(
                name: "CommissionEarned",
                table: "payroll_records");

            migrationBuilder.DropColumn(
                name: "CommissionPaid",
                table: "payroll_records");

            migrationBuilder.DropColumn(
                name: "CommissionPayable",
                table: "payroll_records");

            migrationBuilder.DropColumn(
                name: "GrossSalary",
                table: "payroll_records");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "payroll_records");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "payroll_records");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "payroll_periods");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "payroll_periods");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "payroll_periods");

            migrationBuilder.DropColumn(
                name: "FinalizedAt",
                table: "payroll_periods");

            migrationBuilder.DropColumn(
                name: "TotalAllowances",
                table: "payroll_periods");

            migrationBuilder.DropColumn(
                name: "TotalBaseSalary",
                table: "payroll_periods");

            migrationBuilder.DropColumn(
                name: "TotalBonuses",
                table: "payroll_periods");

            migrationBuilder.DropColumn(
                name: "TotalCommission",
                table: "payroll_periods");

            migrationBuilder.DropColumn(
                name: "TotalDeductions",
                table: "payroll_periods");

            migrationBuilder.DropColumn(
                name: "TotalGross",
                table: "payroll_periods");

            migrationBuilder.DropColumn(
                name: "TotalNet",
                table: "payroll_periods");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "payroll_adjustments");

            migrationBuilder.DropColumn(
                name: "AchievedCalls",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "AchievedConversionRate",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "AchievedMeetings",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "AchievedNewLeads",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "AchievedTasksCompleted",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "AchievementScore",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "DefinitionId",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "EvaluatedAt",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "EvaluatedBy",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "PeriodQuarter",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "TargetCalls",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "TargetConversionRate",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "TargetMeetings",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "TargetTasksCompleted",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "WeightPercent",
                table: "kpis");

            migrationBuilder.DropColumn(
                name: "AdjustmentAmount",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "BaseAmount",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "CalculationBasis",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "NetCommission",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "PeriodMonth",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "PeriodYear",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "PolicyVersion",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "RateApplied",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "CalculationBasis",
                table: "commission_policies");

            migrationBuilder.DropColumn(
                name: "CustomerType",
                table: "commission_policies");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "commission_policies");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "commission_policies");

            migrationBuilder.DropColumn(
                name: "MaxCap",
                table: "commission_policies");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "commission_policies");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "commission_policies");

            migrationBuilder.DropColumn(
                name: "TierRulesJson",
                table: "commission_policies");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "commission_policies");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "payroll_records",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "payroll_periods",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "payroll_periods",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "payroll_adjustments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "commissions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PolicyType",
                table: "commission_policies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "commission_policies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AddForeignKey(
                name: "FK_commissions_commission_policies_PolicyId",
                table: "commissions",
                column: "PolicyId",
                principalTable: "commission_policies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_commissions_contracts_ContractId",
                table: "commissions",
                column: "ContractId",
                principalTable: "contracts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_commissions_deals_DealId",
                table: "commissions",
                column: "DealId",
                principalTable: "deals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_payroll_records_employees_EmployeeId",
                table: "payroll_records",
                column: "EmployeeId",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
