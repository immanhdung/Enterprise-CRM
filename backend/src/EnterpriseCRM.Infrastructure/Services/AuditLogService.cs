using System.Text;
using System.Text.Json;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;

    public AuditLogService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetLogsAsync(AuditLogFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Module))
        {
            query = query.Where(a => a.Module == filter.Module);
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            query = query.Where(a => a.Action == filter.Action);
        }

        if (filter.UserId.HasValue)
        {
            query = query.Where(a => a.UserId == filter.UserId.Value);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= filter.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(a =>
                a.EntityName.ToLower().Contains(s) ||
                a.UserEmail.ToLower().Contains(s) ||
                (a.EntityId != null && a.EntityId.ToLower().Contains(s)));
        }

        int skip = (Math.Max(1, filter.PageNumber) - 1) * Math.Max(1, filter.PageSize);
        int take = Math.Min(100, Math.Max(1, filter.PageSize));

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .Select(a => new AuditLogDto(
                a.Id,
                a.UserId,
                a.UserEmail,
                a.Action,
                a.Module,
                a.EntityName,
                a.EntityId,
                a.IpAddress,
                a.UserAgent,
                a.IsSuccess,
                a.Timestamp
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<AuditLogDetailDto?> GetLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var a = await _context.AuditLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (a == null) return null;

        return new AuditLogDetailDto(
            a.Id,
            a.UserId,
            a.UserEmail,
            a.Action,
            a.Module,
            a.EntityName,
            a.EntityId,
            a.IpAddress,
            a.UserAgent,
            a.CorrelationId,
            a.IsSuccess,
            a.OldValuesJson,
            a.NewValuesJson,
            a.Timestamp
        );
    }

    public async Task<byte[]> ExportLogsAsync(AuditLogFilterRequest filter, string format = "Excel", CancellationToken cancellationToken = default)
    {
        var logs = await GetLogsAsync(filter with { PageNumber = 1, PageSize = 1000 }, cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Id,TimestampUtc,UserEmail,Action,Module,EntityName,EntityId,IpAddress,IsSuccess");

        foreach (var l in logs)
        {
            sb.AppendLine($"\"{l.Id}\",\"{l.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{l.UserEmail}\",\"{l.Action}\",\"{l.Module}\",\"{l.EntityName}\",\"{l.EntityId}\",\"{l.IpAddress}\",\"{l.IsSuccess}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task LogAsync(
        string action,
        string module,
        string entityName,
        string? entityId,
        object? oldValues,
        object? newValues,
        Guid? userId,
        string? userEmail,
        string? ipAddress = null,
        string? userAgent = null,
        bool isSuccess = true,
        CancellationToken cancellationToken = default)
    {
        string? oldJson = oldValues != null ? MaskSensitiveFields(JsonSerializer.Serialize(oldValues)) : null;
        string? newJson = newValues != null ? MaskSensitiveFields(JsonSerializer.Serialize(newValues)) : null;

        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserEmail = userEmail ?? "system@enterprise.internal",
            Action = action,
            Module = module,
            EntityName = entityName,
            EntityId = entityId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsSuccess = isSuccess,
            OldValuesJson = oldJson,
            NewValuesJson = newJson,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string MaskSensitiveFields(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;
        // Mask passwords, tokens, secrets
        var sensitiveKeys = new[] { "password", "token", "secret", "apikey", "salt" };
        var result = json;
        foreach (var key in sensitiveKeys)
        {
            result = System.Text.RegularExpressions.Regex.Replace(
                result,
                $"(\"{key}\"\\s*:\\s*\")[^\"]+(\")",
                "$1********$2",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        return result;
    }
}
