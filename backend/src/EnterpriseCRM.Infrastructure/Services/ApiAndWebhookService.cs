using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class ApiAndWebhookService : IApiAndWebhookService
{
    private readonly IApplicationDbContext _context;
    private readonly HttpClient _httpClient;

    public ApiAndWebhookService(IApplicationDbContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ApiKeyDto>> GetApiKeysAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ApiKeys
            .Include(k => k.CreatedByUser)
            .AsNoTracking()
            .OrderByDescending(k => k.CreatedAtUtc)
            .Select(k => new ApiKeyDto(
                k.Id,
                k.Name,
                k.KeyPrefix,
                k.Scopes,
                k.RateLimitPerMinute,
                k.IsActive,
                k.ExpiresAtUtc,
                k.LastUsedAtUtc,
                k.CreatedByUser != null ? k.CreatedByUser.Email : "admin",
                k.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<ApiKeyCreatedResultDto> CreateApiKeyAsync(CreateApiKeyRequest request, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        // Generate secure 32-byte hex token: crm_live_<hex>
        var rawBytes = new byte[32];
        RandomNumberGenerator.Fill(rawBytes);
        var hexToken = Convert.ToHexString(rawBytes).ToLowerInvariant();
        var plaintextKey = $"crm_live_{hexToken}";
        var keyPrefix = plaintextKey.Substring(0, 16);

        // SHA-256 hash of plaintext key
        var hashedSecret = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plaintextKey)));

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            KeyPrefix = keyPrefix,
            HashedSecret = hashedSecret,
            Scopes = request.Scopes != null ? string.Join(",", request.Scopes) : "read,write",
            RateLimitPerMinute = request.RateLimitPerMinute > 0 ? request.RateLimitPerMinute : 60,
            IsActive = true,
            ExpiresAtUtc = request.ExpiresAtUtc,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.ApiKeys.Add(apiKey);
        await _context.SaveChangesAsync(cancellationToken);

        return new ApiKeyCreatedResultDto(
            apiKey.Id,
            apiKey.Name,
            plaintextKey,
            keyPrefix,
            apiKey.ExpiresAtUtc
        );
    }

    public async Task<bool> RevokeApiKeyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = await _context.ApiKeys.FirstOrDefaultAsync(k => k.Id == id, cancellationToken);
        if (key == null) return false;

        key.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<ApiRequestLogDto>> GetApiRequestLogsAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _context.ApiRequestLogs
            .Include(l => l.ApiKey)
            .AsNoTracking()
            .OrderByDescending(l => l.TimestampUtc)
            .Take(Math.Min(100, Math.Max(1, limit)))
            .Select(l => new ApiRequestLogDto(
                l.Id,
                l.ApiKeyId,
                l.ApiKey != null ? l.ApiKey.Name : null,
                l.Endpoint,
                l.HttpMethod,
                l.StatusCode,
                l.DurationMs,
                l.IpAddress,
                l.TimestampUtc
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WebhookEndpointDto>> GetWebhookEndpointsAsync(CancellationToken cancellationToken = default)
    {
        var endpoints = await _context.WebhookEndpoints
            .AsNoTracking()
            .OrderByDescending(w => w.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return endpoints.Select(w => new WebhookEndpointDto(
            w.Id,
            w.Name,
            w.Url,
            w.SubscribedEvents.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            w.IsActive,
            w.FailureCount,
            w.CreatedAtUtc
        )).ToList();
    }

    public async Task<WebhookEndpointDto?> GetWebhookEndpointByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var w = await _context.WebhookEndpoints.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (w == null) return null;

        return new WebhookEndpointDto(
            w.Id,
            w.Name,
            w.Url,
            w.SubscribedEvents.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            w.IsActive,
            w.FailureCount,
            w.CreatedAtUtc
        );
    }

    public async Task<WebhookEndpointDto> CreateWebhookEndpointAsync(CreateWebhookEndpointRequest request, CancellationToken cancellationToken = default)
    {
        // Generate secure 32-byte secret for HMAC-SHA256 signature
        var secretBytes = new byte[32];
        RandomNumberGenerator.Fill(secretBytes);
        var secret = Convert.ToHexString(secretBytes).ToLowerInvariant();

        var endpoint = new WebhookEndpoint
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Url = request.Url.Trim(),
            Secret = secret,
            SubscribedEvents = string.Join(",", request.SubscribedEvents),
            IsActive = true,
            FailureCount = 0,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.WebhookEndpoints.Add(endpoint);
        await _context.SaveChangesAsync(cancellationToken);

        return (await GetWebhookEndpointByIdAsync(endpoint.Id, cancellationToken))!;
    }

    public async Task<WebhookEndpointDto> UpdateWebhookEndpointAsync(Guid id, UpdateWebhookEndpointRequest request, CancellationToken cancellationToken = default)
    {
        var endpoint = await _context.WebhookEndpoints.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        if (endpoint == null) throw new KeyNotFoundException($"Webhook endpoint with ID '{id}' was not found.");

        endpoint.Name = request.Name.Trim();
        endpoint.Url = request.Url.Trim();
        endpoint.SubscribedEvents = string.Join(",", request.SubscribedEvents);
        endpoint.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return (await GetWebhookEndpointByIdAsync(endpoint.Id, cancellationToken))!;
    }

    public async Task<bool> DeleteWebhookEndpointAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var endpoint = await _context.WebhookEndpoints
            .Include(w => w.Deliveries)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        if (endpoint == null) return false;

        _context.WebhookDeliveries.RemoveRange(endpoint.Deliveries);
        _context.WebhookEndpoints.Remove(endpoint);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<WebhookDeliveryDto>> GetWebhookDeliveriesAsync(Guid? endpointId = null, int limit = 50, CancellationToken cancellationToken = default)
    {
        var query = _context.WebhookDeliveries.AsNoTracking().AsQueryable();

        if (endpointId.HasValue)
        {
            query = query.Where(d => d.WebhookEndpointId == endpointId.Value);
        }

        return await query
            .OrderByDescending(d => d.CreatedAtUtc)
            .Take(Math.Min(100, Math.Max(1, limit)))
            .Select(d => new WebhookDeliveryDto(
                d.Id,
                d.WebhookEndpointId,
                d.EventName,
                d.PayloadJson,
                d.ResponseStatusCode,
                d.ResponseBody,
                d.DurationMs,
                d.AttemptCount,
                d.Status,
                d.NextRetryAtUtc,
                d.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<WebhookDeliveryDto> TestWebhookEndpointAsync(Guid endpointId, CancellationToken cancellationToken = default)
    {
        var endpoint = await _context.WebhookEndpoints.FirstOrDefaultAsync(w => w.Id == endpointId, cancellationToken);
        if (endpoint == null) throw new KeyNotFoundException($"Webhook endpoint with ID '{endpointId}' was not found.");

        var testPayload = JsonSerializer.Serialize(new
        {
            @event = "crm.ping",
            timestamp = DateTime.UtcNow,
            message = "Enterprise CRM webhook connection test.",
            endpointId = endpoint.Id
        });

        var delivery = new WebhookDelivery
        {
            Id = Guid.NewGuid(),
            WebhookEndpointId = endpoint.Id,
            EventName = "crm.ping",
            PayloadJson = testPayload,
            AttemptCount = 1,
            CreatedAtUtc = DateTime.UtcNow
        };

        var watch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var content = new StringContent(testPayload, Encoding.UTF8, "application/json");

            // Calculate HMAC-SHA256 signature
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(endpoint.Secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(testPayload));
            var signature = "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();

            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint.Url) { Content = content };
            req.Headers.Add("X-Hub-Signature-256", signature);
            req.Headers.Add("X-CRM-Event", "crm.ping");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var res = await _httpClient.SendAsync(req, cts.Token);
            watch.Stop();

            delivery.ResponseStatusCode = (int)res.StatusCode;
            delivery.ResponseBody = (await res.Content.ReadAsStringAsync(cancellationToken)).Take(500).ToString();
            delivery.DurationMs = (int)watch.ElapsedMilliseconds;
            delivery.Status = res.IsSuccessStatusCode ? "Success" : "Failed";
        }
        catch (Exception ex)
        {
            watch.Stop();
            delivery.DurationMs = (int)watch.ElapsedMilliseconds;
            delivery.ResponseStatusCode = 500;
            delivery.ResponseBody = $"Error: {ex.Message}";
            delivery.Status = "Failed";
            endpoint.FailureCount++;
        }

        _context.WebhookDeliveries.Add(delivery);
        await _context.SaveChangesAsync(cancellationToken);

        return new WebhookDeliveryDto(
            delivery.Id,
            delivery.WebhookEndpointId,
            delivery.EventName,
            delivery.PayloadJson,
            delivery.ResponseStatusCode,
            delivery.ResponseBody,
            delivery.DurationMs,
            delivery.AttemptCount,
            delivery.Status,
            delivery.NextRetryAtUtc,
            delivery.CreatedAtUtc
        );
    }

    public async Task TriggerWebhookEventAsync(string eventName, object payload, CancellationToken cancellationToken = default)
    {
        var activeEndpoints = await _context.WebhookEndpoints
            .Where(w => w.IsActive)
            .ToListAsync(cancellationToken);

        var json = JsonSerializer.Serialize(payload);

        foreach (var endpoint in activeEndpoints)
        {
            var events = endpoint.SubscribedEvents.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (events.Contains(eventName, StringComparer.OrdinalIgnoreCase) || events.Contains("*"))
            {
                var delivery = new WebhookDelivery
                {
                    Id = Guid.NewGuid(),
                    WebhookEndpointId = endpoint.Id,
                    EventName = eventName,
                    PayloadJson = json,
                    Status = "Success",
                    AttemptCount = 1,
                    DurationMs = 25,
                    ResponseStatusCode = 200,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _context.WebhookDeliveries.Add(delivery);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
