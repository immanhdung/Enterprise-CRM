using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Domain.Enums;

namespace EnterpriseCRM.Application.Features.Deals;

public interface IDealService
{
    Task<ApiResponse<PaginatedList<DealDto>>> GetDealsAsync(
        string? search,
        Guid? accountId,
        Guid? stageId,
        DealStatus? status,
        Guid? assignedEmployeeId,
        DateTime? fromDate,
        DateTime? toDate,
        string? sortBy,
        bool sortDescending,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<DealDto>> GetDealByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<DealDto>> CreateDealAsync(CreateDealRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<DealDto>> UpdateDealAsync(Guid id, UpdateDealRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<DealDto>> UpdateDealStageAsync(Guid id, UpdateDealStageRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteDealAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<KanbanBoardDto>> GetKanbanBoardAsync(
        Guid? assignedEmployeeId,
        DealStatus? status,
        CancellationToken cancellationToken = default);
}
