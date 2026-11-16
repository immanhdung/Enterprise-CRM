namespace EnterpriseCRM.Application.Features.ElectronicSignature;

public record SignatureRequestDto(
    Guid Id,
    Guid ContractId,
    string ContractNumber,
    string Provider,
    string ExternalEnvelopeId,
    string SignerName,
    string SignerEmail,
    string Status,
    DateTime SentAt,
    DateTime? SignedAt,
    string? SigningUrl
);

public record CreateSignatureSessionRequest(
    Guid ContractId,
    string SignerName,
    string SignerEmail,
    string? ReturnUrl = null
);

public record SignatureSessionResult(
    Guid SignatureRequestId,
    string ExternalEnvelopeId,
    string Provider,
    string SigningUrl,
    string Status
);

public record WebhookEventDto(
    string Provider,
    string EventType,
    string ExternalEnvelopeId,
    string Status,
    string? SignerEmail,
    DateTime Timestamp,
    string? SignatureIpAddress,
    string? PayloadJson
);
