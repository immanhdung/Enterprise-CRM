using System.Diagnostics;
using System.Text.Json;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class AiAssistantService : IAiAssistantService
{
    private readonly IApplicationDbContext _context;
    private readonly IAiProvider _aiProvider;

    public AiAssistantService(IApplicationDbContext context, IAiProvider aiProvider)
    {
        _context = context;
        _aiProvider = aiProvider;
    }

    public async Task<List<AiConversationDto>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var list = await _context.AiConversations
            .Where(c => c.UserId == userId)
            .Include(c => c.Messages)
            .OrderByDescending(c => c.CreatedAt)
            .Take(30)
            .ToListAsync(cancellationToken);

        return list.Select(c => new AiConversationDto(
            c.Id,
            c.UserId,
            c.Title,
            c.ContextSnapshotJson,
            c.CreatedAt,
            c.Messages.Count
        )).ToList();
    }

    public async Task<AiConversationDto?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var c = await _context.AiConversations
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken);

        if (c == null) return null;

        return new AiConversationDto(
            c.Id,
            c.UserId,
            c.Title,
            c.ContextSnapshotJson,
            c.CreatedAt,
            c.Messages.Count
        );
    }

    public async Task<List<AiConversationMessageDto>> GetConversationMessagesAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var messages = await _context.AiConversationMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return messages.Select(m => new AiConversationMessageDto(
            m.Id,
            m.ConversationId,
            m.Role,
            m.Content,
            m.ContextJson,
            m.TokensUsed,
            m.ModelUsed,
            m.CreatedAtUtc
        )).ToList();
    }

    public async Task<AiChatResponse> SendChatMessageAsync(Guid userId, AiChatRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // 1. Get or create conversation
        AiConversation? conversation;
        if (request.ConversationId.HasValue && request.ConversationId.Value != Guid.Empty)
        {
            conversation = await _context.AiConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId.Value, cancellationToken);
        }
        else
        {
            conversation = null;
        }

        if (conversation == null)
        {
            var title = request.Message.Length > 40 ? request.Message.Substring(0, 40) + "..." : request.Message;
            conversation = new AiConversation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                ContextSnapshotJson = request.CurrentPageContext
            };
            _context.AiConversations.Add(conversation);
        }

        // 2. Add user message
        var userMsg = new AiConversationMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = "user",
            Content = request.Message.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.AiConversationMessages.Add(userMsg);

        // 3. Prepare message history for completion
        var systemInstruction = "Bạn là Enterprise CRM AI Assistant chuyên gia cho ngành dịch vụ. Hãy phân tích chuyên nghiệp, trung thực với dữ liệu thực tế, khuyến nghị các hành động kinh doanh thực tiễn. Không bịa đặt số liệu tài chính.";
        
        var chatMessages = new List<AiChatMessage>
        {
            new AiChatMessage("system", systemInstruction)
        };

        if (conversation.Messages != null && conversation.Messages.Any())
        {
            foreach (var m in conversation.Messages.TakeLast(6))
            {
                chatMessages.Add(new AiChatMessage(m.Role, m.Content));
            }
        }
        chatMessages.Add(new AiChatMessage("user", request.Message));

        // 4. Invoke provider
        var result = await _aiProvider.GenerateChatCompletionAsync(chatMessages, cancellationToken: cancellationToken);
        stopwatch.Stop();

        // 5. Store assistant message
        var assistantMsg = new AiConversationMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = "assistant",
            Content = result.Content,
            TokensUsed = result.TotalTokens,
            ModelUsed = result.ModelUsed,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.AiConversationMessages.Add(assistantMsg);

        // 6. Record usage log
        _context.AiUsageLogs.Add(new AiUsageLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OperationType = "Chat",
            ProviderType = _aiProvider.ProviderName,
            ModelName = result.ModelUsed,
            PromptTokens = result.PromptTokens,
            CompletionTokens = result.CompletionTokens,
            TotalTokens = result.TotalTokens,
            DurationMs = stopwatch.ElapsedMilliseconds,
            Success = true,
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        var messageDto = new AiConversationMessageDto(
            assistantMsg.Id,
            assistantMsg.ConversationId,
            assistantMsg.Role,
            assistantMsg.Content,
            assistantMsg.ContextJson,
            assistantMsg.TokensUsed,
            assistantMsg.ModelUsed,
            assistantMsg.CreatedAtUtc
        );

        return new AiChatResponse(
            conversation.Id,
            messageDto,
            result.IsFallback
        );
    }

    public async Task<bool> DeleteConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _context.AiConversations.FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
        if (conversation == null) return false;

        conversation.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AiProviderConfigDto> GetProviderConfigAsync(CancellationToken cancellationToken = default)
    {
        var config = await _context.AiProviderConfigurations.FirstOrDefaultAsync(cancellationToken);
        if (config == null)
        {
            return new AiProviderConfigDto(
                Guid.NewGuid(),
                "Gemini",
                null,
                "gemini-1.5-flash",
                0.2m,
                2048,
                true,
                false
            );
        }

        return new AiProviderConfigDto(
            config.Id,
            config.ProviderType,
            config.EndpointUrl,
            config.ModelName,
            config.Temperature,
            config.MaxTokens,
            config.IsActive,
            !string.IsNullOrWhiteSpace(config.ApiKeyEncrypted)
        );
    }

    public async Task<AiProviderConfigDto> UpdateProviderConfigAsync(UpdateAiProviderConfigRequest request, CancellationToken cancellationToken = default)
    {
        var config = await _context.AiProviderConfigurations.FirstOrDefaultAsync(cancellationToken);
        if (config == null)
        {
            config = new AiProviderConfiguration
            {
                Id = Guid.NewGuid(),
                ProviderType = request.ProviderType,
                EndpointUrl = request.EndpointUrl,
                ModelName = request.ModelName,
                ApiKeyEncrypted = request.ApiKey,
                Temperature = request.Temperature,
                MaxTokens = request.MaxTokens,
                IsActive = request.IsActive
            };
            _context.AiProviderConfigurations.Add(config);
        }
        else
        {
            config.ProviderType = request.ProviderType;
            config.EndpointUrl = request.EndpointUrl;
            config.ModelName = request.ModelName;
            if (!string.IsNullOrWhiteSpace(request.ApiKey))
                config.ApiKeyEncrypted = request.ApiKey;
            config.Temperature = request.Temperature;
            config.MaxTokens = request.MaxTokens;
            config.IsActive = request.IsActive;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new AiProviderConfigDto(
            config.Id,
            config.ProviderType,
            config.EndpointUrl,
            config.ModelName,
            config.Temperature,
            config.MaxTokens,
            config.IsActive,
            !string.IsNullOrWhiteSpace(config.ApiKeyEncrypted)
        );
    }

    public async Task<AiUsageSummaryDto> GetUsageSummaryAsync(CancellationToken cancellationToken = default)
    {
        var logs = await _context.AiUsageLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(100)
            .ToListAsync(cancellationToken);

        var totalRequests = logs.Count;
        var totalTokens = logs.Sum(l => l.TotalTokens);
        // Cost estimation: ~$0.000001 per token roughly for Gemini 1.5 Flash
        var estimatedCost = Math.Round(totalTokens * 0.0000005m, 4);

        var opCounts = logs.GroupBy(l => l.OperationType)
            .ToDictionary(g => g.Key, g => g.Count());

        var recentDtos = logs.Take(20).Select(l => new AiUsageLogDto(
            l.Id,
            l.UserId,
            l.OperationType,
            l.ProviderType,
            l.ModelName,
            l.PromptTokens,
            l.CompletionTokens,
            l.TotalTokens,
            l.DurationMs,
            l.Success,
            l.ErrorMessage,
            l.Timestamp
        )).ToList();

        return new AiUsageSummaryDto(
            totalRequests,
            totalTokens,
            estimatedCost,
            opCounts,
            recentDtos
        );
    }
}
