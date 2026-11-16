using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.Contracts;
using EnterpriseCRM.Application.Features.ElectronicSignature;
using EnterpriseCRM.Application.Features.Quotations;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class MockElectronicSignatureProvider : IElectronicSignatureProvider
{
    private readonly ILogger<MockElectronicSignatureProvider> _logger;

    public string ProviderName => "MockEsignProvider";

    public MockElectronicSignatureProvider(ILogger<MockElectronicSignatureProvider> logger)
    {
        _logger = logger;
    }

    public Task<SignatureSessionResult> CreateEnvelopeAsync(
        string contractNumber,
        string documentTitle,
        byte[] pdfBytes,
        string signerName,
        string signerEmail,
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        var envelopeId = $"ENV-{Guid.NewGuid():N}"[..18].ToUpper();
        var signingUrl = $"/contracts/esign/portal?envelopeId={envelopeId}&signer={Uri.EscapeDataString(signerEmail)}";

        _logger.LogInformation("Simulated e-sign envelope created: {EnvelopeId} for {SignerName} ({SignerEmail})", envelopeId, signerName, signerEmail);

        return Task.FromResult(new SignatureSessionResult(
            Guid.Empty,
            envelopeId,
            ProviderName,
            signingUrl,
            "Sent"
        ));
    }

    public Task<string> GetEnvelopeStatusAsync(string externalEnvelopeId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult("Delivered");
    }
}

public class ElectronicSignatureService : IElectronicSignatureService
{
    private readonly IApplicationDbContext _context;
    private readonly IElectronicSignatureProvider _provider;
    private readonly IPdfGenerationService _pdfService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ElectronicSignatureService> _logger;

