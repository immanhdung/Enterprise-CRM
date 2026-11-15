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
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    // Billing & Operations
    public DbSet<ServiceItem> Services => Set<ServiceItem>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractItem> ContractItems => Set<ContractItem>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<PaymentSchedule> PaymentSchedules => Set<PaymentSchedule>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<InvoiceRequest> InvoiceRequests => Set<InvoiceRequest>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    // Performance, Payroll & Automation
    public DbSet<Kpi> Kpis => Set<Kpi>();
    public DbSet<CommissionPolicy> CommissionPolicies => Set<CommissionPolicy>();
    public DbSet<Commission> Commissions => Set<Commission>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<PayrollAdjustment> PayrollAdjustments => Set<PayrollAdjustment>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowExecution> WorkflowExecutions => Set<WorkflowExecution>();
    public DbSet<Notification> Notifications => Set<Notification>();

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
        modelBuilder.Entity<ServiceItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Quotation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Contract>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PaymentSchedule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Payment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<InvoiceRequest>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CommissionPolicy>().HasQueryFilter(e => !e.IsDeleted);

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
            entity.HasOne(e => e.Account).WithMany(a => a.Deals).HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Stage).WithMany(s => s.Deals).HasForeignKey(e => e.StageId).OnDelete(DeleteBehavior.Restrict);
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

        // Billing: Services & Quotations
        modelBuilder.Entity<ServiceItem>(entity =>
        {
            entity.ToTable("services");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.BasePrice).HasPrecision(18, 2);
            entity.Property(e => e.VatRate).HasPrecision(5, 2);
        });

        modelBuilder.Entity<Quotation>(entity =>
        {
            entity.ToTable("quotations");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QuotationNumber).IsUnique();
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.VatAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<QuotationItem>(entity =>
        {
            entity.ToTable("quotation_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(10, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountRate).HasPrecision(5, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            entity.HasOne(e => e.Quotation).WithMany(q => q.Items).HasForeignKey(e => e.QuotationId).OnDelete(DeleteBehavior.Cascade);
        });

        // Billing: Contracts & Appendices
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("contracts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContractNumber).IsUnique();
            entity.Property(e => e.TotalValue).HasPrecision(18, 2);
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContractItem>(entity =>
        {
            entity.ToTable("contract_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(10, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            entity.HasOne(e => e.Contract).WithMany(c => c.Items).HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.Cascade);
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
            entity.Property(e => e.AmountDue).HasPrecision(18, 2);
            entity.Property(e => e.AmountPaid).HasPrecision(18, 2);
            entity.HasOne(e => e.Contract).WithMany(c => c.PaymentSchedules).HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TransactionReference).IsUnique();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(e => e.Contract).WithMany().HasForeignKey(e => e.ContractId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PaymentSchedule).WithMany(s => s.Payments).HasForeignKey(e => e.PaymentScheduleId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InvoiceRequest>(entity =>
        {
            entity.ToTable("invoice_requests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("invoices");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalWithTax).HasPrecision(18, 2);
        });

        // Performance & Payroll
        modelBuilder.Entity<Kpi>(entity =>
        {
            entity.ToTable("kpis");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetRevenue).HasPrecision(18, 2);
            entity.Property(e => e.AchievedRevenue).HasPrecision(18, 2);
        });

        modelBuilder.Entity<CommissionPolicy>(entity =>
        {
            entity.ToTable("commission_policies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MinThreshold).HasPrecision(18, 2);
            entity.Property(e => e.MaxThreshold).HasPrecision(18, 2);
            entity.Property(e => e.RatePercent).HasPrecision(5, 2);
        });

        modelBuilder.Entity<Commission>(entity =>
        {
            entity.ToTable("commissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CalculatedAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PayrollPeriod>(entity =>
        {
            entity.ToTable("payroll_periods");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<PayrollRecord>(entity =>
        {
            entity.ToTable("payroll_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BaseSalary).HasPrecision(18, 2);
            entity.Property(e => e.TotalCommission).HasPrecision(18, 2);
            entity.Property(e => e.Bonuses).HasPrecision(18, 2);
            entity.Property(e => e.Deductions).HasPrecision(18, 2);
            entity.Property(e => e.NetSalary).HasPrecision(18, 2);
            entity.HasOne(e => e.PayrollPeriod).WithMany(p => p.Records).HasForeignKey(e => e.PayrollPeriodId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PayrollAdjustment>(entity =>
        {
            entity.ToTable("payroll_adjustments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(e => e.PayrollRecord).WithMany(r => r.Adjustments).HasForeignKey(e => e.PayrollRecordId).OnDelete(DeleteBehavior.Cascade);
        });

        // Automation & Notifications
        modelBuilder.Entity<WorkflowDefinition>(entity =>
        {
            entity.ToTable("workflow_definitions");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<WorkflowExecution>(entity =>
        {
            entity.ToTable("workflow_executions");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Workflow).WithMany().HasForeignKey(e => e.WorkflowId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RecipientUserId);
            entity.HasIndex(e => e.IsRead);
            entity.HasOne(e => e.RecipientUser).WithMany().HasForeignKey(e => e.RecipientUserId).OnDelete(DeleteBehavior.Cascade);
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
