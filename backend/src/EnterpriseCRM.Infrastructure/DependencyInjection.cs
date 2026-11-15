using EnterpriseCRM.Application.Common.Interfaces;
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
        services.AddSingleton<IDateTimeService, DateTimeService>();

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

        // Check for specific environment variables
        var dbPassword = Environment.GetEnvironmentVariable("SUPABASE_DB_PASSWORD")
                      ?? Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")
                      ?? configuration["Database:Password"];

        var host = configuration["Database:Host"]
                ?? Environment.GetEnvironmentVariable("SUPABASE_DB_HOST")
                ?? "db.ehmjgxwrmzlccnivqqxb.supabase.co";

        var port = configuration["Database:Port"]
                ?? Environment.GetEnvironmentVariable("SUPABASE_DB_PORT")
                ?? "5432";

        var database = configuration["Database:Name"]
                    ?? Environment.GetEnvironmentVariable("SUPABASE_DB_NAME")
                    ?? "postgres";

        var username = configuration["Database:Username"]
                    ?? Environment.GetEnvironmentVariable("SUPABASE_DB_USERNAME")
                    ?? "postgres";

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
