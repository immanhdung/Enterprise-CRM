using EnterpriseCRM.Application.Common.Models;

namespace EnterpriseCRM.Application.Features.Contacts;

public interface IContactService
{
    Task<ApiResponse<PaginatedList<ContactDto>>> GetContactsAsync(
        string? search,
        Guid? accountId,
        string? status,
        bool? isPrimary,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ContactDto>> GetContactByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<ContactDto>> CreateContactAsync(CreateContactRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<ContactDto>> UpdateContactAsync(Guid id, UpdateContactRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteContactAsync(Guid id, CancellationToken cancellationToken = default);
}
