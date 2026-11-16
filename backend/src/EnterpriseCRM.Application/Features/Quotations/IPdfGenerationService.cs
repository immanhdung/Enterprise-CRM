using EnterpriseCRM.Domain.Entities;

namespace EnterpriseCRM.Application.Features.Quotations;

public interface IPdfGenerationService
{
    Task<byte[]> GenerateQuotationPdfAsync(Quotation quotation, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateContractPdfAsync(Contract contract, CancellationToken cancellationToken = default);
}
