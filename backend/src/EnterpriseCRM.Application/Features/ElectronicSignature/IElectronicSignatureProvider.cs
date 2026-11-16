namespace EnterpriseCRM.Application.Features.ElectronicSignature;

public interface IElectronicSignatureProvider
{
    string ProviderName { get; }
    Task<SignatureSessionResult> CreateEnvelopeAsync(
        string contractNumber,
        string documentTitle,
        byte[] pdfBytes,
        string signerName,
        string signerEmail,
        string? returnUrl = null,
        CancellationToken cancellationToken = default);

    Task<string> GetEnvelopeStatusAsync(string externalEnvelopeId, CancellationToken cancellationToken = default);
}
