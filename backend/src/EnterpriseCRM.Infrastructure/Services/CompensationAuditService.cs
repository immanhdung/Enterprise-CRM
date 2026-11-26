using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.PerformanceAndCompensation;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class CompensationAuditService : ICompensationAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CompensationAuditService> _logger;

    public CompensationAuditService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CompensationAuditService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task LogActionAsync(
        string entityType,
        Guid entityId,
        string action,
        string? oldValueJson,
        string? newValueJson,
        string reason,
        CancellationToken ct = default)
    {
        var actorUserId = _currentUserService.UserId ?? Guid.Empty;

        var auditLog = new CompensationAuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            ActorUserId = actorUserId,
            OldValueJson = oldValueJson,
            NewValueJson = newValueJson,
            Reason = reason,
            Timestamp = DateTime.UtcNow
        };

        _context.CompensationAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Compensation audit: {Action} on {EntityType} {EntityId} by {ActorUserId}. Reason: {Reason}",
            action, entityType, entityId, actorUserId, reason);
    }

    public async Task<List<CompensationAuditLogDto>> GetAuditLogsAsync(
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken ct = default)
    {
        var query = _context.CompensationAuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (entityId.HasValue)
        {
            query = query.Where(a => a.EntityId == entityId.Value);
        }

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Take(100)
            .Select(a => new CompensationAuditLogDto(
                a.Id,
                a.EntityType,
                a.EntityId,
                a.Action,
                a.ActorUserId,
                a.OldValueJson,
                a.NewValueJson,
                a.Reason,
                a.Timestamp
            ))
            .ToListAsync(ct);
    }
}
