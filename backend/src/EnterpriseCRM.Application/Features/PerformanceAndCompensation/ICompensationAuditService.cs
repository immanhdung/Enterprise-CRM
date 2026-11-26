namespace EnterpriseCRM.Application.Features.PerformanceAndCompensation;

public interface ICompensationAuditService
{
    Task LogActionAsync(
        string entityType,
        Guid entityId,
        string action,
        string? oldValueJson,
        string? newValueJson,
        string reason,
        CancellationToken ct = default
    );

    Task<List<CompensationAuditLogDto>> GetAuditLogsAsync(
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken ct = default
    );
}
