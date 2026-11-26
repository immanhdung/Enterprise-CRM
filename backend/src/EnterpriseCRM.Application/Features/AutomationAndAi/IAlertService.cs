namespace EnterpriseCRM.Application.Features.AutomationAndAi;

public interface IAlertService
{
    Task<List<AlertRuleDto>> GetRulesAsync(CancellationToken cancellationToken = default);
    Task<AlertRuleDto> CreateRuleAsync(CreateAlertRuleRequest request, CancellationToken cancellationToken = default);
    Task<AlertRuleDto> UpdateRuleAsync(Guid id, CreateAlertRuleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<BusinessAlertDto>> GetActiveAlertsAsync(string? category = null, string? severity = null, CancellationToken cancellationToken = default);
    Task<BusinessAlertDto?> AcknowledgeAlertAsync(Guid id, string? notes = null, CancellationToken cancellationToken = default);
    Task<BusinessAlertDto?> ResolveAlertAsync(Guid id, string? notes = null, CancellationToken cancellationToken = default);
    Task<int> ScanAndTriggerAlertsAsync(CancellationToken cancellationToken = default);
    Task<NotificationPreferenceDto> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<NotificationPreferenceDto> UpdateUserPreferencesAsync(Guid userId, UpdateNotificationPreferenceRequest request, CancellationToken cancellationToken = default);
}
