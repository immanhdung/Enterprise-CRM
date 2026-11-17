namespace EnterpriseCRM.Application.Features.Finance;

public interface IFinancialReconciliationService
{
    Task<IReadOnlyList<FinancialReconciliationRecordDto>> GetReconciliationHistoryAsync(CancellationToken cancellationToken = default);
    Task<FinancialReconciliationRecordDto?> GetReconciliationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FinancialReconciliationRecordDto> RunReconciliationAsync(RunReconciliationRequest request, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default);
    Task<ReconciliationItemDto> ResolveDiscrepancyAsync(Guid itemId, ResolveDiscrepancyRequest request, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default);
}
