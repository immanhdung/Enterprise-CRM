using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.TasksAndActivities;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class TimelineService : ITimelineService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TimelineService> _logger;

    public TimelineService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<TimelineService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<IReadOnlyList<TimelineEventDto>>> GetTimelineAsync(
        Guid? leadId,
        Guid? accountId,
        Guid? contactId,
        Guid? dealId,
        string? activityType,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.CrmActivities.AsQueryable();

        if (leadId.HasValue) query = query.Where(a => a.LeadId == leadId.Value);
        if (accountId.HasValue) query = query.Where(a => a.AccountId == accountId.Value);
        if (contactId.HasValue) query = query.Where(a => a.ContactId == contactId.Value);
        if (dealId.HasValue) query = query.Where(a => a.DealId == dealId.Value);

        if (!string.IsNullOrWhiteSpace(activityType))
        {
            query = query.Where(a => a.ActivityType == activityType);
        }

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(Math.Clamp(limit, 1, 100))
            .Select(a => new TimelineEventDto(
                a.Id,
                a.ActivityType,
                a.Subject,
                a.Notes,
                a.PerformedByName,
                a.PerformedByUserId,
                a.ScheduledAt,
                a.CompletedAt,
                a.CreatedAt,
                a.LeadId,
                a.AccountId,
                a.ContactId,
                a.DealId
            ))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<TimelineEventDto>>.SuccessResponse(items);
    }

    public async Task<ApiResponse<TimelineEventDto>> RecordActivityAsync(
        string activityType,
        string subject,
        string? notes,
        Guid? leadId,
        Guid? accountId,
        Guid? contactId,
        Guid? dealId,
        DateTime? scheduledAt = null,
        DateTime? completedAt = null,
        CancellationToken cancellationToken = default)
    {
        var act = new CrmActivity
        {
            Id = Guid.NewGuid(),
            ActivityType = activityType,
            Subject = subject.Trim(),
            Notes = notes?.Trim(),
            LeadId = leadId,
            AccountId = accountId,
            ContactId = contactId,
            DealId = dealId,
            ScheduledAt = scheduledAt,
            CompletedAt = completedAt ?? DateTime.UtcNow,
            PerformedByUserId = _currentUserService.UserId,
            PerformedByName = _currentUserService.Email ?? "User"
        };

        _context.CrmActivities.Add(act);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new TimelineEventDto(
            act.Id,
            act.ActivityType,
            act.Subject,
            act.Notes,
            act.PerformedByName,
            act.PerformedByUserId,
            act.ScheduledAt,
            act.CompletedAt,
            act.CreatedAt,
            act.LeadId,
            act.AccountId,
            act.ContactId,
            act.DealId
        );

        return ApiResponse<TimelineEventDto>.SuccessResponse(dto, "Activity logged successfully.");
    }
}
