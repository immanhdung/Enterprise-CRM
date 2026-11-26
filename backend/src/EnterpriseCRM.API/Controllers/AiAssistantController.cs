using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/ai-assistant")]
public class AiAssistantController : BaseApiController
{
    private readonly IAiAssistantService _aiAssistantService;
    private readonly IBusinessQueryService _businessQueryService;
    private readonly ICurrentUserService _currentUserService;

    public AiAssistantController(
        IAiAssistantService aiAssistantService,
        IBusinessQueryService businessQueryService,
        ICurrentUserService currentUserService)
    {
        _aiAssistantService = aiAssistantService;
        _businessQueryService = businessQueryService;
        _currentUserService = currentUserService;
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<ApiResponse<List<AiConversationDto>>>> GetConversations(CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var result = await _aiAssistantService.GetUserConversationsAsync(userId, ct);
        return OkResponse(result);
    }

    [HttpGet("conversations/{id}")]
    public async Task<ActionResult<ApiResponse<AiConversationDto>>> GetConversationById(Guid id, CancellationToken ct = default)
    {
        var result = await _aiAssistantService.GetConversationByIdAsync(id, ct);
        if (result == null) return FailResponse<AiConversationDto>("Conversation not found.", StatusCodes.Status404NotFound);
        return OkResponse(result);
    }

    [HttpGet("conversations/{id}/messages")]
    public async Task<ActionResult<ApiResponse<List<AiConversationMessageDto>>>> GetConversationMessages(Guid id, CancellationToken ct = default)
    {
        var result = await _aiAssistantService.GetConversationMessagesAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPost("chat")]
    public async Task<ActionResult<ApiResponse<AiChatResponse>>> SendChatMessage(
        [FromBody] AiChatRequest request,
        CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var result = await _aiAssistantService.SendChatMessageAsync(userId, request, ct);
        return OkResponse(result);
    }

    [HttpDelete("conversations/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteConversation(Guid id, CancellationToken ct = default)
    {
        var success = await _aiAssistantService.DeleteConversationAsync(id, ct);
        return OkResponse(success, "Conversation deleted.");
    }

    [HttpPost("query")]
    public async Task<ActionResult<ApiResponse<BusinessQueryResultDto>>> ExecuteNaturalLanguageQuery(
        [FromBody] BusinessQueryRequest request,
        CancellationToken ct = default)
    {
        var result = await _businessQueryService.ExecuteNaturalLanguageQueryAsync(request, ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod}")]
    [HttpGet("config")]
    public async Task<ActionResult<ApiResponse<AiProviderConfigDto>>> GetConfig(CancellationToken ct = default)
    {
        var result = await _aiAssistantService.GetProviderConfigAsync(ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod}")]
    [HttpPut("config")]
    public async Task<ActionResult<ApiResponse<AiProviderConfigDto>>> UpdateConfig(
        [FromBody] UpdateAiProviderConfigRequest request,
        CancellationToken ct = default)
    {
        var result = await _aiAssistantService.UpdateProviderConfigAsync(request, ct);
        return OkResponse(result, "AI provider configuration updated.");
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod}")]
    [HttpGet("usage")]
    public async Task<ActionResult<ApiResponse<AiUsageSummaryDto>>> GetUsageSummary(CancellationToken ct = default)
    {
        var result = await _aiAssistantService.GetUsageSummaryAsync(ct);
        return OkResponse(result);
    }
}
