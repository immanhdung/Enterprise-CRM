using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Domain.Enums;

namespace EnterpriseCRM.Application.Features.Leads;

public interface ILeadService
{
    Task<ApiResponse<PaginatedList<LeadDto>>> GetLeadsAsync(
        string? search,
        LeadStatus? status,
        Guid? sourceId,
        Guid? assignedEmployeeId,
        DateTime? fromDate,
        DateTime? toDate,
        string? sortBy,
        bool sortDescending,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<LeadDto>> GetLeadByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<DuplicateCheckResult>> CheckDuplicateAsync(DuplicateCheckRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<LeadDto>> CreateLeadAsync(CreateLeadRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<LeadDto>> UpdateLeadAsync(Guid id, UpdateLeadRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteLeadAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<LeadDto>> AssignLeadAsync(Guid id, AssignLeadRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<ConvertLeadResult>> ConvertLeadAsync(Guid id, ConvertLeadRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<LeadActivityDto>>> GetLeadActivitiesAsync(Guid leadId, CancellationToken cancellationToken = default);

    Task<ApiResponse<LeadActivityDto>> AddLeadActivityAsync(Guid leadId, CreateLeadActivityRequest request, CancellationToken cancellationToken = default);

    Task<byte[]> ExportLeadsCsvAsync(
        string? search,
        LeadStatus? status,
        Guid? sourceId,
        Guid? assignedEmployeeId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<LeadImportResult>> ImportLeadsAsync(IReadOnlyList<LeadImportItem> items, CancellationToken cancellationToken = default);
}
