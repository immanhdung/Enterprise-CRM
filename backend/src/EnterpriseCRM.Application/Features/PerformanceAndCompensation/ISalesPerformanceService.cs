namespace EnterpriseCRM.Application.Features.PerformanceAndCompensation;

public interface ISalesPerformanceService
{
    Task<SalesPerformanceSummaryDto> GetPerformanceSummaryAsync(
        int? month,
        int? year,
        Guid? employeeId,
        Guid? departmentId,
        Guid? teamId,
        CancellationToken ct = default
    );

    Task<List<PerformanceRankingItemDto>> GetEmployeeRankingsAsync(
        int? month,
        int? year,
        Guid? departmentId,
        CancellationToken ct = default
    );

    Task<List<PerformanceTimelineItemDto>> GetTimelineAnalyticsAsync(
        string timeframe, // "monthly", "quarterly", "yearly"
        int? year,
        Guid? employeeId,
        Guid? departmentId,
        CancellationToken ct = default
    );
}
