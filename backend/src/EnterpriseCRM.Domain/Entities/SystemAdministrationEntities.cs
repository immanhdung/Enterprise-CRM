using EnterpriseCRM.Domain.Common;

namespace EnterpriseCRM.Domain.Entities;

public class UserSession : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string SessionToken { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAtUtc { get; set; }
}

public class OrganizationChangeHistory : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public string ChangeType { get; set; } = string.Empty; // TransferDepartment, TransferTeam, ChangePosition, Promotion, SalaryAdjustment
    public Guid? OldDepartmentId { get; set; }
    public string? OldDepartmentName { get; set; }
    public Guid? NewDepartmentId { get; set; }
    public string? NewDepartmentName { get; set; }

    public Guid? OldTeamId { get; set; }
    public string? OldTeamName { get; set; }
    public Guid? NewTeamId { get; set; }
    public string? NewTeamName { get; set; }

    public string? OldPosition { get; set; }
    public string? NewPosition { get; set; }

    public decimal? OldBaseSalary { get; set; }
    public decimal? NewBaseSalary { get; set; }

    public DateOnly EffectiveDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid ApprovedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class DataImportJob : BaseEntity
{
    public string EntityType { get; set; } = string.Empty; // Leads, Accounts, Contacts, Deals, Employees, Services, Invoices
    public string FileName { get; set; } = string.Empty;
    public int TotalRecords { get; set; } = 0;
    public int ProcessedRecords { get; set; } = 0;
    public int SuccessCount { get; set; } = 0;
    public int FailureCount { get; set; } = 0;
    public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
    public string? ErrorLogJson { get; set; }
    public Guid UploadedByUserId { get; set; }
    public User? UploadedByUser { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
}

public class DataExportJob : BaseEntity
{
    public string EntityType { get; set; } = string.Empty;
    public string Format { get; set; } = "Excel"; // Excel, CSV
    public int TotalRecords { get; set; } = 0;
    public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
    public string? FileUrl { get; set; }
    public string? FilterCriteriaJson { get; set; }
    public string? SelectedColumnsJson { get; set; }
    public Guid RequestedByUserId { get; set; }
    public User? RequestedByUser { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
}

public class ApiKey : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty; // e.g. crm_live_
    public string HashedSecret { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty; // Comma-delimited permissions
    public int RateLimitPerMinute { get; set; } = 60;
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime? LastUsedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ApiRequestLog> RequestLogs { get; set; } = new List<ApiRequestLog>();
}

public class ApiRequestLog : BaseEntity
{
    public Guid? ApiKeyId { get; set; }
    public ApiKey? ApiKey { get; set; }

    public string Endpoint { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int DurationMs { get; set; }
    public string? IpAddress { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}

public class WebhookEndpoint : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty; // HMAC key
    public string SubscribedEvents { get; set; } = string.Empty; // comma-delimited: lead.created,deal.won,payment.received
    public bool IsActive { get; set; } = true;
    public int FailureCount { get; set; } = 0;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<WebhookDelivery> Deliveries { get; set; } = new List<WebhookDelivery>();
}

public class WebhookDelivery : BaseEntity
{
    public Guid WebhookEndpointId { get; set; }
    public WebhookEndpoint WebhookEndpoint { get; set; } = null!;

    public string EventName { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public int DurationMs { get; set; }
    public int AttemptCount { get; set; } = 1;
    public string Status { get; set; } = "Success"; // Success, Failed, Retrying
    public DateTime? NextRetryAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class BackupRecord : BaseEntity
{
    public string BackupName { get; set; } = string.Empty;
    public string BackupType { get; set; } = "Manual"; // Manual, Scheduled, Snapshot
    public string Status { get; set; } = "Completed"; // InProgress, Completed, Failed, Verified
    public long FileSizeBytes { get; set; } = 0;
    public string? StoragePath { get; set; }
    public string? ChecksumSha256 { get; set; }
    public string? Notes { get; set; }
    public Guid? InitiatedByUserId { get; set; }
    public User? InitiatedByUser { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
}

public class RestoreRecord : BaseEntity
{
    public Guid BackupRecordId { get; set; }
    public BackupRecord BackupRecord { get; set; } = null!;

    public string Status { get; set; } = "Verified"; // Verified, DrillSuccessful, Failed
    public string TargetEnvironment { get; set; } = "SandboxVerification";
    public string? ExecutionLog { get; set; }
    public Guid VerifiedByUserId { get; set; }
    public User? VerifiedByUser { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
}

public class SystemConfiguration : BaseEntity
{
    public string Category { get; set; } = "General"; // General, CRM, Security, Notification, Integration
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEncrypted { get; set; } = false;
    public Guid? UpdatedByUserId { get; set; }
    public User? UpdatedByUser { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ConfigurationHistory> Histories { get; set; } = new List<ConfigurationHistory>();
}

public class ConfigurationHistory : BaseEntity
{
    public Guid ConfigurationId { get; set; }
    public SystemConfiguration Configuration { get; set; } = null!;

    public string? OldValue { get; set; }
    public string NewValue { get; set; } = string.Empty;
    public Guid? ChangedByUserId { get; set; }
    public User? ChangedByUser { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
