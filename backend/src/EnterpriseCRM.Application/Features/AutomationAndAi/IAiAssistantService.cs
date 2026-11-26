namespace EnterpriseCRM.Application.Features.AutomationAndAi;

public record AiChatMessage(string Role, string Content);

public record AiCompletionResult(
    string Content,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    string ModelUsed,
    bool IsFallback
);

public interface IAiProvider
{
    string ProviderName { get; }
    Task<AiCompletionResult> GenerateChatCompletionAsync(
        List<AiChatMessage> messages,
        decimal temperature = 0.2m,
        int maxTokens = 2048,
        CancellationToken cancellationToken = default
    );
}

public interface IAiAssistantService
{
    Task<List<AiConversationDto>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AiConversationDto?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<List<AiConversationMessageDto>> GetConversationMessagesAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<AiChatResponse> SendChatMessageAsync(Guid userId, AiChatRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<AiProviderConfigDto> GetProviderConfigAsync(CancellationToken cancellationToken = default);
    Task<AiProviderConfigDto> UpdateProviderConfigAsync(UpdateAiProviderConfigRequest request, CancellationToken cancellationToken = default);
    Task<AiUsageSummaryDto> GetUsageSummaryAsync(CancellationToken cancellationToken = default);
}
