using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Domain.Common;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Identity & Organization
    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // CRM
    public DbSet<LeadSource> LeadSources => Set<LeadSource>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadActivity> LeadActivities => Set<LeadActivity>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<PipelineStage> PipelineStages => Set<PipelineStage>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<DealStageHistory> DealStageHistories => Set<DealStageHistory>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<CrmActivity> CrmActivities => Set<CrmActivity>();

    // Billing & Operations
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<ServiceItem> Services => Set<ServiceItem>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();
    public DbSet<ServicePriceHistory> ServicePriceHistories => Set<ServicePriceHistory>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();
    public DbSet<QuotationApprovalHistory> QuotationApprovalHistories => Set<QuotationApprovalHistory>();
    public DbSet<ContractTemplate> ContractTemplates => Set<ContractTemplate>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractItem> ContractItems => Set<ContractItem>();
    public DbSet<ContractAppendix> ContractAppendices => Set<ContractAppendix>();
    public DbSet<ContractHistory> ContractHistories => Set<ContractHistory>();
    public DbSet<SignatureRequest> SignatureRequests => Set<SignatureRequest>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<PaymentSchedule> PaymentSchedules => Set<PaymentSchedule>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<InvoiceRequest> InvoiceRequests => Set<InvoiceRequest>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<ReceivableCollectionLog> ReceivableCollectionLogs => Set<ReceivableCollectionLog>();
    public DbSet<FinancialReconciliationRecord> FinancialReconciliationRecords => Set<FinancialReconciliationRecord>();
    public DbSet<ReconciliationItem> ReconciliationItems => Set<ReconciliationItem>();

    // Performance, Payroll & Automation
    public DbSet<KpiDefinition> KpiDefinitions => Set<KpiDefinition>();
    public DbSet<Kpi> Kpis => Set<Kpi>();
    public DbSet<CommissionPolicy> CommissionPolicies => Set<CommissionPolicy>();
    public DbSet<Commission> Commissions => Set<Commission>();
    public DbSet<BonusPolicy> BonusPolicies => Set<BonusPolicy>();
    public DbSet<BonusRecord> BonusRecords => Set<BonusRecord>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<PayrollAdjustment> PayrollAdjustments => Set<PayrollAdjustment>();
    public DbSet<CompensationAuditLog> CompensationAuditLogs => Set<CompensationAuditLog>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowExecution> WorkflowExecutions => Set<WorkflowExecution>();
    public DbSet<WorkflowExecutionLog> WorkflowExecutionLogs => Set<WorkflowExecutionLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<LeadAssignmentRule> LeadAssignmentRules => Set<LeadAssignmentRule>();
    public DbSet<LeadAssignmentLog> LeadAssignmentLogs => Set<LeadAssignmentLog>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<BusinessAlert> BusinessAlerts => Set<BusinessAlert>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<AiConversation> AiConversations => Set<AiConversation>();
    public DbSet<AiConversationMessage> AiConversationMessages => Set<AiConversationMessage>();
    public DbSet<AiProviderConfiguration> AiProviderConfigurations => Set<AiProviderConfiguration>();
    public DbSet<AiUsageLog> AiUsageLogs => Set<AiUsageLog>();
    public DbSet<RevenueForecast> RevenueForecasts => Set<RevenueForecast>();
    public DbSet<AnomalyDetection> AnomalyDetections => Set<AnomalyDetection>();
    public DbSet<BusinessRecommendation> BusinessRecommendations => Set<BusinessRecommendation>();
    public DbSet<ScheduledReport> ScheduledReports => Set<ScheduledReport>();
    public DbSet<GeneratedReport> GeneratedReports => Set<GeneratedReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Global Query Filters for Soft Delete
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Department>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Team>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Lead>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Account>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Contact>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Deal>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TaskItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Appointment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CrmActivity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ServiceCategory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ServiceItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PriceList>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Quotation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ContractTemplate>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Contract>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ContractAppendix>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SignatureRequest>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PaymentSchedule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Payment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<InvoiceRequest>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ReceivableCollectionLog>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FinancialReconciliationRecord>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CommissionPolicy>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<KpiDefinition>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BonusPolicy>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BonusRecord>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WorkflowDefinition>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LeadAssignmentRule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AlertRule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AiConversation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ScheduledReport>().HasQueryFilter(e => !e.IsDeleted);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(e => e.Salt).HasMaxLength(128).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        });

        // Department Configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("departments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
        });

        // Team Configuration
        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("teams");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Teams)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Employee Configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employees");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            entity.Property(e => e.EmployeeCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.BaseSalary).HasPrecision(18, 2);

            entity.HasOne(e => e.User)
                  .WithOne(u => u.EmployeeProfile)
                  .HasForeignKey<Employee>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Team)
                  .WithMany(t => t.Members)
                  .HasForeignKey(e => e.TeamId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Roles & Permissions
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(e => e.UserId);

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(e => e.RoleId);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");
            entity.HasKey(e => new { e.RoleId, e.PermissionId });

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.RolePermissions)
                  .HasForeignKey(e => e.RoleId);

            entity.HasOne(e => e.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(e => e.PermissionId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.EntityName);
        });

        // CRM: Leads
        modelBuilder.Entity<LeadSource>(entity =>
        {
            entity.ToTable("lead_sources");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("leads");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Phone);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.EstimatedBudget).HasPrecision(18, 2);
            entity.HasOne(e => e.Source).WithMany(s => s.Leads).HasForeignKey(e => e.SourceId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.AssignedEmployee).WithMany().HasForeignKey(e => e.AssignedEmployeeId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<LeadActivity>(entity =>
        {
            entity.ToTable("lead_activities");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Lead).WithMany(l => l.Activities).HasForeignKey(e => e.LeadId).OnDelete(DeleteBehavior.Cascade);
        });

        // CRM: Accounts & Contacts
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.TaxCode);
            entity.HasIndex(e => e.Phone);
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("contacts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Phone);
            entity.HasIndex(e => e.Email);
            entity.HasOne(e => e.Account).WithMany(a => a.Contacts).HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Cascade);
        });

        // CRM: Deals & Stages
        modelBuilder.Entity<PipelineStage>(entity =>
        {
            entity.ToTable("pipeline_stages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Probability).HasPrecision(5, 2);
        });

        modelBuilder.Entity<Deal>(entity =>
        {
            entity.ToTable("deals");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.ProbabilityOverride).HasPrecision(5, 2);
            entity.HasOne(e => e.Account).WithMany(a => a.Deals).HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Stage).WithMany(s => s.Deals).HasForeignKey(e => e.StageId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DealStageHistory>(entity =>
        {
            entity.ToTable("deal_stage_histories");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealId);
            entity.HasIndex(e => e.ChangedAt);
            entity.HasOne(e => e.Deal).WithMany(d => d.StageHistories).HasForeignKey(e => e.DealId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.FromStage).WithMany().HasForeignKey(e => e.FromStageId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ToStage).WithMany().HasForeignKey(e => e.ToStageId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CrmActivity>(entity =>
        {
            entity.ToTable("crm_activities");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.ContactId);
            entity.HasIndex(e => e.DealId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ActivityType);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("tasks");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.StartTime);
        });

        // Billing: Services & Price Lists
        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.ToTable("service_categories");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<PriceList>(entity =>
        {
            entity.ToTable("price_lists");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("USD");
        });

        modelBuilder.Entity<PriceListItem>(entity =>
        {
            entity.ToTable("price_list_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.MinQuantity).HasPrecision(10, 2);
            entity.Property(e => e.MaxDiscountRate).HasPrecision(5, 2);
            entity.HasOne(e => e.PriceList).WithMany(p => p.Items).HasForeignKey(e => e.PriceListId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Service).WithMany(s => s.PriceListItems).HasForeignKey(e => e.ServiceId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ServicePriceHistory>(entity =>
        {
            entity.ToTable("service_price_histories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OldPrice).HasPrecision(18, 2);
            entity.Property(e => e.NewPrice).HasPrecision(18, 2);
            entity.HasOne(e => e.Service).WithMany(s => s.PriceHistories).HasForeignKey(e => e.ServiceId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ServiceItem>(entity =>
        {
            entity.ToTable("services");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.BasePrice).HasPrecision(18, 2);
            entity.Property(e => e.CostPrice).HasPrecision(18, 2);
            entity.Property(e => e.VatRate).HasPrecision(5, 2);
            entity.HasOne(e => e.ServiceCategory).WithMany(c => c.Services).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Quotation>(entity =>
        {
            entity.ToTable("quotations");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.QuotationNumber, e.RevisionNumber }).IsUnique();
            entity.Property(e => e.QuotationNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RevisionNumber).HasMaxLength(20).HasDefaultValue("REV-01");
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.VatAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.HasOne(e => e.Account).WithMany(a => a.Quotations).HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Contact).WithMany(c => c.Quotations).HasForeignKey(e => e.ContactId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Deal).WithMany(d => d.Quotations).HasForeignKey(e => e.DealId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.PriceList).WithMany(p => p.Quotations).HasForeignKey(e => e.PriceListId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ParentQuotation).WithMany(q => q.Revisions).HasForeignKey(e => e.ParentQuotationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuotationItem>(entity =>
        {
            entity.ToTable("quotation_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(10, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountRate).HasPrecision(5, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.VatRate).HasPrecision(5, 2);
            entity.Property(e => e.VatAmount).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            entity.HasOne(e => e.Quotation).WithMany(q => q.Items).HasForeignKey(e => e.QuotationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Service).WithMany(s => s.QuotationItems).HasForeignKey(e => e.ServiceId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuotationApprovalHistory>(entity =>
        {
            entity.ToTable("quotation_approval_histories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.Quotation).WithMany(q => q.ApprovalHistories).HasForeignKey(e => e.QuotationId).OnDelete(DeleteBehavior.Cascade);
        });

        // Billing: Contract Templates, Contracts & Appendices
        modelBuilder.Entity<ContractTemplate>(entity =>
        {
            entity.ToTable("contract_templates");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("contracts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContractNumber).IsUnique();
            entity.Property(e => e.ContractNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TotalValue).HasPrecision(18, 2);
            entity.HasOne(e => e.Account).WithMany(a => a.Contracts).HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Contact).WithMany(c => c.Contracts).HasForeignKey(e => e.ContactId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Quotation).WithMany(q => q.Contracts).HasForeignKey(e => e.QuotationId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Template).WithMany(t => t.Contracts).HasForeignKey(e => e.TemplateId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ParentContract).WithMany(c => c.Renewals).HasForeignKey(e => e.ParentContractId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContractItem>(entity =>
        {
            entity.ToTable("contract_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(10, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            entity.HasOne(e => e.Contract).WithMany(c => c.Items).HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Service).WithMany(s => s.ContractItems).HasForeignKey(e => e.ServiceId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContractAppendix>(entity =>
        {
            entity.ToTable("contract_appendices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AppendixNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.AdjustedValue).HasPrecision(18, 2);
            entity.HasOne(e => e.Contract).WithMany(c => c.Appendices).HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ContractHistory>(entity =>
        {
            entity.ToTable("contract_histories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.Contract).WithMany(c => c.Histories).HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SignatureRequest>(entity =>
        {
            entity.ToTable("signature_requests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Provider).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ExternalEnvelopeId).HasMaxLength(150);
            entity.HasOne(e => e.Contract).WithMany(c => c.SignatureRequests).HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.Cascade);
        });

        // Billing: Projects, Payments & Invoices
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("projects");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<PaymentSchedule>(entity =>
        {
            entity.ToTable("payment_schedules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MilestoneTitle).HasMaxLength(200);
            entity.Property(e => e.Percentage).HasPrecision(5, 2);
            entity.Property(e => e.AmountDue).HasPrecision(18, 2);
            entity.Property(e => e.AmountPaid).HasPrecision(18, 2);
            entity.HasOne(e => e.Contract).WithMany(c => c.PaymentSchedules).HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PaymentNumber).IsUnique();
            entity.Property(e => e.PaymentNumber).HasMaxLength(50);
            entity.Property(e => e.TransactionReference).HasMaxLength(100);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired();
            entity.HasOne(e => e.Contract).WithMany(c => c.Payments).HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Account).WithMany(a => a.Payments).HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PaymentSchedule).WithMany(s => s.Payments).HasForeignKey(e => e.PaymentScheduleId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InvoiceRequest>(entity =>
        {
            entity.ToTable("invoice_requests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired();
            entity.HasOne(e => e.Contract).WithMany(c => c.InvoiceRequests).HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Payment).WithMany().HasForeignKey(e => e.PaymentId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("invoices");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.InvoiceSeries).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalWithTax).HasPrecision(18, 2);
            entity.HasOne(e => e.Contract).WithMany(c => c.Invoices).HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Account).WithMany(a => a.Invoices).HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.InvoiceRequest).WithOne(r => r.IssuedInvoice).HasForeignKey<Invoice>(e => e.InvoiceRequestId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.ToTable("invoice_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(10, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            entity.HasOne(e => e.Invoice).WithMany(i => i.Items).HasForeignKey(e => e.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Service).WithMany().HasForeignKey(e => e.ServiceId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReceivableCollectionLog>(entity =>
        {
            entity.ToTable("receivable_collection_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActivityType).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.Contract).WithMany().HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PaymentSchedule).WithMany().HasForeignKey(e => e.PaymentScheduleId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FinancialReconciliationRecord>(entity =>
        {
            entity.ToTable("financial_reconciliation_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Period).HasMaxLength(20).IsRequired();
            entity.Property(e => e.TotalContractValue).HasPrecision(18, 2);
            entity.Property(e => e.TotalExpectedRevenue).HasPrecision(18, 2);
            entity.Property(e => e.TotalCollectedRevenue).HasPrecision(18, 2);
            entity.Property(e => e.TotalInvoicedRevenue).HasPrecision(18, 2);
            entity.Property(e => e.DiscrepancyAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ReconciliationItem>(entity =>
        {
            entity.ToTable("reconciliation_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DiscrepancyType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ExpectedAmount).HasPrecision(18, 2);
            entity.Property(e => e.ActualAmount).HasPrecision(18, 2);
            entity.Property(e => e.Difference).HasPrecision(18, 2);
            entity.HasOne(e => e.ReconciliationRecord).WithMany(r => r.Items).HasForeignKey(e => e.ReconciliationRecordId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Contract).WithMany().HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Payment).WithMany().HasForeignKey(e => e.PaymentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Invoice).WithMany().HasForeignKey(e => e.InvoiceId).OnDelete(DeleteBehavior.SetNull);
        });

        // Performance & Payroll
        modelBuilder.Entity<KpiDefinition>(entity =>
        {
            entity.ToTable("kpi_definitions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.MetricType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Unit).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DefaultTarget).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Kpi>(entity =>
        {
            entity.ToTable("kpis");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Scope).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TargetRevenue).HasPrecision(18, 2);
            entity.Property(e => e.AchievedRevenue).HasPrecision(18, 2);
            entity.Property(e => e.TargetConversionRate).HasPrecision(5, 2);
            entity.Property(e => e.AchievedConversionRate).HasPrecision(5, 2);
            entity.Property(e => e.WeightPercent).HasPrecision(5, 2);
            entity.Property(e => e.AchievementScore).HasPrecision(5, 2);

            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Definition).WithMany().HasForeignKey(e => e.DefinitionId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Team).WithMany().HasForeignKey(e => e.TeamId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CommissionPolicy>(entity =>
        {
            entity.ToTable("commission_policies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.PolicyType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CalculationBasis).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CustomerType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.MinThreshold).HasPrecision(18, 2);
            entity.Property(e => e.MaxThreshold).HasPrecision(18, 2);
            entity.Property(e => e.MaxCap).HasPrecision(18, 2);
            entity.Property(e => e.RatePercent).HasPrecision(5, 2);

            entity.HasOne(e => e.Service).WithMany().HasForeignKey(e => e.ServiceId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Team).WithMany().HasForeignKey(e => e.TeamId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Commission>(entity =>
        {
            entity.ToTable("commissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CalculationBasis).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SourceType).HasMaxLength(50);
            entity.Property(e => e.BaseAmount).HasPrecision(18, 2);
            entity.Property(e => e.RateApplied).HasPrecision(5, 2);
            entity.Property(e => e.CalculatedAmount).HasPrecision(18, 2);
            entity.Property(e => e.AdjustmentAmount).HasPrecision(18, 2);
            entity.Property(e => e.NetCommission).HasPrecision(18, 2);

            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Deal).WithMany().HasForeignKey(e => e.DealId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Contract).WithMany().HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Payment).WithMany().HasForeignKey(e => e.PaymentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Policy).WithMany().HasForeignKey(e => e.PolicyId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<BonusPolicy>(entity =>
        {
            entity.ToTable("bonus_policies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Period).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TargetThreshold).HasPrecision(18, 2);
            entity.Property(e => e.BonusAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<BonusRecord>(entity =>
        {
            entity.ToTable("bonus_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            entity.HasOne(e => e.BonusPolicy).WithMany().HasForeignKey(e => e.BonusPolicyId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PayrollRecord).WithMany().HasForeignKey(e => e.PayrollRecordId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PayrollPeriod>(entity =>
        {
            entity.ToTable("payroll_periods");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TotalBaseSalary).HasPrecision(18, 2);
            entity.Property(e => e.TotalCommission).HasPrecision(18, 2);
            entity.Property(e => e.TotalBonuses).HasPrecision(18, 2);
            entity.Property(e => e.TotalAllowances).HasPrecision(18, 2);
            entity.Property(e => e.TotalDeductions).HasPrecision(18, 2);
            entity.Property(e => e.TotalGross).HasPrecision(18, 2);
            entity.Property(e => e.TotalNet).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PayrollRecord>(entity =>
        {
            entity.ToTable("payroll_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.BaseSalary).HasPrecision(18, 2);
            entity.Property(e => e.TotalCommission).HasPrecision(18, 2);
            entity.Property(e => e.Bonuses).HasPrecision(18, 2);
            entity.Property(e => e.Allowances).HasPrecision(18, 2);
            entity.Property(e => e.Deductions).HasPrecision(18, 2);
            entity.Property(e => e.GrossSalary).HasPrecision(18, 2);
            entity.Property(e => e.NetSalary).HasPrecision(18, 2);
            entity.Property(e => e.CommissionEarned).HasPrecision(18, 2);
            entity.Property(e => e.CommissionApproved).HasPrecision(18, 2);
            entity.Property(e => e.CommissionPayable).HasPrecision(18, 2);
            entity.Property(e => e.CommissionPaid).HasPrecision(18, 2);

            entity.HasOne(e => e.PayrollPeriod).WithMany(p => p.Records).HasForeignKey(e => e.PayrollPeriodId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PayrollAdjustment>(entity =>
        {
            entity.ToTable("payroll_adjustments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(e => e.PayrollRecord).WithMany(r => r.Adjustments).HasForeignKey(e => e.PayrollRecordId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CompensationAuditLog>(entity =>
        {
            entity.ToTable("compensation_audit_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
        });

        // Automation & Notifications
        modelBuilder.Entity<WorkflowDefinition>(entity =>
        {
            entity.ToTable("workflow_definitions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TriggerEvent).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<WorkflowExecution>(entity =>
        {
            entity.ToTable("workflow_executions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TriggerEvent).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.Workflow).WithMany(w => w.Executions).HasForeignKey(e => e.WorkflowId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkflowExecutionLog>(entity =>
        {
            entity.ToTable("workflow_execution_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StepName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ActionType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.WorkflowExecution).WithMany(we => we.Logs).HasForeignKey(e => e.WorkflowExecutionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Priority).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.RecipientUserId);
            entity.HasIndex(e => e.IsRead);
            entity.HasOne(e => e.RecipientUser).WithMany().HasForeignKey(e => e.RecipientUserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LeadAssignmentRule>(entity =>
        {
            entity.ToTable("lead_assignment_rules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Mode).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<LeadAssignmentLog>(entity =>
        {
            entity.ToTable("lead_assignment_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Mode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.HasOne(e => e.Lead).WithMany().HasForeignKey(e => e.LeadId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Rule).WithMany().HasForeignKey(e => e.RuleId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.AssignedEmployee).WithMany().HasForeignKey(e => e.AssignedEmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AlertRule>(entity =>
        {
            entity.ToTable("alert_rules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AlertType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Severity).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Channels).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ThresholdAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<BusinessAlert>(entity =>
        {
            entity.ToTable("business_alerts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(250).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Severity).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.AlertRule).WithMany().HasForeignKey(e => e.AlertRuleId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.ToTable("notification_preferences");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.MinSeverity).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // AI Assistant & Analytics
        modelBuilder.Entity<AiConversation>(entity =>
        {
            entity.ToTable("ai_conversations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiConversationMessage>(entity =>
        {
            entity.ToTable("ai_conversation_messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ModelUsed).HasMaxLength(100);
            entity.HasOne(e => e.Conversation).WithMany(c => c.Messages).HasForeignKey(e => e.ConversationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiProviderConfiguration>(entity =>
        {
            entity.ToTable("ai_provider_configurations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProviderType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ModelName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Temperature).HasPrecision(5, 2);
        });

        modelBuilder.Entity<AiUsageLog>(entity =>
        {
            entity.ToTable("ai_usage_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OperationType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ProviderType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ModelName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<RevenueForecast>(entity =>
        {
            entity.ToTable("revenue_forecasts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PeriodLabel).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ModelType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PredictedAmount).HasPrecision(18, 2);
            entity.Property(e => e.LowerBound).HasPrecision(18, 2);
            entity.Property(e => e.UpperBound).HasPrecision(18, 2);
            entity.Property(e => e.ConfidenceScore).HasPrecision(18, 2);
        });

        modelBuilder.Entity<AnomalyDetection>(entity =>
        {
            entity.ToTable("anomaly_detections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetricName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Severity).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ActualValue).HasPrecision(18, 2);
            entity.Property(e => e.ExpectedValue).HasPrecision(18, 2);
            entity.Property(e => e.DeviationPercent).HasPrecision(18, 2);
        });

        modelBuilder.Entity<BusinessRecommendation>(entity =>
        {
            entity.ToTable("business_recommendations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TargetEntityType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Priority).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<ScheduledReport>(entity =>
        {
            entity.ToTable("scheduled_reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ReportType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Format).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TimeOfDay).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<GeneratedReport>(entity =>
        {
            entity.ToTable("generated_reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReportType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
            entity.HasOne(e => e.ScheduledReport).WithMany().HasForeignKey(e => e.ScheduledReportId).OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
