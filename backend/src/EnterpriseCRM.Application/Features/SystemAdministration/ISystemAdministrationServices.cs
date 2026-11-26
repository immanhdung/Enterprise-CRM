namespace EnterpriseCRM.Application.Features.SystemAdministration;

public interface IUserAdministrationService
{
    Task<IReadOnlyList<UserSummaryDto>> GetAllUsersAsync(string? search = null, string? status = null, Guid? departmentId = null, string? role = null, CancellationToken cancellationToken = default);
    Task<UserDetailDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDetailDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDetailDto> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    Task<bool> AdminResetPasswordAsync(Guid id, string newPassword, CancellationToken cancellationToken = default);
    Task<bool> AssignRolesAsync(Guid id, List<string> roles, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSessionDto>> GetUserSessionsAsync(Guid? userId = null, CancellationToken cancellationToken = default);
    Task<bool> RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}

public interface IOrganizationAdministrationService
{
    Task<IReadOnlyList<DepartmentHierarchyDto>> GetHierarchyAsync(CancellationToken cancellationToken = default);
    Task<OrganizationChangeHistoryDto> TransferEmployeeAsync(TransferEmployeeRequest request, Guid approvedByUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrganizationChangeHistoryDto>> GetChangeHistoryAsync(Guid? employeeId = null, CancellationToken cancellationToken = default);
}

public interface IRbacAdministrationService
{
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<RoleDto?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PermissionDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
    Task<RolePermissionMatrixDto> GetPermissionMatrixAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdatePermissionMatrixEntryAsync(UpdateRolePermissionMatrixRequest request, CancellationToken cancellationToken = default);
}

public interface IAuditLogService
{
    Task<IReadOnlyList<AuditLogDto>> GetLogsAsync(AuditLogFilterRequest filter, CancellationToken cancellationToken = default);
    Task<AuditLogDetailDto?> GetLogByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<byte[]> ExportLogsAsync(AuditLogFilterRequest filter, string format = "Excel", CancellationToken cancellationToken = default);
    Task LogAsync(string action, string module, string entityName, string? entityId, object? oldValues, object? newValues, Guid? userId, string? userEmail, string? ipAddress = null, string? userAgent = null, bool isSuccess = true, CancellationToken cancellationToken = default);
}

public interface IDataImportExportService
{
    Task<IReadOnlyList<DataImportJobDto>> GetImportJobsAsync(CancellationToken cancellationToken = default);
    Task<ImportExecutionResultDto> ProcessImportAsync(InitiateImportRequest request, Guid uploadedByUserId, string userEmail, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DataExportJobDto>> GetExportJobsAsync(CancellationToken cancellationToken = default);
    Task<DataExportJobDto> InitiateExportAsync(InitiateExportRequest request, Guid requestedByUserId, string userEmail, CancellationToken cancellationToken = default);
    Task<byte[]> DownloadExportFileAsync(Guid exportJobId, CancellationToken cancellationToken = default);
}

public interface IApiAndWebhookService
{
    Task<IReadOnlyList<ApiKeyDto>> GetApiKeysAsync(CancellationToken cancellationToken = default);
    Task<ApiKeyCreatedResultDto> CreateApiKeyAsync(CreateApiKeyRequest request, Guid createdByUserId, CancellationToken cancellationToken = default);
    Task<bool> RevokeApiKeyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApiRequestLogDto>> GetApiRequestLogsAsync(int limit = 50, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WebhookEndpointDto>> GetWebhookEndpointsAsync(CancellationToken cancellationToken = default);
    Task<WebhookEndpointDto?> GetWebhookEndpointByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WebhookEndpointDto> CreateWebhookEndpointAsync(CreateWebhookEndpointRequest request, CancellationToken cancellationToken = default);
    Task<WebhookEndpointDto> UpdateWebhookEndpointAsync(Guid id, UpdateWebhookEndpointRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteWebhookEndpointAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WebhookDeliveryDto>> GetWebhookDeliveriesAsync(Guid? endpointId = null, int limit = 50, CancellationToken cancellationToken = default);
    Task<WebhookDeliveryDto> TestWebhookEndpointAsync(Guid endpointId, CancellationToken cancellationToken = default);
    Task TriggerWebhookEventAsync(string eventName, object payload, CancellationToken cancellationToken = default);
}

public interface INotificationCenterService
{
    Task<NotificationSummaryDto> GetUserNotificationsAsync(Guid userId, bool? unreadOnly = null, int limit = 50, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task<NotificationDto> CreateAndDispatchAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
}

public interface IBackupAndRestoreService
{
    Task<IReadOnlyList<BackupRecordDto>> GetBackupRecordsAsync(CancellationToken cancellationToken = default);
    Task<BackupRecordDto> InitiateBackupAsync(InitiateBackupRequest request, Guid initiatedByUserId, string userEmail, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RestoreRecordDto>> GetRestoreRecordsAsync(CancellationToken cancellationToken = default);
    Task<RestoreRecordDto> ExecuteRestoreDrillAsync(ExecuteRestoreDrillRequest request, Guid verifiedByUserId, string userEmail, CancellationToken cancellationToken = default);
    Task<SupabasePitrStatusDto> GetSupabasePitrStatusAsync(CancellationToken cancellationToken = default);
}

public interface ISystemConfigurationService
{
    Task<IReadOnlyList<SystemConfigurationDto>> GetAllSettingsAsync(string? category = null, CancellationToken cancellationToken = default);
    Task<SystemConfigurationDto> UpdateSettingAsync(UpdateConfigurationRequest request, Guid updatedByUserId, string userEmail, CancellationToken cancellationToken = default);
    Task<bool> BulkUpdateSettingsAsync(BulkUpdateConfigurationRequest request, Guid updatedByUserId, string userEmail, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConfigurationHistoryDto>> GetSettingHistoryAsync(Guid settingId, CancellationToken cancellationToken = default);
    Task InitializeDefaultSettingsAsync(CancellationToken cancellationToken = default);
}
