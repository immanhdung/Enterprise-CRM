using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.ElectronicSignature;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/e-signature")]
public class ElectronicSignatureController : BaseApiController
{
    private readonly IElectronicSignatureService _signatureService;

    public ElectronicSignatureController(IElectronicSignatureService signatureService)
    {
        _signatureService = signatureService;
    }

    [HttpPost("send")]
    public async Task<ActionResult<ApiResponse<SignatureSessionResult>>> SendSignatureRequest(
        [FromBody] CreateSignatureSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _signatureService.InitiateSigningAsync(request, cancellationToken);
        return OkResponse(result, "E-Signature invitation sent successfully.");
    }

    [HttpGet("contract/{contractId:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SignatureRequestDto>>>> GetContractSignatureRequests(
        Guid contractId,
        CancellationToken cancellationToken = default)
    {
        var result = await _signatureService.GetSignatureRequestsForContractAsync(contractId, cancellationToken);
        return OkResponse(result);
    }

    [HttpPost("sync/{signatureRequestId:guid}")]
    public async Task<ActionResult<ApiResponse<SignatureRequestDto>>> SyncSignatureStatus(
        Guid signatureRequestId,
        CancellationToken cancellationToken = default)
    {
        var result = await _signatureService.SyncEnvelopeStatusAsync(signatureRequestId, cancellationToken);
        return OkResponse(result, "Signature envelope status synchronized.");
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook(
        [FromBody] WebhookEventDto eventDto,
        CancellationToken cancellationToken = default)
    {
        var success = await _signatureService.ProcessWebhookAsync(eventDto, cancellationToken);
        if (!success)
        {
            return BadRequest(new { message = "Failed to process webhook event or envelope unknown" });
        }

        return Ok(new { message = "Webhook processed successfully", timestamp = DateTime.UtcNow });
    }
}
