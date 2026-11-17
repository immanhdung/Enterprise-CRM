using EnterpriseCRM.Domain.Entities;

namespace EnterpriseCRM.Application.Features.Finance;

public record ElectronicInvoiceIssueResult(
    bool IsSuccess,
    string? ElectronicInvoiceId,
    string? InvoiceNumber,
    string? InvoiceSeries,
    string? LookupCode,
    string? LookupUrl,
    string? ErrorMessage
);

public record ElectronicInvoiceCancelResult(
    bool IsSuccess,
    string? ErrorMessage
);

public record ElectronicInvoiceStatusResult(
    bool IsSuccess,
    string? Status,
    string? ErrorMessage
);

public interface IElectronicInvoiceProvider
{
    string ProviderName { get; }
    Task<ElectronicInvoiceIssueResult> IssueInvoiceAsync(Invoice invoice, IReadOnlyList<InvoiceItem> items, CancellationToken cancellationToken = default);
    Task<ElectronicInvoiceCancelResult> CancelInvoiceAsync(string electronicInvoiceId, string reason, CancellationToken cancellationToken = default);
    Task<ElectronicInvoiceStatusResult> SyncStatusAsync(string electronicInvoiceId, CancellationToken cancellationToken = default);
}
