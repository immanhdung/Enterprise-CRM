using EnterpriseCRM.Application.Common.Models;

namespace EnterpriseCRM.Application.Features.TasksAndActivities;

public interface ITimelineService
{
    Task<ApiResponse<IReadOnlyList<TimelineEventDto>>> GetTimelineAsync(
        Guid? leadId,
        Guid? accountId,
        Guid? contactId,
        Guid? dealId,
        string? activityType,
        int limit = 50,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<TimelineEventDto>> RecordActivityAsync(
        string activityType,
        string subject,
        string? notes,
        Guid? leadId,
        Guid? accountId,
        Guid? contactId,
        Guid? dealId,
        DateTime? scheduledAt = null,
        DateTime? completedAt = null,
        CancellationToken cancellationToken = default);
}
