using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EnterpriseCRM.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Identity & Organization
    DbSet<User> Users { get; }
    DbSet<Employee> Employees { get; }
    DbSet<Department> Departments { get; }
    DbSet<Team> Teams { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<AuditLog> AuditLogs { get; }

    // CRM
    DbSet<LeadSource> LeadSources { get; }
    DbSet<Lead> Leads { get; }
    DbSet<LeadActivity> LeadActivities { get; }
    DbSet<Account> Accounts { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<PipelineStage> PipelineStages { get; }
    DbSet<Deal> Deals { get; }
    DbSet<DealStageHistory> DealStageHistories { get; }
    DbSet<TaskItem> Tasks { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<CrmActivity> CrmActivities { get; }

    // Billing & Operations
    DbSet<ServiceCategory> ServiceCategories { get; }
    DbSet<ServiceItem> Services { get; }
    DbSet<PriceList> PriceLists { get; }
    DbSet<PriceListItem> PriceListItems { get; }
    DbSet<ServicePriceHistory> ServicePriceHistories { get; }
    DbSet<Quotation> Quotations { get; }
    DbSet<QuotationItem> QuotationItems { get; }
    DbSet<QuotationApprovalHistory> QuotationApprovalHistories { get; }
    DbSet<ContractTemplate> ContractTemplates { get; }
    DbSet<Contract> Contracts { get; }
    DbSet<ContractItem> ContractItems { get; }
    DbSet<ContractAppendix> ContractAppendices { get; }
    DbSet<ContractHistory> ContractHistories { get; }
    DbSet<SignatureRequest> SignatureRequests { get; }
    DbSet<Project> Projects { get; }
    DbSet<PaymentSchedule> PaymentSchedules { get; }
    DbSet<Payment> Payments { get; }
    DbSet<InvoiceRequest> InvoiceRequests { get; }
    DbSet<Invoice> Invoices { get; }

    // Performance, Payroll & Automation
    DbSet<Kpi> Kpis { get; }
    DbSet<CommissionPolicy> CommissionPolicies { get; }
    DbSet<Commission> Commissions { get; }
    DbSet<PayrollPeriod> PayrollPeriods { get; }
    DbSet<PayrollRecord> PayrollRecords { get; }
    DbSet<PayrollAdjustment> PayrollAdjustments { get; }
    DbSet<WorkflowDefinition> WorkflowDefinitions { get; }
    DbSet<WorkflowExecution> WorkflowExecutions { get; }
    DbSet<Notification> Notifications { get; }

    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
