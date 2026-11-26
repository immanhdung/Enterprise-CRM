using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.Auth;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Infrastructure.Persistence;
using EnterpriseCRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseCRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<EnterpriseCRM.Application.Features.Leads.ILeadService, LeadService>();
        services.AddScoped<EnterpriseCRM.Application.Features.Accounts.IAccountService, AccountService>();
        services.AddScoped<EnterpriseCRM.Application.Features.Contacts.IContactService, ContactService>();
        services.AddScoped<EnterpriseCRM.Application.Features.Deals.IDealService, DealService>();
        services.AddScoped<EnterpriseCRM.Application.Features.TasksAndActivities.ITaskService, TaskService>();
        services.AddScoped<EnterpriseCRM.Application.Features.TasksAndActivities.IAppointmentService, AppointmentService>();
        services.AddScoped<EnterpriseCRM.Application.Features.TasksAndActivities.ITimelineService, TimelineService>();

        // Phase 4 Services
        services.AddScoped<IDocumentStorageService, DocumentStorageService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<EnterpriseCRM.Application.Features.Services.IServiceCatalogService, ServiceCatalogService>();
        services.AddScoped<EnterpriseCRM.Application.Features.Quotations.IPdfGenerationService, PdfGenerationService>();
        services.AddScoped<EnterpriseCRM.Application.Features.Quotations.IQuotationService, QuotationService>();
        services.AddScoped<EnterpriseCRM.Application.Features.Contracts.IContractTemplateService, ContractTemplateService>();
        services.AddScoped<EnterpriseCRM.Application.Features.Contracts.IContractService, ContractService>();
        services.AddScoped<EnterpriseCRM.Application.Features.ElectronicSignature.IElectronicSignatureProvider, MockElectronicSignatureProvider>();
        services.AddScoped<EnterpriseCRM.Application.Features.ElectronicSignature.IElectronicSignatureService, ElectronicSignatureService>();

        // Phase 5 Finance & Project Management Services
        services.AddScoped<EnterpriseCRM.Application.Features.Finance.IPaymentScheduleService, PaymentScheduleService>();
        services.AddScoped<EnterpriseCRM.Application.Features.Finance.IPaymentService, PaymentService>();
        services.AddScoped<EnterpriseCRM.Application.Features.Finance.IRevenueService, RevenueService>();
        services.AddScoped<EnterpriseCRM.Application.Features.Finance.IReceivableService, ReceivableService>();
        services.AddScoped<EnterpriseCRM.Application.Features.Finance.IElectronicInvoiceProvider, MockElectronicInvoiceProvider>();
        services.AddScoped<EnterpriseCRM.Application.Features.Finance.IInvoiceService, InvoiceService>();
        services.AddScoped<EnterpriseCRM.Application.Features.Finance.IFinancialReconciliationService, FinancialReconciliationService>();

        // Phase 6 Performance, KPI, Commission & Payroll Services
        services.AddScoped<EnterpriseCRM.Application.Features.PerformanceAndCompensation.ICompensationAuditService, CompensationAuditService>();
        services.AddScoped<EnterpriseCRM.Application.Features.PerformanceAndCompensation.IKpiService, KpiService>();
        services.AddScoped<EnterpriseCRM.Application.Features.PerformanceAndCompensation.ISalesPerformanceService, SalesPerformanceService>();
        services.AddScoped<EnterpriseCRM.Application.Features.PerformanceAndCompensation.ICommissionService, CommissionService>();
        services.AddScoped<EnterpriseCRM.Application.Features.PerformanceAndCompensation.IBonusService, BonusService>();
        services.AddScoped<EnterpriseCRM.Application.Features.PerformanceAndCompensation.IPayrollService, PayrollService>();

        // Phase 7 Automation & AI Intelligence Services
        services.AddHttpClient<IAiProvider, AiProvider>();
        services.AddScoped<IWorkflowEngine, WorkflowEngine>();
        services.AddScoped<ILeadAssignmentEngine, LeadAssignmentEngine>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IAiAssistantService, AiAssistantService>();
        services.AddScoped<IBusinessQueryService, BusinessQueryService>();
        services.AddScoped<ISalesAnalyticsService, SalesAnalyticsService>();
        services.AddScoped<IRevenueForecastingService, RevenueForecastingService>();
        services.AddScoped<IAnomalyDetectionService, AnomalyDetectionService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IRoleDashboardService, RoleDashboardService>();
        services.AddHostedService<CRMAutomationBackgroundWorker>();

        // Build Supabase PostgreSQL Connection String
        var connectionString = BuildPostgreSqlConnectionString(configuration);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    public static string BuildPostgreSqlConnectionString(IConfiguration configuration)
    {
        var configuredConn = configuration.GetConnectionString("DefaultConnection");

        // Check configuration (including User Secrets) and environment variables
        var dbPassword = configuration["SUPABASE_DB_PASSWORD"]
                      ?? configuration["Database:Password"]
                      ?? Environment.GetEnvironmentVariable("SUPABASE_DB_PASSWORD")
                      ?? Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        var host = configuration["Database:Host"]
                ?? Environment.GetEnvironmentVariable("SUPABASE_DB_HOST")
                ?? "aws-0-ap-northeast-2.pooler.supabase.com";

        var port = configuration["Database:Port"]
                ?? Environment.GetEnvironmentVariable("SUPABASE_DB_PORT")
                ?? "5432"; // 5432 for session pooler (supports DDL and queries reliably)

        var database = configuration["Database:Name"]
                    ?? Environment.GetEnvironmentVariable("SUPABASE_DB_NAME")
                    ?? "postgres";

        var username = configuration["Database:Username"]
                    ?? Environment.GetEnvironmentVariable("SUPABASE_DB_USERNAME")
                    ?? "postgres.ehmjgxwrmzlccnivqqxb";

        if (!string.IsNullOrWhiteSpace(configuredConn))
        {
            if (!string.IsNullOrWhiteSpace(dbPassword) && configuredConn.Contains("{SUPABASE_DB_PASSWORD}"))
            {
                return configuredConn.Replace("{SUPABASE_DB_PASSWORD}", dbPassword);
            }
            return configuredConn;
        }

        // Construct standard connection string
        var pwdPart = string.IsNullOrWhiteSpace(dbPassword) ? "" : $"Password={dbPassword};";
        return $"Host={host};Port={port};Database={database};Username={username};{pwdPart}SSL Mode=Require;Trust Server Certificate=true;Include Error Detail=true;";
    }
}
