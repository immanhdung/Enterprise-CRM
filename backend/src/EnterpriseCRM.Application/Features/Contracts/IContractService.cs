namespace EnterpriseCRM.Application.Features.Contracts;

public interface IContractService
{
    Task<PagedContractResult> GetContractsAsync(ContractFilterParams filter, CancellationToken cancellationToken = default);
    Task<ContractDto?> GetContractByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContractDto> CreateContractAsync(CreateContractRequest request, CancellationToken cancellationToken = default);
    Task<ContractDto> GenerateContractFromQuotationAsync(GenerateContractFromQuotationRequest request, CancellationToken cancellationToken = default);
    Task<ContractDto> UpdateContractAsync(Guid id, UpdateContractRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteContractAsync(Guid id, CancellationToken cancellationToken = default);

    // Document & Render
    Task<string> RenderContractContentAsync(Guid contractId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateContractPdfAsync(Guid contractId, CancellationToken cancellationToken = default);

    // Appendices
    Task<IReadOnlyList<ContractAppendixDto>> GetAppendicesAsync(Guid contractId, CancellationToken cancellationToken = default);
    Task<ContractAppendixDto> CreateAppendixAsync(Guid contractId, CreateContractAppendixRequest request, CancellationToken cancellationToken = default);

    // Renewals & Extensions
    Task<ContractDto> RenewContractAsync(Guid contractId, RenewContractRequest request, CancellationToken cancellationToken = default);
}
