namespace EnterpriseCRM.Application.Features.SystemAdministration;

// ==========================================
// 1. User & Account Management DTOs
// ==========================================

public record UserSummaryDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    bool IsActive,
    string Status,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles,
    string? DepartmentName,
    string? TeamName,
    string? Position,
    DateTime CreatedAt
);

public record UserDetailDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    bool IsActive,
    string Status,
    int FailedLoginAttempts,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? TeamId,
    string? TeamName,
    string? Position,
    string? Phone,
    decimal BaseSalary,
    DateTime CreatedAt
);

public record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    List<string> Roles,
    Guid? DepartmentId,
    Guid? TeamId,
    string? Position,
    string? Phone,
    decimal? BaseSalary
);

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string? Position,
    string? Phone,
    Guid? DepartmentId,
    Guid? TeamId,
    decimal? BaseSalary,
    string? Status
);

public record AdminResetPasswordRequest(
    string NewPassword
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

public record UpdateUserStatusRequest(
    string Status // Active, Inactive, Suspended, PendingActivation
);

public record AssignUserRolesRequest(
    List<string> Roles
);

public record UserSessionDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc,
    bool IsRevoked,
    DateTime? RevokedAtUtc
);

// ==========================================
// 2. Organization Management DTOs
// ==========================================

public record EmployeeHierarchyDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string EmployeeCode,
    string Position,
    string Phone,
    bool IsActive,
    Guid? TeamId,
    string? TeamName
);

public record TeamHierarchyDto(
    Guid Id,
    string Name,
    Guid? TeamLeaderId,
    string? TeamLeaderName,
    int MemberCount,
    IReadOnlyList<EmployeeHierarchyDto> Members
);

public record DepartmentHierarchyDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid? ManagerId,
    string? ManagerName,
    int TotalEmployees,
    IReadOnlyList<TeamHierarchyDto> Teams,
    IReadOnlyList<EmployeeHierarchyDto> DirectEmployees
);

public record TransferEmployeeRequest(
    Guid EmployeeId,
    Guid NewDepartmentId,
    Guid? NewTeamId,
    string? NewPosition,
    decimal? NewBaseSalary,
    DateOnly EffectiveDate,
    string Reason
);

public record OrganizationChangeHistoryDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string ChangeType,
    string? OldDepartmentName,
    string? NewDepartmentName,
    string? OldTeamName,
    string? NewTeamName,
    string? OldPosition,
    string? NewPosition,
    decimal? OldBaseSalary,
    decimal? NewBaseSalary,
    DateOnly EffectiveDate,
    string Reason,
    DateTime CreatedAtUtc
);

// ==========================================
// 3. RBAC & Data Scope Management DTOs
// ==========================================

public record RoleDto(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    int UsersCount,
    IReadOnlyList<string> Permissions
);

public record CreateRoleRequest(
    string Name,
    string Description,
    List<string> Permissions
);

public record UpdateRoleRequest(
    string Description,
    List<string> Permissions
);

public record PermissionDto(
    Guid Id,
    string Code,
    string Module,
    string Description
);

public record RolePermissionEntryDto(
    string RoleName,
    bool HasPermission,
    string DataScope // Own, Assigned, Team, Department, Company
);

public record PermissionMatrixRowDto(
    string PermissionCode,
    string Module,
    string Description,
    Dictionary<string, RolePermissionEntryDto> Roles
);

public record RolePermissionMatrixDto(
    IReadOnlyList<string> RoleNames,
    IReadOnlyList<PermissionMatrixRowDto> Rows
);

public record UpdateRolePermissionMatrixRequest(
    string RoleName,
    string PermissionCode,
    bool IsGranted,
    string? DataScope
);

// ==========================================
// 4. Audit Log & System Activity DTOs
// ==========================================

public record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string UserEmail,
    string Action,
    string Module,
    string EntityName,
    string? EntityId,
    string? IpAddress,
    string? UserAgent,
    bool IsSuccess,
    DateTime Timestamp
);

public record AuditLogDetailDto(
    Guid Id,
    Guid? UserId,
    string UserEmail,
    string Action,
    string Module,
    string EntityName,
    string? EntityId,
    string? IpAddress,
    string? UserAgent,
    string? CorrelationId,
    bool IsSuccess,
    string? OldValuesJson,
    string? NewValuesJson,
    DateTime Timestamp
);

public record AuditLogFilterRequest(
    string? Search,
    string? Module,
    string? Action,
    Guid? UserId,
    DateTime? StartDate,
    DateTime? EndDate,
    int PageNumber = 1,
    int PageSize = 50
);

// ==========================================
// 5. Excel Import / Export Center DTOs
// ==========================================

public record DataImportJobDto(
    Guid Id,
    string EntityType,
    string FileName,
    int TotalRecords,
    int ProcessedRecords,
    int SuccessCount,
    int FailureCount,
    string Status,
    string? ErrorLogJson,
    string UploadedByEmail,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc
);

