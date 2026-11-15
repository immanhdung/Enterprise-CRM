using EnterpriseCRM.Domain.Common;

namespace EnterpriseCRM.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // e.g. Login, Create, Update, Delete
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
