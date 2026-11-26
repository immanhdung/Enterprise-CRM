using System.Text.Json;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class SalesAnalyticsService : ISalesAnalyticsService
{
    private readonly IApplicationDbContext _context;

    public SalesAnalyticsService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalesAnalysisSummaryDto> GetPerformanceAnalysisAsync(int? month = null, int? year = null, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var targetMonth = month ?? now.Month;
        var targetYear = year ?? now.Year;

        var startDateOnly = new DateOnly(targetYear, targetMonth, 1);
        var endDateOnly = startDateOnly.AddMonths(1).AddDays(-1);

        var wonDeals = await _context.Deals
            .Where(d => d.Status == DealStatus.Won && d.ActualClosingDate >= startDateOnly && d.ActualClosingDate <= endDateOnly)
            .ToListAsync(cancellationToken);

        var lostDeals = await _context.Deals
            .Where(d => d.Status == DealStatus.Lost && d.ActualClosingDate >= startDateOnly && d.ActualClosingDate <= endDateOnly)
            .ToListAsync(cancellationToken);

        var totalClosed = wonDeals.Count + lostDeals.Count;
        var winRate = totalClosed > 0 ? Math.Round((decimal)wonDeals.Count / totalClosed * 100m, 1) : 0m;

        var openDeals = await _context.Deals
            .Where(d => d.Status == DealStatus.Open)
            .ToListAsync(cancellationToken);

        var pipelineValue = openDeals.Sum(d => d.Amount);

        // Calculate pipeline health score (scale 0 - 100)
        // Factors: win rate (40%), deal velocity (30%), active volume (30%)
        var pipelineHealthScore = Math.Min(100m, Math.Round((winRate * 0.4m) + (openDeals.Count > 10 ? 30m : openDeals.Count * 3m) + 25m, 1));

        var strengths = new List<string>();
        var weaknesses = new List<string>();
        var recommendations = new List<string>();

        if (winRate >= 50m)
        {
            strengths.Add($"Tỷ lệ chốt hợp đồng (Win Rate) đạt mức ấn tượng {winRate}% trong tháng.");
        }
        else
        {
            weaknesses.Add($"Tỷ lệ chốt deal hiện ở mức {winRate}%, thấp hơn mục tiêu tối thiểu 50%.");
            recommendations.Add("Tập trung đào tạo kỹ năng xử lý từ chối và hỗ trợ sales ở khâu đàm phán hợp đồng.");
        }

        if (wonDeals.Any())
        {
            var totalWonAmount = wonDeals.Sum(d => d.Amount);
            strengths.Add($"Tổng giá trị hợp đồng đã chốt đạt {totalWonAmount:N0} VND với {wonDeals.Count} thương vụ thành công.");
        }

        var stagnantCount = openDeals.Count(d => d.UpdatedAt <= now.AddDays(-14));
        if (stagnantCount > 0)
        {
            weaknesses.Add($"Có {stagnantCount} cơ hội bán hàng bị đình trệ (không có tương tác hơn 14 ngày).");
            recommendations.Add("Khởi tạo chiến dịch tái kích hoạt (re-engagement) hoặc phân bổ lại lead cho các nhân sự có công suất trống.");
        }
        else
        {
            strengths.Add("Pipeline vận hành ổn định, không ghi nhận các cụm deal bị tắc nghẽn kéo dài.");
        }

        recommendations.Add("Tăng cường các cuộc hẹn trực tiếp với các khách hàng doanh nghiệp có quy mô ngân sách trên 100 triệu VND.");

        var overviewText = $"Phân tích hiệu suất kinh doanh tháng {targetMonth}/{targetYear}: Hệ thống ghi nhận {openDeals.Count} deal đang mở với tổng dung lượng pipeline {pipelineValue:N0} VND. Điểm sức khỏe phễu bán hàng đạt {pipelineHealthScore}/100.";

        return new SalesAnalysisSummaryDto(
            overviewText,
            strengths,
            weaknesses,
            recommendations,
            winRate,
            pipelineHealthScore
        );
    }

    public async Task<List<BusinessRecommendationDto>> GetRecommendationsAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.BusinessRecommendations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status);
        }

        var list = await query.OrderByDescending(r => r.GeneratedAt).Take(50).ToListAsync(cancellationToken);

        // If none exist, seed default recommendations dynamically
        if (!list.Any())
        {
            await SeedInitialRecommendationsAsync(cancellationToken);
            list = await query.OrderByDescending(r => r.GeneratedAt).Take(50).ToListAsync(cancellationToken);
        }

        return list.Select(r => new BusinessRecommendationDto(
            r.Id,
            r.Category,
            r.TargetEntityType,
            r.TargetEntityId,
            r.TargetEntityType,
            r.Title,
            r.Description,
            r.Priority,
            r.Status,
            r.GeneratedAt,
            r.ActionPayloadJson
        )).ToList();
    }

    public async Task<bool> ApplyRecommendationAsync(Guid id, ApplyRecommendationRequest request, CancellationToken cancellationToken = default)
    {
        var rec = await _context.BusinessRecommendations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (rec == null) return false;

        rec.Status = request.Action == "Apply" ? "Applied" : "Dismissed";
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task SeedInitialRecommendationsAsync(CancellationToken cancellationToken)
    {
        var leads = await _context.Leads.Where(l => l.Status == LeadStatus.New).Take(3).ToListAsync(cancellationToken);
        var deals = await _context.Deals.Where(d => d.Status == DealStatus.Open).OrderByDescending(d => d.Amount).Take(2).ToListAsync(cancellationToken);

        var recommendations = new List<BusinessRecommendation>();

        foreach (var lead in leads)
        {
            recommendations.Add(new BusinessRecommendation
            {
                Id = Guid.NewGuid(),
                Category = "LeadPriority",
                TargetEntityType = "Lead",
                TargetEntityId = lead.Id,
                Title = $"Ưu tiên liên hệ Lead: {lead.FullName}",
                Description = $"Lead '{lead.FullName}' ({lead.CompanyName ?? "Khách hàng cá nhân"}) có điểm tiềm năng cao. Khuyến nghị đặt lịch hẹn tư vấn giải pháp trong 24 giờ tới.",
                Priority = "High",
                Status = "Pending",
                GeneratedAt = DateTime.UtcNow
            });
        }

        foreach (var deal in deals)
        {
            recommendations.Add(new BusinessRecommendation
            {
                Id = Guid.NewGuid(),
                Category = "DealIntervention",
                TargetEntityType = "Deal",
                TargetEntityId = deal.Id,
                Title = $"Thúc đẩy chốt Deal: {deal.Title}",
                Description = $"Deal giá trị {deal.Amount:N0} VND đang ở giai đoạn thương lượng. Khuyến nghị gửi đề xuất chính sách chiết khấu thanh toán sớm để ký hợp đồng trước cuối tháng.",
                Priority = "Urgent",
                Status = "Pending",
                GeneratedAt = DateTime.UtcNow
            });
        }

        if (recommendations.Any())
        {
            _context.BusinessRecommendations.AddRange(recommendations);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
