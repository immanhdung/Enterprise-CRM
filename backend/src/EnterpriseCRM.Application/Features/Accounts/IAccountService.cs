using EnterpriseCRM.Application.Common.Models;

namespace EnterpriseCRM.Application.Features.Accounts;

public interface IAccountService
{
    Task<ApiResponse<PaginatedList<AccountDto>>> GetAccountsAsync(
        string? search,
        string? classification,
        string? industry,
        string? status,
        Guid? assignedEmployeeId,
        string? sortBy,
        bool sortDescending,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<AccountDetailDto>> GetAccountDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<AccountDto>> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<AccountDto>> UpdateAccountAsync(Guid id, UpdateAccountRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteAccountAsync(Guid id, CancellationToken cancellationToken = default);
}
