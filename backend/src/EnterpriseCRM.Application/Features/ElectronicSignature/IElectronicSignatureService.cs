namespace EnterpriseCRM.Application.Features.ElectronicSignature;

public interface IElectronicSignatureService
{
    Task<SignatureSessionResult> InitiateSigningAsync(CreateSignatureSessionRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SignatureRequestDto>> GetSignatureRequestsForContractAsync(Guid contractId, CancellationToken cancellationToken = default);
    Task<bool> ProcessWebhookAsync(WebhookEventDto eventDto, CancellationToken cancellationToken = default);
    Task<SignatureRequestDto> SyncEnvelopeStatusAsync(Guid signatureRequestId, CancellationToken cancellationToken = default);
}
