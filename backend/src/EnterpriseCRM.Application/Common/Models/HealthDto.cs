namespace EnterpriseCRM.Application.Common.Models;

public class SystemHealthDto
{
    public string Status { get; set; } = "Healthy";
    public string Environment { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public DateTime ServerTimeUtc { get; set; } = DateTime.UtcNow;
    public TimeSpan Uptime { get; set; }
    public DatabaseHealthDto Database { get; set; } = new();
}

public class DatabaseHealthDto
{
    public bool IsConnected { get; set; }
    public string Provider { get; set; } = "PostgreSQL (Supabase)";
    public string Host { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public double ResponseTimeMs { get; set; }
    public string? ServerVersion { get; set; }
    public string? Message { get; set; }
}
