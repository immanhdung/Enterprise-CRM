using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class SystemConfigurationService : ISystemConfigurationService
{
    private readonly IApplicationDbContext _context;

    public SystemConfigurationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SystemConfigurationDto>> GetAllSettingsAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        await InitializeDefaultSettingsAsync(cancellationToken);

        var query = _context.SystemConfigurations
            .Include(c => c.UpdatedByUser)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(c => c.Category == category);
        }

        return await query
            .OrderBy(c => c.Category)
            .ThenBy(c => c.Key)
            .Select(c => new SystemConfigurationDto(
                c.Id,
                c.Category,
                c.Key,
                c.Value,
                c.Description,
                c.IsEncrypted,
                c.UpdatedByUser != null ? c.UpdatedByUser.Email : "system",
                c.UpdatedAtUtc
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<SystemConfigurationDto> UpdateSettingAsync(
        UpdateConfigurationRequest request,
        Guid updatedByUserId,
        string userEmail,
        CancellationToken cancellationToken = default)
    {
        var setting = await _context.SystemConfigurations
            .FirstOrDefaultAsync(c => c.Category == request.Category && c.Key == request.Key, cancellationToken);

        if (setting == null)
        {
            setting = new SystemConfiguration
            {
                Id = Guid.NewGuid(),
                Category = request.Category,
                Key = request.Key,
                Value = request.Value,
                Description = request.Description,
                UpdatedByUserId = updatedByUserId,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _context.SystemConfigurations.Add(setting);
        }
        else
        {
            // Record history
            var history = new ConfigurationHistory
            {
                Id = Guid.NewGuid(),
                ConfigurationId = setting.Id,
                OldValue = setting.Value,
                NewValue = request.Value,
                ChangedByUserId = updatedByUserId,
                TimestampUtc = DateTime.UtcNow
            };
            _context.ConfigurationHistories.Add(history);

            setting.Value = request.Value;
            if (!string.IsNullOrWhiteSpace(request.Description)) setting.Description = request.Description;
            setting.UpdatedByUserId = updatedByUserId;
            setting.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new SystemConfigurationDto(
            setting.Id,
            setting.Category,
            setting.Key,
            setting.Value,
            setting.Description,
            setting.IsEncrypted,
            userEmail,
            setting.UpdatedAtUtc
        );
    }

    public async Task<bool> BulkUpdateSettingsAsync(
        BulkUpdateConfigurationRequest request,
        Guid updatedByUserId,
        string userEmail,
        CancellationToken cancellationToken = default)
    {
        foreach (var item in request.Settings)
        {
            await UpdateSettingAsync(item, updatedByUserId, userEmail, cancellationToken);
        }
        return true;
    }

    public async Task<IReadOnlyList<ConfigurationHistoryDto>> GetSettingHistoryAsync(Guid settingId, CancellationToken cancellationToken = default)
    {
        return await _context.ConfigurationHistories
            .Include(h => h.ChangedByUser)
            .AsNoTracking()
            .Where(h => h.ConfigurationId == settingId)
            .OrderByDescending(h => h.TimestampUtc)
            .Select(h => new ConfigurationHistoryDto(
                h.Id,
                h.ConfigurationId,
                h.OldValue,
                h.NewValue,
                h.ChangedByUser != null ? h.ChangedByUser.Email : "system",
                h.TimestampUtc
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task InitializeDefaultSettingsAsync(CancellationToken cancellationToken = default)
    {
        var defaults = new[]
        {
            // General
            ("General", "CompanyName", "Enterprise CRM Corporation", "Legal organization business name"),
            ("General", "CompanyLogoUrl", "/assets/logo.png", "Company logo URL path"),
            ("General", "DefaultCurrency", "VND", "System-wide default base currency"),
            ("General", "Timezone", "SE Asia Standard Time (UTC+07:00)", "Operational business timezone"),
            ("General", "DateFormat", "dd/MM/yyyy", "Standard localized date display format"),
            ("General", "DefaultPageSize", "20", "Default data table pagination size"),

            // CRM Pipeline
            ("CRM", "MaxActiveLeadsPerRep", "50", "Maximum active leads workload capacity per sales rep"),
            ("CRM", "StagnantLeadThresholdDays", "3", "Days without contact before flagging a lead as stagnant"),
            ("CRM", "StagnantDealThresholdDays", "14", "Days without stage change before flagging a deal as stagnant"),
            ("CRM", "ContractExpiringNoticeDays", "30", "Notice days ahead for contract renewal and expiration alerts"),

            // Security
            ("Security", "MinPasswordLength", "8", "Minimum length for user passwords"),
            ("Security", "RequireSpecialChar", "true", "Require special character in passwords"),
            ("Security", "SessionTimeoutMinutes", "120", "Inactivity timeout in minutes for user sessions"),
            ("Security", "MaxLoginAttempts", "5", "Maximum failed login attempts before lockout"),
            ("Security", "LockoutDurationMinutes", "15", "Account lockout duration in minutes"),

            // Notifications & Integrations
            ("Notification", "EnableInAppNotifications", "true", "Global toggle for in-app alert notifications"),
            ("Notification", "EnableEmailAlerts", "true", "Global toggle for external email dispatch"),
            ("Integration", "DefaultAiProvider", "Gemini", "Default LLM provider (Gemini / OpenAI)"),
            ("Integration", "WebhookRetryLimit", "3", "Maximum delivery retry attempts for webhooks")
        };

        bool anyAdded = false;
        var existingKeys = await _context.SystemConfigurations
            .Select(c => $"{c.Category}_{c.Key}")
            .ToHashSetAsync(cancellationToken);

        foreach (var (cat, key, val, desc) in defaults)
        {
            if (!existingKeys.Contains($"{cat}_{key}"))
            {
                _context.SystemConfigurations.Add(new SystemConfiguration
                {
                    Id = Guid.NewGuid(),
                    Category = cat,
                    Key = key,
                    Value = val,
                    Description = desc,
                    IsEncrypted = false,
                    UpdatedAtUtc = DateTime.UtcNow
                });
                anyAdded = true;
            }
        }

        if (anyAdded)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
