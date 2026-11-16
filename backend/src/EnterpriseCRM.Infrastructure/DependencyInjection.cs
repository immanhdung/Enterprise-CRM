using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.Auth;
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
