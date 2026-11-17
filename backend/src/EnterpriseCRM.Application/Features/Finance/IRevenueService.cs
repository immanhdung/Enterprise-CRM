namespace EnterpriseCRM.Application.Features.Finance;

public interface IRevenueService
{
    Task<RevenueAnalyticsResponse> GetRevenueAnalyticsAsync(RevenueFilterParams filter, CancellationToken cancellationToken = default);
}