public record DataExportJobDto(
    Guid Id,
    string EntityType,
    string Format,
    int TotalRecords,
    string Status,
    string? FileUrl,
    string RequestedByEmail,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc
);

public record InitiateImportRequest(
    string EntityType,
    string FileName,
    List<Dictionary<string, object?>> Rows
);

public record ImportExecutionResultDto(
    Guid JobId,
    string EntityType,
    int TotalRecords,
    int SuccessCount,
    int FailureCount,
    List<string> Errors
);

public record InitiateExportRequest(
    string EntityType,
    string Format = "Excel", // Excel, CSV
    List<string>? SelectedColumns = null,
    string? FilterJson = null
);

// ==========================================
// 6. API & Webhook Management DTOs
// ==========================================

public record ApiKeyDto(
    Guid Id,
    string Name,
    string KeyPrefix,
    string Scopes,
    int RateLimitPerMinute,
    bool IsActive,
    DateTime? ExpiresAtUtc,
    DateTime? LastUsedAtUtc,
    string CreatedByEmail,
    DateTime CreatedAtUtc
);

public record CreateApiKeyRequest(
    string Name,
    List<string> Scopes,
    int RateLimitPerMinute = 60,
    DateTime? ExpiresAtUtc = null
);

public record ApiKeyCreatedResultDto(
    Guid Id,
    string Name,
    string PlaintextKey, // Only shown once at creation!
    string KeyPrefix,
    DateTime? ExpiresAtUtc
);

public record ApiRequestLogDto(
    Guid Id,
    Guid? ApiKeyId,
    string? ApiKeyName,
    string Endpoint,
    string HttpMethod,
    int StatusCode,
    int DurationMs,
    string? IpAddress,
    DateTime TimestampUtc
);

public record WebhookEndpointDto(
    Guid Id,
    string Name,
    string Url,
    List<string> SubscribedEvents,
    bool IsActive,
    int FailureCount,
    DateTime CreatedAtUtc
);

public record CreateWebhookEndpointRequest(
    string Name,
    string Url,
    List<string> SubscribedEvents
);

public record UpdateWebhookEndpointRequest(
    string Name,
    string Url,
    List<string> SubscribedEvents,
    bool IsActive
);

public record WebhookDeliveryDto(
    Guid Id,
    Guid WebhookEndpointId,
    string EventName,
    string PayloadJson,
    int? ResponseStatusCode,
    string? ResponseBody,
    int DurationMs,
    int AttemptCount,
    string Status,
    DateTime? NextRetryAtUtc,
    DateTime CreatedAtUtc
);

// ==========================================
// 7. Notification Center DTOs
// ==========================================

public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string? Link,
    bool IsRead,
    string Type,
    string Category,
    string Priority,
    DateTime CreatedAt
);

public record NotificationSummaryDto(
    int TotalCount,
    int UnreadCount,
    IReadOnlyList<NotificationDto> RecentNotifications
);

public record CreateNotificationRequest(
    Guid? RecipientUserId, // null means broadcast to all
    string Title,
    string Message,
    string? Link,
    string Type = "Info",
    string Category = "System",
    string Priority = "Normal"
);

// ==========================================
// 8. Backup & Restore Management DTOs
// ==========================================

public record BackupRecordDto(
    Guid Id,
    string BackupName,
    string BackupType,
    string Status,
    long FileSizeBytes,
    string FileSizeFormatted,
    string? StoragePath,
    string? ChecksumSha256,
    string? Notes,
    string? InitiatedByEmail,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc
);

public record RestoreRecordDto(
    Guid Id,
    Guid BackupRecordId,
    string BackupName,
    string Status,
    string TargetEnvironment,
    string? ExecutionLog,
    string VerifiedByEmail,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc
);

public record InitiateBackupRequest(
    string BackupName,
    string? Notes
);

public record ExecuteRestoreDrillRequest(
    Guid BackupRecordId,
    string TargetEnvironment = "SandboxVerification"
);

public record SupabasePitrStatusDto(
    bool IsPitrEnabled,
    string PlanTier,
    int RetentionDays,
    DateTime EarliestRestorePointUtc,
    DateTime LatestRestorePointUtc,
    string StatusMessage
);

// ==========================================
// 9. System Configuration DTOs
// ==========================================

public record SystemConfigurationDto(
    Guid Id,
    string Category,
    string Key,
    string Value,
    string? Description,
    bool IsEncrypted,
    string? UpdatedByEmail,
    DateTime UpdatedAtUtc
);

public record UpdateConfigurationRequest(
    string Category,
    string Key,
    string Value,
    string? Description
);

public record BulkUpdateConfigurationRequest(
    List<UpdateConfigurationRequest> Settings
);

public record ConfigurationHistoryDto(
    Guid Id,
    Guid ConfigurationId,
    string? OldValue,
    string NewValue,
    string? ChangedByEmail,
    DateTime TimestampUtc
);