    public ElectronicSignatureService(
        IApplicationDbContext context,
        IElectronicSignatureProvider provider,
        IPdfGenerationService pdfService,
        ICurrentUserService currentUserService,
        ILogger<ElectronicSignatureService> logger)
    {
        _context = context;
        _provider = provider;
        _pdfService = pdfService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<SignatureSessionResult> InitiateSigningAsync(CreateSignatureSessionRequest request, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .Include(c => c.Account)
            .Include(c => c.Contact)
            .Include(c => c.Items)
            .Include(c => c.Histories)
            .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

        if (contract == null) throw new KeyNotFoundException($"Contract '{request.ContractId}' not found.");

        var pdfBytes = await _pdfService.GenerateContractPdfAsync(contract, cancellationToken);

        var result = await _provider.CreateEnvelopeAsync(
            contract.ContractNumber,
            contract.Title,
            pdfBytes,
            request.SignerName,
            request.SignerEmail,
            request.ReturnUrl,
            cancellationToken);

        var sigRequest = new SignatureRequest
        {
            ContractId = contract.Id,
            Provider = result.Provider,
            ExternalEnvelopeId = result.ExternalEnvelopeId,
            SignerName = request.SignerName,
            SignerEmail = request.SignerEmail,
            Status = "Sent",
            SentAt = DateTime.UtcNow,
            SigningUrl = result.SigningUrl
        };

        contract.EsignStatus = "PendingSignature";
        var history = new ContractHistory
        {
            ContractId = contract.Id,
            Action = "EsignSent",
            OldStatus = contract.Status.ToString(),
            NewStatus = contract.Status.ToString(),
            ActorUserId = _currentUserService.UserId ?? Guid.Empty,
            ActorName = _currentUserService.Email ?? "System",
            Notes = $"E-Signature request dispatched via {result.Provider} to {request.SignerName} ({request.SignerEmail}). Envelope: {result.ExternalEnvelopeId}",
            Timestamp = DateTime.UtcNow
        };
        _context.ContractHistories.Add(history);

        _context.SignatureRequests.Add(sigRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return new SignatureSessionResult(
            sigRequest.Id,
            sigRequest.ExternalEnvelopeId,
            sigRequest.Provider,
            sigRequest.SigningUrl ?? string.Empty,
            sigRequest.Status
        );
    }

    public async Task<IReadOnlyList<SignatureRequestDto>> GetSignatureRequestsForContractAsync(Guid contractId, CancellationToken cancellationToken = default)
    {
        return await _context.SignatureRequests
            .AsNoTracking()
            .Include(s => s.Contract)
            .Where(s => s.ContractId == contractId)
            .OrderByDescending(s => s.SentAt)
            .Select(s => new SignatureRequestDto(
                s.Id,
                s.ContractId,
                s.Contract.ContractNumber,
                s.Provider,
                s.ExternalEnvelopeId,
                s.SignerName,
                s.SignerEmail,
                s.Status,
                s.SentAt,
                s.SignedAt,
                s.SigningUrl
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ProcessWebhookAsync(WebhookEventDto eventDto, CancellationToken cancellationToken = default)
    {
        var sigRequest = await _context.SignatureRequests
            .Include(s => s.Contract)
            .FirstOrDefaultAsync(s => s.ExternalEnvelopeId == eventDto.ExternalEnvelopeId, cancellationToken);

        if (sigRequest == null)
        {
            _logger.LogWarning("Signature webhook received for unknown envelope: {EnvelopeId}", eventDto.ExternalEnvelopeId);
            return false;
        }

        sigRequest.WebhookLog = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Status: {eventDto.Status}, Event: {eventDto.EventType}\nPayload: {eventDto.PayloadJson}";
        sigRequest.Status = eventDto.Status;

        var contract = sigRequest.Contract;

        if (eventDto.Status.Equals("Signed", StringComparison.OrdinalIgnoreCase) ||
            eventDto.EventType.Equals("envelope-completed", StringComparison.OrdinalIgnoreCase))
        {
            sigRequest.SignedAt = eventDto.Timestamp;
            contract.EsignStatus = "Signed";
            contract.Status = ContractStatus.Signed;
            contract.SignedAt = eventDto.Timestamp;
            contract.SignedByClientName = sigRequest.SignerName;
            contract.SignedByClientEmail = sigRequest.SignerEmail;
            contract.SignerIpAddress = eventDto.SignatureIpAddress ?? "127.0.0.1";

            var history = new ContractHistory
            {
                ContractId = contract.Id,
                Action = "EsignCompleted",
                OldStatus = ContractStatus.Draft.ToString(),
                NewStatus = ContractStatus.Signed.ToString(),
                ActorUserId = Guid.Empty,
                ActorName = "Webhook/E-Sign Service",
                Notes = $"Document digitally signed by {sigRequest.SignerName} ({sigRequest.SignerEmail}). Envelope: {eventDto.ExternalEnvelopeId}",
                Timestamp = eventDto.Timestamp
            };
            _context.ContractHistories.Add(history);
        }
        else if (eventDto.Status.Equals("Declined", StringComparison.OrdinalIgnoreCase))
        {
            contract.EsignStatus = "Declined";
            var history = new ContractHistory
            {
                ContractId = contract.Id,
                Action = "EsignDeclined",
                OldStatus = contract.Status.ToString(),
                NewStatus = contract.Status.ToString(),
                ActorUserId = Guid.Empty,
                ActorName = "Webhook/E-Sign Service",
                Notes = $"E-Signature declined by {sigRequest.SignerName}.",
                Timestamp = eventDto.Timestamp
            };
            _context.ContractHistories.Add(history);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<SignatureRequestDto> SyncEnvelopeStatusAsync(Guid signatureRequestId, CancellationToken cancellationToken = default)
    {
        var sig = await _context.SignatureRequests
            .Include(s => s.Contract)
            .FirstOrDefaultAsync(s => s.Id == signatureRequestId, cancellationToken);

        if (sig == null) throw new KeyNotFoundException($"SignatureRequest '{signatureRequestId}' not found.");

        var currentStatus = await _provider.GetEnvelopeStatusAsync(sig.ExternalEnvelopeId, cancellationToken);
        sig.Status = currentStatus;
        await _context.SaveChangesAsync(cancellationToken);

        return new SignatureRequestDto(
            sig.Id,
            sig.ContractId,
            sig.Contract.ContractNumber,
            sig.Provider,
            sig.ExternalEnvelopeId,
            sig.SignerName,
            sig.SignerEmail,
            sig.Status,
            sig.SentAt,
            sig.SignedAt,
            sig.SigningUrl
        );
    }
}
