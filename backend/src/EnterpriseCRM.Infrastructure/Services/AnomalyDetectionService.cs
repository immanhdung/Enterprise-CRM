using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class AnomalyDetectionService : IAnomalyDetectionService
{
    private readonly IApplicationDbContext _context;

    public AnomalyDetectionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AnomalyDetectionDto>> GetAnomaliesAsync(string? category = null, string? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AnomalyDetections.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(a => a.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(a => a.Status == status);
        }

        var list = await query.OrderByDescending(a => a.DetectedAt).Take(50).ToListAsync(cancellationToken);

        // If none exist yet, run scan automatically
        if (!list.Any())
        {
            await RunAnomalyScanAsync(cancellationToken);
            list = await query.OrderByDescending(a => a.DetectedAt).Take(50).ToListAsync(cancellationToken);
        }

        return list.Select(a => new AnomalyDetectionDto(
            a.Id,
            a.MetricName,
            a.Category,
            a.Severity,
            a.ActualValue,
            a.ExpectedValue,
            a.DeviationPercent,
            a.Description,
            a.DetectedAt,
            a.Status,
            a.ResolutionNotes
        )).ToList();
    }

    public async Task<int> RunAnomalyScanAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        int detectedCount = 0;

        // Cutoff to avoid duplicate scans on same day
        var todayStart = now.Date;
        var existingMetricsToday = await _context.AnomalyDetections
            .Where(a => a.DetectedAt >= todayStart)
            .Select(a => a.MetricName)
            .ToListAsync(cancellationToken);

        var existingSet = new HashSet<string>(existingMetricsToday);

        // 1. Scan Stagnant Deals Cluster
        if (!existingSet.Contains("StagnantDealCluster"))
        {
            var stagnantThreshold = now.AddDays(-21);
            var stagnantCount = await _context.Deals
                .CountAsync(d => d.Status == DealStatus.Open && d.UpdatedAt <= stagnantThreshold, cancellationToken);

            var totalOpen = await _context.Deals.CountAsync(d => d.Status == DealStatus.Open, cancellationToken);
            if (totalOpen > 0 && stagnantCount >= 3)
            {
                var stagnantPercent = Math.Round((decimal)stagnantCount / totalOpen * 100m, 1);
                if (stagnantPercent >= 20m)
                {
                    _context.AnomalyDetections.Add(new AnomalyDetection
                    {
                        Id = Guid.NewGuid(),
                        MetricName = "StagnantDealCluster",
                        Category = "Pipeline",
                        Severity = stagnantPercent >= 40m ? "Critical" : "Warning",
                        ActualValue = stagnantCount,
                        ExpectedValue = Math.Max(1, (int)(totalOpen * 0.1m)),
                        DeviationPercent = stagnantPercent,
                        Description = $"Phát hiện {stagnantCount}/{totalOpen} cơ hội bán hàng ({stagnantPercent}%) bị đình trệ hơn 21 ngày mà không có chuyển đổi giai đoạn.",
                        DetectedAt = now,
                        Status = "Active"
                    });
                    detectedCount++;
                }
            }
        }

        // 2. Scan Payment Delay Spikes
        if (!existingSet.Contains("PaymentDelaySpike"))
        {
            var overdueDate = DateOnly.FromDateTime(now.AddDays(-15));
            var overdueSchedules = await _context.PaymentSchedules
                .Where(s => s.Status != "Paid" && s.DueDate <= overdueDate)
                .ToListAsync(cancellationToken);

            if (overdueSchedules.Count >= 2)
            {
                var overdueAmount = overdueSchedules.Sum(s => s.AmountDue - s.AmountPaid);
                _context.AnomalyDetections.Add(new AnomalyDetection
                {
                    Id = Guid.NewGuid(),
                    MetricName = "PaymentDelaySpike",
                    Category = "Finance",
                    Severity = "High",
                    ActualValue = overdueAmount,
                    ExpectedValue = 0,
                    DeviationPercent = 100m,
                    Description = $"Ghi nhận {overdueSchedules.Count} đợt thanh toán quá hạn trên 15 ngày với tổng công nợ đọng {overdueAmount:N0} VND.",
                    DetectedAt = now,
                    Status = "Active"
                });
                detectedCount++;
            }
        }

        // 3. Scan Deal Win-Rate Drop
        if (!existingSet.Contains("DealWinRateDrop"))
        {
            var last30d = DateOnly.FromDateTime(now.AddDays(-30));
            var wonCount = await _context.Deals.CountAsync(d => d.Status == DealStatus.Won && d.ActualClosingDate >= last30d, cancellationToken);
            var lostCount = await _context.Deals.CountAsync(d => d.Status == DealStatus.Lost && d.ActualClosingDate >= last30d, cancellationToken);

            var totalClosed = wonCount + lostCount;
            if (totalClosed >= 4)
            {
                var winRate = Math.Round((decimal)wonCount / totalClosed * 100m, 1);
                if (winRate < 35m)
                {
                    _context.AnomalyDetections.Add(new AnomalyDetection
                    {
                        Id = Guid.NewGuid(),
                        MetricName = "DealWinRateDrop",
                        Category = "Performance",
                        Severity = "High",
                        ActualValue = winRate,
                        ExpectedValue = 50m,
                        DeviationPercent = Math.Round((50m - winRate) / 50m * 100m, 1),
                        Description = $"Tỷ lệ chốt hợp đồng trong 30 ngày qua giảm còn {winRate}% (kỳ vọng tối thiểu 50%). Số deal thất bại: {lostCount}/{totalClosed}.",
                        DetectedAt = now,
                        Status = "Active"
                    });
                    detectedCount++;
                }
            }
        }

        if (detectedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return detectedCount;
    }

    public async Task<bool> ResolveAnomalyAsync(Guid id, ResolveAnomalyRequest request, CancellationToken cancellationToken = default)
    {
        var anomaly = await _context.AnomalyDetections.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (anomaly == null) return false;

        anomaly.Status = request.Status;
        anomaly.ResolutionNotes = request.ResolutionNotes;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
