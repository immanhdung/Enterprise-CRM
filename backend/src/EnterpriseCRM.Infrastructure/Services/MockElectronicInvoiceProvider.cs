using EnterpriseCRM.Application.Features.Finance;
using EnterpriseCRM.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class MockElectronicInvoiceProvider : IElectronicInvoiceProvider
{
    private readonly ILogger<MockElectronicInvoiceProvider> _logger;

    public MockElectronicInvoiceProvider(ILogger<MockElectronicInvoiceProvider> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "Mock";

    public Task<ElectronicInvoiceIssueResult> IssueInvoiceAsync(
        Invoice invoice,
        IReadOnlyList<InvoiceItem> items,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Simulating electronic invoice issuance for Invoice {InvoiceNumber}, Contract {ContractId}",
            invoice.InvoiceNumber, invoice.ContractId);

        var randomCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var lookupCode = $"EIN-{DateTime.UtcNow:yyyyMMdd}-{randomCode}";
        var electronicInvoiceId = $"EINV-MOCK-{Guid.NewGuid():N}";
        var series = $"1C{DateTime.UtcNow:yy}TBB";
        var lookupUrl = $"https://tracuu.einvoice.vn/lookup?code={lookupCode}";

        var result = new ElectronicInvoiceIssueResult(
            IsSuccess: true,
            ElectronicInvoiceId: electronicInvoiceId,
            InvoiceNumber: invoice.InvoiceNumber,
            InvoiceSeries: series,
            LookupCode: lookupCode,
            LookupUrl: lookupUrl,
            ErrorMessage: null
        );

        return Task.FromResult(result);
    }

    public Task<ElectronicInvoiceCancelResult> CancelInvoiceAsync(
        string electronicInvoiceId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Simulating electronic invoice cancellation for {ElectronicInvoiceId}. Reason: {Reason}",
            electronicInvoiceId, reason);

        return Task.FromResult(new ElectronicInvoiceCancelResult(true, null));
    }

    public Task<ElectronicInvoiceStatusResult> SyncStatusAsync(
        string electronicInvoiceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Simulating electronic invoice status sync for {ElectronicInvoiceId}", electronicInvoiceId);

        return Task.FromResult(new ElectronicInvoiceStatusResult(true, "Published", null));
    }
}
