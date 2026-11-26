namespace EnterpriseCRM.Application.Features.AutomationAndAi;

public interface IBusinessQueryService
{
    Task<BusinessQueryResultDto> ExecuteNaturalLanguageQueryAsync(BusinessQueryRequest request, CancellationToken cancellationToken = default);
}

public interface ISalesAnalyticsService
{
    Task<SalesAnalysisSummaryDto> GetPerformanceAnalysisAsync(int? month = null, int? year = null, CancellationToken cancellationToken = default);
    Task<List<BusinessRecommendationDto>> GetRecommendationsAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<bool> ApplyRecommendationAsync(Guid id, ApplyRecommendationRequest request, CancellationToken cancellationToken = default);
}

public interface IRevenueForecastingService
{
    Task<RevenueForecastDto> GetForecastAsync(int horizonDays = 30, CancellationToken cancellationToken = default);
    Task<RevenueForecastDto> RecalculateForecastAsync(int horizonDays = 30, CancellationToken cancellationToken = default);
}

public interface IAnomalyDetectionService
{
    Task<List<AnomalyDetectionDto>> GetAnomaliesAsync(string? category = null, string? status = null, CancellationToken cancellationToken = default);
    Task<int> RunAnomalyScanAsync(CancellationToken cancellationToken = default);
    Task<bool> ResolveAnomalyAsync(Guid id, ResolveAnomalyRequest request, CancellationToken cancellationToken = default);
}

public interface IReportingService
{
    Task<List<ScheduledReportDto>> GetScheduledReportsAsync(CancellationToken cancellationToken = default);
    Task<ScheduledReportDto> CreateScheduledReportAsync(CreateScheduledReportRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteScheduledReportAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<GeneratedReportDto>> GetGeneratedReportsAsync(string? reportType = null, int limit = 30, CancellationToken cancellationToken = default);
    Task<GeneratedReportDto> GenerateReportAsync(GenerateReportRequest request, CancellationToken cancellationToken = default);
    Task<ManagementReportDataDto> GetReportDataAsync(string reportType, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default);
}

public interface IRoleDashboardService
{
    Task<SalesDashboardDto> GetSalesDashboardAsync(Guid? employeeId = null, CancellationToken cancellationToken = default);
    Task<ManagerDashboardDto> GetManagerDashboardAsync(Guid? managerId = null, CancellationToken cancellationToken = default);
    Task<ExecutiveDashboardDto> GetExecutiveDashboardAsync(CancellationToken cancellationToken = default);
}
