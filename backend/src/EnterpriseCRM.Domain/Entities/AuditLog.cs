using EnterpriseCRM.Domain.Common;

namespace EnterpriseCRM.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // e.g. Login, Create, Update, Delete
    public string Module { get; set; } = "General"; // Auth, Users, Roles, Leads, Deals, Finance, Payroll, Workflows, Admin
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
