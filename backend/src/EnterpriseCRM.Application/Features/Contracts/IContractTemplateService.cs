namespace EnterpriseCRM.Application.Features.Contracts;

public interface IContractTemplateService
{
    Task<IReadOnlyList<ContractTemplateDto>> GetTemplatesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<ContractTemplateDto?> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContractTemplateDto> CreateTemplateAsync(CreateContractTemplateRequest request, CancellationToken cancellationToken = default);
    Task<ContractTemplateDto> UpdateTemplateAsync(Guid id, UpdateContractTemplateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteTemplateAsync(Guid id, CancellationToken cancellationToken = default);
}
