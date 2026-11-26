using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class AlertService : IAlertService
{
    private readonly IApplicationDbContext _context;

    public AlertService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AlertRuleDto>> GetRulesAsync(CancellationToken cancellationToken = default)
    {
        var rules = await _context.AlertRules.AsNoTracking().ToListAsync(cancellationToken);
        return rules.Select(r => new AlertRuleDto(
            r.Id,
            r.Name,
            r.Category,
            r.AlertType,
            r.ThresholdDays,
            r.ThresholdAmount,
            r.Severity,
            r.Channels,
            r.IsActive,
            r.Description
        )).ToList();
    }

    public async Task<AlertRuleDto> CreateRuleAsync(CreateAlertRuleRequest request, CancellationToken cancellationToken = default)
    {
        var rule = new AlertRule
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Category = request.Category,
            AlertType = request.AlertType,
            ThresholdDays = request.ThresholdDays,
            ThresholdAmount = request.ThresholdAmount,
            Severity = request.Severity,
            Channels = request.Channels,
            IsActive = request.IsActive,
            Description = request.Description
        };

        _context.AlertRules.Add(rule);
        await _context.SaveChangesAsync(cancellationToken);

        return new AlertRuleDto(
            rule.Id,
            rule.Name,
            rule.Category,
            rule.AlertType,
            rule.ThresholdDays,
            rule.ThresholdAmount,
            rule.Severity,
            rule.Channels,
            rule.IsActive,
            rule.Description
        );
    }

    public async Task<AlertRuleDto> UpdateRuleAsync(Guid id, CreateAlertRuleRequest request, CancellationToken cancellationToken = default)
    {
        var rule = await _context.AlertRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (rule == null)
            throw new KeyNotFoundException($"Alert rule {id} not found.");

        rule.Name = request.Name.Trim();
        rule.Category = request.Category;
        rule.AlertType = request.AlertType;
        rule.ThresholdDays = request.ThresholdDays;
        rule.ThresholdAmount = request.ThresholdAmount;
        rule.Severity = request.Severity;
        rule.Channels = request.Channels;
        rule.IsActive = request.IsActive;
        rule.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return new AlertRuleDto(
            rule.Id,
            rule.Name,
            rule.Category,
            rule.AlertType,
            rule.ThresholdDays,
            rule.ThresholdAmount,
            rule.Severity,
            rule.Channels,
            rule.IsActive,
            rule.Description
        );
    }

    public async Task<bool> DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _context.AlertRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (rule == null) return false;

        rule.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<BusinessAlertDto>> GetActiveAlertsAsync(string? category = null, string? severity = null, CancellationToken cancellationToken = default)
    {
        var query = _context.BusinessAlerts.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(a => a.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(severity))
        {
            query = query.Where(a => a.Severity == severity);
        }

        var alerts = await query.OrderByDescending(a => a.TriggeredAt).Take(100).ToListAsync(cancellationToken);

        return alerts.Select(a => new BusinessAlertDto(
            a.Id,
            a.AlertRuleId,
            a.Title,
            a.Message,
            a.Category,
            a.EntityType,
            a.EntityId,
            a.Severity,
            a.Status,
            a.AssignedToUserId,
            a.TriggeredAt,
            a.AcknowledgedAt,
            a.AcknowledgedBy,
            a.ResolvedAt,
            a.ResolvedBy,
            a.ResolutionNotes
        )).ToList();
    }

    public async Task<BusinessAlertDto?> AcknowledgeAlertAsync(Guid id, string? notes = null, CancellationToken cancellationToken = default)
    {
        var alert = await _context.BusinessAlerts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (alert == null) return null;

        alert.Status = "Acknowledged";
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.ResolutionNotes = notes;

        await _context.SaveChangesAsync(cancellationToken);

        return new BusinessAlertDto(
            alert.Id,
            alert.AlertRuleId,
            alert.Title,
            alert.Message,
            alert.Category,
            alert.EntityType,
            alert.EntityId,
            alert.Severity,
            alert.Status,
            alert.AssignedToUserId,
            alert.TriggeredAt,
            alert.AcknowledgedAt,
            alert.AcknowledgedBy,
            alert.ResolvedAt,
            alert.ResolvedBy,
            alert.ResolutionNotes
        );
    }

    public async Task<BusinessAlertDto?> ResolveAlertAsync(Guid id, string? notes = null, CancellationToken cancellationToken = default)
    {
        var alert = await _context.BusinessAlerts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (alert == null) return null;

        alert.Status = "Resolved";
        alert.ResolvedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(notes))
            alert.ResolutionNotes = notes;

        await _context.SaveChangesAsync(cancellationToken);

        return new BusinessAlertDto(
            alert.Id,
            alert.AlertRuleId,
            alert.Title,
            alert.Message,
            alert.Category,
            alert.EntityType,
            alert.EntityId,
            alert.Severity,
            alert.Status,
            alert.AssignedToUserId,
            alert.TriggeredAt,
            alert.AcknowledgedAt,
            alert.AcknowledgedBy,
            alert.ResolvedAt,
            alert.ResolvedBy,
            alert.ResolutionNotes
        );
    }

    public async Task<int> ScanAndTriggerAlertsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var duplicateCutoff = now.AddHours(-24);
        int triggeredCount = 0;

        // Existing active alerts within last 24h
        var recentAlertKeys = await _context.BusinessAlerts
            .Where(a => a.TriggeredAt >= duplicateCutoff)
            .Select(a => $"{a.EntityType}_{a.EntityId}_{a.Category}")
            .ToListAsync(cancellationToken);

        var recentSet = new HashSet<string>(recentAlertKeys);

        // 1. Scan Stagnant Leads (Created > 3 days ago, Status = New, no activity)
        var stagnantLeadThreshold = now.AddDays(-3);
        var stagnantLeads = await _context.Leads
            .Include(l => l.AssignedEmployee)
            .Where(l => l.Status == LeadStatus.New && l.CreatedAt <= stagnantLeadThreshold)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var lead in stagnantLeads)
        {
            var key = $"Lead_{lead.Id}_Lead";
            if (!recentSet.Contains(key))
            {
                var alert = new BusinessAlert
                {
                    Id = Guid.NewGuid(),
                    Title = $"Stagnant Lead: {lead.FullName}",
                    Message = $"Lead '{lead.FullName}' has been in 'New' status for over 3 days with no progression.",
                    Category = "Lead",
                    EntityType = "Lead",
                    EntityId = lead.Id,
                    Severity = "Warning",
                    Status = "Active",
                    TriggeredAt = now
                };
                _context.BusinessAlerts.Add(alert);
                recentSet.Add(key);
                triggeredCount++;
            }
        }

        // 2. Scan Stagnant Deals (In active stage for > 14 days)
        var stagnantDealThreshold = now.AddDays(-14);
        var stagnantDeals = await _context.Deals
            .Include(d => d.Account)
            .Where(d => d.Status == DealStatus.Open && d.UpdatedAt <= stagnantDealThreshold)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var deal in stagnantDeals)
        {
            var key = $"Deal_{deal.Id}_Deal";
            if (!recentSet.Contains(key))
            {
                var alert = new BusinessAlert
                {
                    Id = Guid.NewGuid(),
                    Title = $"Stagnant Deal: {deal.Title}",
                    Message = $"Deal '{deal.Title}' ({deal.Amount:N0} VND) has had no activity or stage transition for over 14 days.",
                    Category = "Deal",
                    EntityType = "Deal",
                    EntityId = deal.Id,
                    Severity = deal.Amount > 100_000_000 ? "Critical" : "Warning",
                    Status = "Active",
                    TriggeredAt = now
                };
                _context.BusinessAlerts.Add(alert);
                recentSet.Add(key);
                triggeredCount++;
            }
        }

        // 3. Scan Overdue Payments (PaymentSchedule DueDate < today, status != Paid)
        var todayDate = DateOnly.FromDateTime(now);
        var overduePayments = await _context.PaymentSchedules
            .Include(ps => ps.Contract)
            .Where(ps => ps.Status != "Paid" && ps.DueDate < todayDate)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var p in overduePayments)
        {
            var key = $"PaymentSchedule_{p.Id}_Finance";
            if (!recentSet.Contains(key))
            {
                var alert = new BusinessAlert
                {
                    Id = Guid.NewGuid(),
                    Title = $"Overdue Payment Milestone: {p.Contract?.ContractNumber}",
                    Message = $"Installment '{p.MilestoneTitle}' of {p.AmountDue:N0} VND was due on {p.DueDate:yyyy-MM-dd} and is overdue.",
                    Category = "Finance",
                    EntityType = "PaymentSchedule",
                    EntityId = p.Id,
                    Severity = "Critical",
                    Status = "Active",
                    TriggeredAt = now
                };
                _context.BusinessAlerts.Add(alert);
                recentSet.Add(key);
                triggeredCount++;
            }
        }

        // 4. Scan Contracts Expiring in 30 Days
        var expiringThreshold = DateOnly.FromDateTime(now.AddDays(30));
        var expiringContracts = await _context.Contracts
            .Where(c => c.Status == ContractStatus.Active && c.EndDate <= expiringThreshold && c.EndDate >= todayDate)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var c in expiringContracts)
        {
            var key = $"Contract_{c.Id}_Customer";
            if (!recentSet.Contains(key))
            {
                var alert = new BusinessAlert
                {
                    Id = Guid.NewGuid(),
                    Title = $"Contract Expiring Soon: {c.ContractNumber}",
                    Message = $"Contract '{c.Title}' with value {c.TotalValue:N0} VND will expire on {c.EndDate:yyyy-MM-dd}. Contact customer for renewal.",
                    Category = "Customer",
                    EntityType = "Contract",
                    EntityId = c.Id,
                    Severity = "Info",
                    Status = "Active",
                    TriggeredAt = now
                };
                _context.BusinessAlerts.Add(alert);
                recentSet.Add(key);
                triggeredCount++;
            }
        }

        if (triggeredCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return triggeredCount;
    }

    public async Task<NotificationPreferenceDto> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var pref = await _context.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (pref == null)
        {
            return new NotificationPreferenceDto(
                Guid.NewGuid(),
                userId,
                "All",
                true,
                true,
                "Info"
            );
        }

        return new NotificationPreferenceDto(
            pref.Id,
            pref.UserId,
            pref.Category,
            pref.InAppEnabled,
            pref.EmailEnabled,
            pref.MinSeverity
        );
    }

    public async Task<NotificationPreferenceDto> UpdateUserPreferencesAsync(Guid userId, UpdateNotificationPreferenceRequest request, CancellationToken cancellationToken = default)
    {
        var pref = await _context.NotificationPreferences.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (pref == null)
        {
            pref = new NotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Category = request.Category,
                InAppEnabled = request.InAppEnabled,
                EmailEnabled = request.EmailEnabled,
                MinSeverity = request.MinSeverity
            };
            _context.NotificationPreferences.Add(pref);
        }
        else
        {
            pref.Category = request.Category;
            pref.InAppEnabled = request.InAppEnabled;
            pref.EmailEnabled = request.EmailEnabled;
            pref.MinSeverity = request.MinSeverity;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new NotificationPreferenceDto(
            pref.Id,
            pref.UserId,
            pref.Category,
            pref.InAppEnabled,
            pref.EmailEnabled,
            pref.MinSeverity
        );
    }
}
