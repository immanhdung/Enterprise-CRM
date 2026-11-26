using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/admin/integrations")]
[Authorize]
public class IntegrationsController : BaseApiController
{
    private readonly IApiAndWebhookService _integrationService;
    private readonly ICurrentUserService _currentUserService;

    public IntegrationsController(IApiAndWebhookService integrationService, ICurrentUserService currentUserService)
    {
        _integrationService = integrationService;
        _currentUserService = currentUserService;
    }

    // ==========================================
    // API Keys
    // ==========================================
    [HttpGet("api-keys")]
    [HasPermission(Permissions.ApiKeysManage)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ApiKeyDto>>>> GetApiKeys(CancellationToken ct)
    {
        var keys = await _integrationService.GetApiKeysAsync(ct);
        return OkResponse(keys);
    }

    [HttpPost("api-keys")]
    [HasPermission(Permissions.ApiKeysManage)]
    public async Task<ActionResult<ApiResponse<ApiKeyCreatedResultDto>>> CreateApiKey([FromBody] CreateApiKeyRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var result = await _integrationService.CreateApiKeyAsync(request, userId, ct);
        return OkResponse(result, "API Key created successfully. Store the plaintext key safely; it cannot be retrieved again.");
    }

    [HttpDelete("api-keys/{id:guid}")]
    [HasPermission(Permissions.ApiKeysManage)]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeApiKey(Guid id, CancellationToken ct)
    {
        var result = await _integrationService.RevokeApiKeyAsync(id, ct);
        if (!result) return FailResponse<bool>("API Key not found.", StatusCodes.Status404NotFound);
        return OkResponse(true, "API Key revoked successfully.");
    }

    [HttpGet("api-keys/logs")]
    [HasPermission(Permissions.ApiKeysManage)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ApiRequestLogDto>>>> GetApiLogs([FromQuery] int limit = 50, CancellationToken ct = default)
    {
        var logs = await _integrationService.GetApiRequestLogsAsync(limit, ct);
        return OkResponse(logs);
    }

    // ==========================================
    // Webhooks
    // ==========================================
    [HttpGet("webhooks")]
    [HasPermission(Permissions.WebhooksManage)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WebhookEndpointDto>>>> GetWebhooks(CancellationToken ct)
    {
        var webhooks = await _integrationService.GetWebhookEndpointsAsync(ct);
        return OkResponse(webhooks);
    }

    [HttpGet("webhooks/{id:guid}")]
    [HasPermission(Permissions.WebhooksManage)]
    public async Task<ActionResult<ApiResponse<WebhookEndpointDto>>> GetWebhookById(Guid id, CancellationToken ct)
    {
        var webhook = await _integrationService.GetWebhookEndpointByIdAsync(id, ct);
        if (webhook == null) return FailResponse<WebhookEndpointDto>("Webhook endpoint not found.", StatusCodes.Status404NotFound);
        return OkResponse(webhook);
    }

    [HttpPost("webhooks")]
    [HasPermission(Permissions.WebhooksManage)]
    public async Task<ActionResult<ApiResponse<WebhookEndpointDto>>> CreateWebhook([FromBody] CreateWebhookEndpointRequest request, CancellationToken ct)
    {
        var webhook = await _integrationService.CreateWebhookEndpointAsync(request, ct);
        return OkResponse(webhook, "Webhook endpoint registered.");
    }

    [HttpPut("webhooks/{id:guid}")]
    [HasPermission(Permissions.WebhooksManage)]
    public async Task<ActionResult<ApiResponse<WebhookEndpointDto>>> UpdateWebhook(Guid id, [FromBody] UpdateWebhookEndpointRequest request, CancellationToken ct)
    {
        try
        {
            var webhook = await _integrationService.UpdateWebhookEndpointAsync(id, request, ct);
            return OkResponse(webhook, "Webhook endpoint updated.");
        }
        catch (KeyNotFoundException ex)
        {
            return FailResponse<WebhookEndpointDto>(ex.Message, StatusCodes.Status404NotFound);
        }
    }

    [HttpDelete("webhooks/{id:guid}")]
    [HasPermission(Permissions.WebhooksManage)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteWebhook(Guid id, CancellationToken ct)
    {
        var result = await _integrationService.DeleteWebhookEndpointAsync(id, ct);
        if (!result) return FailResponse<bool>("Webhook endpoint not found.", StatusCodes.Status404NotFound);
        return OkResponse(true, "Webhook endpoint deleted.");
    }

    [HttpGet("webhooks/deliveries")]
    [HasPermission(Permissions.WebhooksManage)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WebhookDeliveryDto>>>> GetDeliveries([FromQuery] Guid? endpointId, [FromQuery] int limit = 50, CancellationToken ct = default)
    {
        var deliveries = await _integrationService.GetWebhookDeliveriesAsync(endpointId, limit, ct);
        return OkResponse(deliveries);
    }

    [HttpPost("webhooks/{id:guid}/test")]
    [HasPermission(Permissions.WebhooksManage)]
    public async Task<ActionResult<ApiResponse<WebhookDeliveryDto>>> TestWebhook(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _integrationService.TestWebhookEndpointAsync(id, ct);
            return OkResponse(result, "Webhook test executed.");
        }
        catch (KeyNotFoundException ex)
        {
            return FailResponse<WebhookDeliveryDto>(ex.Message, StatusCodes.Status404NotFound);
        }
    }
}
