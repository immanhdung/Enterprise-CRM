namespace EnterpriseCRM.Application.Features.Finance;

public interface IReceivableService
{
    Task<ReceivableAgingSummaryDto> GetAgingSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountsReceivableItemDto>> GetReceivablesListAsync(ReceivableFilterParams filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReceivableCollectionLogDto>> GetCollectionLogsAsync(Guid? accountId, Guid? contractId, CancellationToken cancellationToken = default);
    Task<ReceivableCollectionLogDto> AddCollectionLogAsync(CreateReceivableCollectionLogRequest request, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default);
}
