using System.Diagnostics;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : BaseApiController
{
    private static readonly DateTime StartTime = DateTime.UtcNow;
    private readonly IApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IApplicationDbContext context,
        IWebHostEnvironment env,
        IConfiguration config,
        ILogger<HealthController> logger)
    {
        _context = context;
        _env = env;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Basic API health check
    /// </summary>
    [HttpGet]
    public ActionResult<ApiResponse<SystemHealthDto>> GetHealth()
    {
        var uptime = DateTime.UtcNow - StartTime;
        var health = new SystemHealthDto
        {
            Status = "Healthy",
            Environment = _env.EnvironmentName,
            Version = "1.0.0",
            ServerTimeUtc = DateTime.UtcNow,
            Uptime = uptime,
            Database = new DatabaseHealthDto
            {
                Provider = "PostgreSQL (Supabase)",
                Host = _config["Database:Host"] ?? "db.ehmjgxwrmzlccnivqqxb.supabase.co",
                DatabaseName = _config["Database:Name"] ?? "postgres",
                IsConnected = false,
                Message = "Use /api/v1/health/database to verify live database connectivity."
            }
        };

        return OkResponse(health, "Enterprise CRM API is running healthy.");
    }

    /// <summary>
    /// Live Supabase PostgreSQL connection & query verification
    /// </summary>
    [HttpGet("database")]
    public async Task<ActionResult<ApiResponse<DatabaseHealthDto>>> GetDatabaseHealth()
    {
        var host = _config["Database:Host"] ?? "db.ehmjgxwrmzlccnivqqxb.supabase.co";
        var dbName = _config["Database:Name"] ?? "postgres";
        var stopwatch = Stopwatch.StartNew();

        var dbHealth = new DatabaseHealthDto
        {
            Provider = "PostgreSQL (Supabase)",
            Host = host,
            DatabaseName = dbName
        };

        try
        {
            _logger.LogInformation("Testing database connectivity to Supabase PostgreSQL at {Host}...", host);

            // Execute test query on Supabase PostgreSQL
            var canConnect = await _context.Database.CanConnectAsync();
            stopwatch.Stop();
            dbHealth.ResponseTimeMs = stopwatch.Elapsed.TotalMilliseconds;

            if (canConnect)
            {
                dbHealth.IsConnected = true;
                dbHealth.Message = "Successfully connected to Supabase PostgreSQL.";
                return OkResponse(dbHealth, "Database connection verified successfully.");
            }
            else
            {
                dbHealth.IsConnected = false;
                dbHealth.Message = "Cannot connect to Supabase PostgreSQL. Please ensure SUPABASE_DB_PASSWORD is configured and network access is permitted.";
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<DatabaseHealthDto>.Failed(new List<string> { dbHealth.Message }, "Database connection unavailable."));
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            dbHealth.ResponseTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            dbHealth.IsConnected = false;
            dbHealth.Message = $"Database connection failed: {ex.Message}";
            _logger.LogError(ex, "Supabase connection test failed.");

            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ApiResponse<DatabaseHealthDto>.Failed(new List<string> { ex.Message }, "Database connection error."));
        }
    }
}
