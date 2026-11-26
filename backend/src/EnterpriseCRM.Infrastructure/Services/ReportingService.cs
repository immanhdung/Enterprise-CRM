using System.Text.Json;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class ReportingService : IReportingService
{
    private readonly IApplicationDbContext _context;

    public ReportingService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ScheduledReportDto>> GetScheduledReportsAsync(CancellationToken cancellationToken = default)
    {
        var list = await _context.ScheduledReports.AsNoTracking().ToListAsync(cancellationToken);
        return list.Select(r => new ScheduledReportDto(
            r.Id,
            r.Name,
            r.ReportType,
            JsonSerializer.Deserialize<List<string>>(r.RecipientsEmailJson) ?? new List<string>(),
            r.Format,
            r.TimeOfDay,
            r.DayOfWeek,
            r.DayOfMonth,
            r.IsActive,
            r.LastRunAt
        )).ToList();
    }

    public async Task<ScheduledReportDto> CreateScheduledReportAsync(CreateScheduledReportRequest request, CancellationToken cancellationToken = default)
    {
        var report = new ScheduledReport
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            ReportType = request.ReportType,
            RecipientsEmailJson = JsonSerializer.Serialize(request.RecipientsEmails),
            Format = request.Format,
            TimeOfDay = request.TimeOfDay,
            DayOfWeek = request.DayOfWeek,
            DayOfMonth = request.DayOfMonth,
            IsActive = request.IsActive
        };

        _context.ScheduledReports.Add(report);
        await _context.SaveChangesAsync(cancellationToken);

        return new ScheduledReportDto(
            report.Id,
            report.Name,
            report.ReportType,
            request.RecipientsEmails,
            report.Format,
            report.TimeOfDay,
            report.DayOfWeek,
            report.DayOfMonth,
            report.IsActive,
            report.LastRunAt
        );
    }

    public async Task<bool> DeleteScheduledReportAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _context.ScheduledReports.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (report == null) return false;

        report.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<GeneratedReportDto>> GetGeneratedReportsAsync(string? reportType = null, int limit = 30, CancellationToken cancellationToken = default)
    {
        var query = _context.GeneratedReports.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(reportType))
        {
            query = query.Where(r => r.ReportType == reportType);
        }

        var list = await query.OrderByDescending(r => r.GeneratedAt).Take(limit).ToListAsync(cancellationToken);

        return list.Select(r => new GeneratedReportDto(
            r.Id,
            r.ScheduledReportId,
            r.ReportType,
            r.Title,
            r.PeriodStart,
            r.PeriodEnd,
            r.SummaryJson,
            r.FullDataJson,
            r.FileUrl,
            r.GeneratedAt
        )).ToList();
    }

    public async Task<GeneratedReportDto> GenerateReportAsync(GenerateReportRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        DateTime periodStart;
        DateTime periodEnd;

        if (request.PeriodStart.HasValue && request.PeriodEnd.HasValue)
        {
            periodStart = request.PeriodStart.Value;
            periodEnd = request.PeriodEnd.Value;
        }
        else
        {
            switch (request.ReportType)
            {
                case "Daily":
                    periodStart = now.Date;
                    periodEnd = now.Date.AddDays(1).AddTicks(-1);
                    break;
                case "Weekly":
                    periodStart = now.Date.AddDays(-(int)now.DayOfWeek + 1); // Monday
                    periodEnd = periodStart.AddDays(7).AddTicks(-1);
                    break;
                case "Monthly":
                default:
                    periodStart = new DateTime(now.Year, now.Month, 1);
                    periodEnd = periodStart.AddMonths(1).AddTicks(-1);
                    break;
            }
        }

        var data = await GetReportDataAsync(request.ReportType, periodStart, periodEnd, cancellationToken);

        var title = $"{request.ReportType} Executive Business Report ({periodStart:yyyy-MM-dd} - {periodEnd:yyyy-MM-dd})";

        var summary = new
        {
            totalRevenue = data.TotalRevenue,
            totalCollectedCash = data.TotalCollectedCash,
            totalReceivables = data.TotalReceivables,
            dealsWonCount = data.DealsWonCount,
            dealsWonValue = data.DealsWonValue,
            newLeadsCount = data.NewLeadsCount,
            activeContractsCount = data.ActiveContractsCount
        };

        var generated = new GeneratedReport
        {
            Id = Guid.NewGuid(),
            ReportType = request.ReportType,
            Title = title,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            SummaryJson = JsonSerializer.Serialize(summary),
            FullDataJson = JsonSerializer.Serialize(data),
            GeneratedAt = DateTime.UtcNow
        };

        _context.GeneratedReports.Add(generated);
        await _context.SaveChangesAsync(cancellationToken);

        return new GeneratedReportDto(
            generated.Id,
            generated.ScheduledReportId,
            generated.ReportType,
            generated.Title,
            generated.PeriodStart,
            generated.PeriodEnd,
            generated.SummaryJson,
            generated.FullDataJson,
            generated.FileUrl,
            generated.GeneratedAt
        );
    }

    public async Task<ManagementReportDataDto> GetReportDataAsync(string reportType, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default)
    {
        // 1. Financial Metrics
        var collectedCash = await _context.Payments
            .Where(p => p.Status == "Completed" && p.PaymentDate >= periodStart && p.PaymentDate <= periodEnd)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

        var contractsInPeriod = await _context.Contracts
            .Where(c => c.CreatedAt >= periodStart && c.CreatedAt <= periodEnd)
            .ToListAsync(cancellationToken);

        var totalRevenue = contractsInPeriod.Sum(c => c.TotalValue);

        var unpaidSchedules = await _context.PaymentSchedules
            .Where(s => s.Status != "Paid")
            .ToListAsync(cancellationToken);
        var totalReceivables = unpaidSchedules.Sum(s => s.AmountDue - s.AmountPaid);

        // 2. Leads Metrics
        var newLeadsCount = await _context.Leads
            .CountAsync(l => l.CreatedAt >= periodStart && l.CreatedAt <= periodEnd, cancellationToken);

        var qualifiedLeadsCount = await _context.Leads
            .CountAsync(l => (l.Status == LeadStatus.Qualified || l.Status == LeadStatus.Converted) && l.UpdatedAt >= periodStart && l.UpdatedAt <= periodEnd, cancellationToken);

        // 3. Deals Metrics
        var dealsCreatedCount = await _context.Deals
            .CountAsync(d => d.CreatedAt >= periodStart && d.CreatedAt <= periodEnd, cancellationToken);

        var startPeriodDate = DateOnly.FromDateTime(periodStart);
        var endPeriodDate = DateOnly.FromDateTime(periodEnd);

        var wonDeals = await _context.Deals
            .Include(d => d.AssignedEmployee)
                .ThenInclude(e => e!.User)
            .Where(d => d.Status == DealStatus.Won && d.ActualClosingDate >= startPeriodDate && d.ActualClosingDate <= endPeriodDate)
            .ToListAsync(cancellationToken);

        var dealsWonCount = wonDeals.Count;
        var dealsWonValue = wonDeals.Sum(d => d.Amount);

        // 4. Contracts Metrics
        var activeContracts = await _context.Contracts
            .Where(c => c.Status == ContractStatus.Active)
            .ToListAsync(cancellationToken);

        var activeContractsCount = activeContracts.Count;
        var activeContractsValue = activeContracts.Sum(c => c.TotalValue);

        // 5. Payroll
        var totalPayrollPaid = await _context.PayrollRecords
            .Where(p => p.Status == "Paid" && p.UpdatedAt >= periodStart && p.UpdatedAt <= periodEnd)
            .SumAsync(p => (decimal?)p.NetSalary, cancellationToken) ?? 0m;

        // 6. Top Performers
        var topPerformers = wonDeals
            .GroupBy(d => d.AssignedEmployeeId)
            .Select(g =>
            {
                var emp = g.First().AssignedEmployee;
                var name = emp?.User != null ? $"{emp.User.FirstName} {emp.User.LastName}" : emp?.EmployeeCode ?? "Unknown";
                var achieved = g.Sum(d => d.Amount);
                return new RepPerformanceReportItemDto(
                    name,
                    "Sales Team",
                    achieved,
                    g.Count(),
                    achieved > 50_000_000 ? 100m : Math.Round(achieved / 50_000_000m * 100m, 1)
                );
            })
            .OrderByDescending(r => r.AchievedRevenue)
            .Take(5)
            .ToList();

        var periodLabel = $"{periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}";

        return new ManagementReportDataDto(
            $"{reportType} Management Report",
            periodLabel,
            totalRevenue,
            collectedCash,
            totalReceivables,
            newLeadsCount,
            qualifiedLeadsCount,
            dealsCreatedCount,
            dealsWonCount,
            dealsWonValue,
            activeContractsCount,
            activeContractsValue,
            totalPayrollPaid,
            topPerformers
        );
    }
}
