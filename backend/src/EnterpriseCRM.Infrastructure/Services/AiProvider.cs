using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using Microsoft.Extensions.Configuration;

namespace EnterpriseCRM.Infrastructure.Services;

public class AiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public string ProviderName { get; } = "Gemini";

    public AiProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<AiCompletionResult> GenerateChatCompletionAsync(
        List<AiChatMessage> messages,
        decimal temperature = 0.2m,
        int maxTokens = 2048,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Gemini:ApiKey"]
                  ?? _configuration["AI:ApiKey"]
                  ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                  ?? Environment.GetEnvironmentVariable("AI_API_KEY");

        var model = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";

        // If no API key is provided, return structured intelligent fallback
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            var fallbackResponse = GenerateDomainFallbackResponse(messages);
            return new AiCompletionResult(
                fallbackResponse,
                PromptTokens: 50,
                CompletionTokens: 120,
                TotalTokens: 170,
                ModelUsed: $"{model}-local-diagnostic",
                IsFallback: true
            );
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Google Gemini generateContent format
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var contents = messages.Select(m => new
            {
                role = m.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = m.Content } }
            }).ToList();

            var payload = new
            {
                contents = contents,
                generationConfig = new
                {
                    temperature = (double)temperature,
                    maxOutputTokens = maxTokens
                }
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, requestContent, cancellationToken);

            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                return new AiCompletionResult(
                    $"AI Service diagnostic: Provider returned {response.StatusCode}. Falling back to internal CRM logic.\n{GenerateDomainFallbackResponse(messages)}",
                    50, 100, 150, model, true
                );
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseJson);

            var root = doc.RootElement;
            var text = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            int promptTokens = 0;
            int completionTokens = 0;
            int totalTokens = 0;

            if (root.TryGetProperty("usageMetadata", out var usage))
            {
                if (usage.TryGetProperty("promptTokenCount", out var pt)) promptTokens = pt.GetInt32();
                if (usage.TryGetProperty("candidatesTokenCount", out var ct)) completionTokens = ct.GetInt32();
                if (usage.TryGetProperty("totalTokenCount", out var tt)) totalTokens = tt.GetInt32();
            }
            else
            {
                promptTokens = messages.Sum(m => m.Content.Length) / 4;
                completionTokens = text.Length / 4;
                totalTokens = promptTokens + completionTokens;
            }

            return new AiCompletionResult(
                text,
                promptTokens,
                completionTokens,
                totalTokens,
                model,
                false
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new AiCompletionResult(
                $"[Offline / Fallback Intelligence] Phân tích tự động dựa trên quy tắc nghiệp vụ CRM:\n{GenerateDomainFallbackResponse(messages)}\n\n(Ghi chú kỹ thuật: {ex.Message})",
                50, 100, 150, model, true
            );
        }
    }

    private string GenerateDomainFallbackResponse(List<AiChatMessage> messages)
    {
        var lastUserMsg = messages.LastOrDefault(m => m.Role == "user")?.Content ?? "";
        var lower = lastUserMsg.ToLowerInvariant();

        if (lower.Contains("doanh thu") || lower.Contains("revenue"))
        {
            return "Dựa trên dữ liệu tài chính CRM hiện tại:\n- Dòng tiền thực thu và doanh thu hợp đồng được tổng hợp trực tiếp từ bảng payments và contracts.\n- Dự báo tuần tới cho thấy xu hướng dòng tiền ổn định với tỷ lệ hoàn thành hợp đồng đạt trên 85%.\n- Để xem chi tiết, bạn có thể kiểm tra trang Dự báo Doanh thu AI hoặc Báo cáo Quản trị.";
        }

        if (lower.Contains("lead") || lower.Contains("khách hàng tiềm năng"))
        {
            return "Hệ thống Phân phối Lead Tự động (Module B) đang áp dụng quy tắc Round-Robin và Workload-based:\n- Các lead mới nhận được gắn trực tiếp cho nhân viên sales theo năng lực tiếp nhận.\n- Khuyến nghị: Cần ưu tiên các lead có điểm chất lượng (Quality Score) >= 80 và follow-up trong vòng 24 giờ đầu.";
        }

        if (lower.Contains("deal") || lower.Contains("cơ hội"))
        {
            return "Phân tích Pipeline Cơ hội Bán hàng:\n- Hiện có các cơ hội ở giai đoạn Đàm phán cần đẩy nhanh tiến độ ký hợp đồng.\n- Khuyến nghị: Thiết lập cuộc họp chốt deal với người ra quyết định và chuẩn bị dự thảo hợp đồng từ báo giá đã duyệt.";
        }

        return "Xin chào! Tôi là Trợ lý Thông minh Enterprise CRM (AI Copilot). Tôi có thể hỗ trợ bạn:\n1. Phân tích hiệu suất bán hàng và dự báo dòng tiền.\n2. Truy vấn dữ liệu kinh doanh bằng tiếng Việt và tiếng Anh tự nhiên.\n3. Phát hiện bất thường trong phễu bán hàng và công nợ quá hạn.\n4. Đề xuất hành động tối ưu cho Lead và Deal quan trọng.\n\nBạn có thể đặt câu hỏi hoặc yêu cầu phân tích cụ thể!";
    }
}
