namespace EnterpriseCRM.Application.Features.Finance;

public interface IInvoiceService
{
    Task<PagedInvoiceResult> GetInvoicesAsync(InvoiceFilterParams filter, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetInvoiceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InvoiceDto> IssueInvoiceAsync(IssueInvoiceRequest request, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default);
    Task<InvoiceDto> CancelInvoiceAsync(Guid id, CancelInvoiceRequest request, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default);
    Task<InvoiceDto> SyncInvoiceStatusAsync(Guid id, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateInvoicePdfAsync(Guid id, CancellationToken cancellationToken = default);

    // Invoice Requests
    Task<PagedInvoiceRequestResult> GetInvoiceRequestsAsync(InvoiceRequestFilterParams filter, CancellationToken cancellationToken = default);
    Task<InvoiceRequestDto> CreateInvoiceRequestAsync(CreateInvoiceRequestInput request, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default);
    Task<InvoiceDto> ApproveAndIssueInvoiceRequestAsync(Guid invoiceRequestId, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default);
    Task<bool> RejectInvoiceRequestAsync(Guid invoiceRequestId, string reason, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default);
}
