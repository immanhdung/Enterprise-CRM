using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.PerformanceAndCompensation;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class SalesPerformanceService : ISalesPerformanceService
{
    private readonly IApplicationDbContext _context;

    public SalesPerformanceService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalesPerformanceSummaryDto> GetPerformanceSummaryAsync(
        int? month,
        int? year,
        Guid? employeeId,
        Guid? departmentId,
        Guid? teamId,
        CancellationToken ct = default)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var hasMonth = month.HasValue;

        var startCurrent = hasMonth
            ? new DateTime(targetYear, month!.Value, 1, 0, 0, 0, DateTimeKind.Utc)
            : new DateTime(targetYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var endCurrent = hasMonth
            ? startCurrent.AddMonths(1).AddTicks(-1)
            : startCurrent.AddYears(1).AddTicks(-1);

        // Previous period for growth comparison
        var startPrevious = hasMonth ? startCurrent.AddMonths(-1) : startCurrent.AddYears(-1);
        var endPrevious = hasMonth ? startCurrent.AddTicks(-1) : startCurrent.AddTicks(-1);

        // 1. Leads
        var leadsQuery = _context.Leads.AsNoTracking().Where(l => l.CreatedAt >= startCurrent && l.CreatedAt <= endCurrent);
        if (employeeId.HasValue) leadsQuery = leadsQuery.Where(l => l.AssignedEmployeeId == employeeId.Value);

        var totalLeads = await leadsQuery.CountAsync(ct);
        var qualifiedLeads = await leadsQuery.CountAsync(l => l.Status == LeadStatus.Qualified, ct);
        var convertedLeads = await leadsQuery.CountAsync(l => l.Status == LeadStatus.Converted, ct);
        var convRate = totalLeads > 0 ? Math.Round(((decimal)convertedLeads / totalLeads) * 100m, 2) : 0m;

        // 2. Deals
        var dealsQuery = _context.Deals.AsNoTracking().Where(d => d.CreatedAt >= startCurrent && d.CreatedAt <= endCurrent);
        if (employeeId.HasValue) dealsQuery = dealsQuery.Where(d => d.AssignedEmployeeId == employeeId.Value);

        var totalDeals = await dealsQuery.CountAsync(ct);
        var wonDeals = await dealsQuery.CountAsync(d => d.Status == DealStatus.Won, ct);
        var lostDeals = await dealsQuery.CountAsync(d => d.Status == DealStatus.Lost, ct);
        var winRate = (wonDeals + lostDeals) > 0 ? Math.Round(((decimal)wonDeals / (wonDeals + lostDeals)) * 100m, 2) : 0m;
        var avgDealVal = wonDeals > 0
            ? Math.Round((await dealsQuery.Where(d => d.Status == DealStatus.Won).SumAsync(d => (decimal?)d.Amount, ct) ?? 0m) / wonDeals, 2)
            : 0m;

        // 3. Contracts, Invoices, Collected Cash (Strict accounting separation)
        var contractsQuery = _context.Contracts.AsNoTracking().Where(c => c.CreatedAt >= startCurrent && c.CreatedAt <= endCurrent);
        var contractVal = await contractsQuery.SumAsync(c => (decimal?)c.TotalValue, ct) ?? 0m;

        var invoicesQuery = _context.Invoices.AsNoTracking().Where(i => i.CreatedAt >= startCurrent && i.CreatedAt <= endCurrent && i.Status != InvoiceStatus.Cancelled);
        var invoicedVal = await invoicesQuery.SumAsync(i => (decimal?)i.TotalWithTax, ct) ?? 0m;

        var paymentsQuery = _context.Payments.AsNoTracking().Where(p => p.PaymentDate >= startCurrent && p.PaymentDate <= endCurrent && p.Status == "Confirmed");
        var collectedCash = await paymentsQuery.SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

        var schedulesQuery = _context.PaymentSchedules.AsNoTracking().Where(s => s.Status == "Pending" || s.Status == "PartiallyPaid" || s.Status == "Overdue");
        var outstanding = await schedulesQuery.SumAsync(s => (decimal?)(s.AmountDue - s.AmountPaid), ct) ?? 0m;

        // New Customer vs Existing Customer Revenue
        var newCustomerRevenue = await contractsQuery
            .Where(c => c.Account != null && c.Account.CreatedAt >= startCurrent.AddMonths(-3))
            .SumAsync(c => (decimal?)c.TotalValue, ct) ?? 0m;
        var existingCustomerRevenue = Math.Max(0m, contractVal - newCustomerRevenue);

        // Previous Period Metrics for MoM / YoY growth calculation
        var prevContractsQuery = _context.Contracts.AsNoTracking().Where(c => c.CreatedAt >= startPrevious && c.CreatedAt <= endPrevious);
        var prevContractVal = await prevContractsQuery.SumAsync(c => (decimal?)c.TotalValue, ct) ?? 0m;

        var prevDealsQuery = _context.Deals.AsNoTracking().Where(d => d.CreatedAt >= startPrevious && d.CreatedAt <= endPrevious && d.Status == DealStatus.Won);
        var prevWonDeals = await prevDealsQuery.CountAsync(ct);

        var prevLeadsQuery = _context.Leads.AsNoTracking().Where(l => l.CreatedAt >= startPrevious && l.CreatedAt <= endPrevious);
        var prevTotalLeads = await prevLeadsQuery.CountAsync(ct);
        var prevConvertedLeads = await prevLeadsQuery.CountAsync(l => l.Status == LeadStatus.Converted, ct);
        var prevConvRate = prevTotalLeads > 0 ? ((decimal)prevConvertedLeads / prevTotalLeads) * 100m : 0m;

        var revGrowth = prevContractVal > 0 ? Math.Round(((contractVal - prevContractVal) / prevContractVal) * 100m, 2) : 0m;
        var dealsGrowth = prevWonDeals > 0 ? Math.Round((((decimal)wonDeals - prevWonDeals) / prevWonDeals) * 100m, 2) : 0m;
        var convGrowth = prevConvRate > 0 ? Math.Round(convRate - prevConvRate, 2) : 0m;

        return new SalesPerformanceSummaryDto(
            totalLeads,
            qualifiedLeads,
            convertedLeads,
            convRate,
            totalDeals,
            wonDeals,
            lostDeals,
            winRate,
            avgDealVal,
            AverageSalesCycleDays: 14,
            contractVal,
            invoicedVal,
            collectedCash,
            outstanding,
            newCustomerRevenue,
            existingCustomerRevenue,
            revGrowth,
            dealsGrowth,
            convGrowth
        );
    }

    public async Task<List<PerformanceRankingItemDto>> GetEmployeeRankingsAsync(
        int? month,
        int? year,
        Guid? departmentId,
        CancellationToken ct = default)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        var start = new DateTime(targetYear, targetMonth, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1).AddTicks(-1);

        var employeesQuery = _context.Employees
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Team)
            .AsNoTracking()
            .AsQueryable();

        if (departmentId.HasValue)
        {
            employeesQuery = employeesQuery.Where(e => e.DepartmentId == departmentId.Value);
        }

        var employees = await employeesQuery.ToListAsync(ct);
        var rankings = new List<PerformanceRankingItemDto>();

        foreach (var emp in employees)
        {
            var empName = $"{emp.User?.FirstName} {emp.User?.LastName}".Trim();

            // Won deals in period
            var wonDeals = await _context.Deals
                .AsNoTracking()
                .Where(d => d.AssignedEmployeeId == emp.Id && d.Status == DealStatus.Won && d.UpdatedAt >= start && d.UpdatedAt <= end)
                .CountAsync(ct);

            // Contract value
            var contractVal = await _context.Contracts
                .AsNoTracking()
                .Where(c => c.CreatedBy == emp.UserId && c.CreatedAt >= start && c.CreatedAt <= end)
                .SumAsync(c => (decimal?)c.TotalValue, ct) ?? 0m;

            // Collected cash (Confirmed payments on contracts owned by employee)
            var collectedCash = await _context.Payments
                .AsNoTracking()
                .Where(p => p.Contract.CreatedBy == emp.UserId && p.Status == "Confirmed" && p.PaymentDate >= start && p.PaymentDate <= end)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            // KPI score
            var kpi = await _context.Kpis
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.EmployeeId == emp.Id && k.PeriodMonth == targetMonth && k.PeriodYear == targetYear, ct);
            var achievementPercent = kpi?.AchievementScore ?? 0m;

            rankings.Add(new PerformanceRankingItemDto(
                0,
                emp.Id,
                empName,
                emp.EmployeeCode,
                emp.Department?.Name,
                emp.Team?.Name,
                wonDeals,
                contractVal,
                collectedCash,
                achievementPercent
            ));
        }

        // Rank by Contract Value descending
        var ordered = rankings
            .OrderByDescending(r => r.ContractValue)
            .ThenByDescending(r => r.CollectedCash)
            .Select((item, idx) => item with { Rank = idx + 1 })
            .ToList();

        return ordered;
    }

    public async Task<List<PerformanceTimelineItemDto>> GetTimelineAnalyticsAsync(
        string timeframe,
        int? year,
        Guid? employeeId,
        Guid? departmentId,
        CancellationToken ct = default)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var list = new List<PerformanceTimelineItemDto>();

        if (timeframe.Equals("quarterly", StringComparison.OrdinalIgnoreCase))
        {
            for (var q = 1; q <= 4; q++)
            {
                var qStart = new DateTime(targetYear, (q - 1) * 3 + 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var qEnd = qStart.AddMonths(3).AddTicks(-1);

                var (cVal, iVal, cash, deals) = await GetPeriodTotalsAsync(qStart, qEnd, employeeId, departmentId, ct);
                list.Add(new PerformanceTimelineItemDto($"Q{q}/{targetYear}", cVal, iVal, cash, deals));
            }
        }
        else
        {
            // Monthly (1 to 12)
            for (var m = 1; m <= 12; m++)
            {
                var mStart = new DateTime(targetYear, m, 1, 0, 0, 0, DateTimeKind.Utc);
                var mEnd = mStart.AddMonths(1).AddTicks(-1);

                var (cVal, iVal, cash, deals) = await GetPeriodTotalsAsync(mStart, mEnd, employeeId, departmentId, ct);
                list.Add(new PerformanceTimelineItemDto($"T{m}/{targetYear}", cVal, iVal, cash, deals));
            }
        }

        return list;
    }

    private async Task<(decimal ContractVal, decimal InvoicedVal, decimal CollectedCash, int WonDeals)> GetPeriodTotalsAsync(
        DateTime start,
        DateTime end,
        Guid? employeeId,
        Guid? departmentId,
        CancellationToken ct)
    {
        var contractsQuery = _context.Contracts.AsNoTracking().Where(c => c.CreatedAt >= start && c.CreatedAt <= end);
        var invoicesQuery = _context.Invoices.AsNoTracking().Where(i => i.CreatedAt >= start && i.CreatedAt <= end && i.Status != InvoiceStatus.Cancelled);
        var paymentsQuery = _context.Payments.AsNoTracking().Where(p => p.PaymentDate >= start && p.PaymentDate <= end && p.Status == "Confirmed");
        var dealsQuery = _context.Deals.AsNoTracking().Where(d => d.Status == DealStatus.Won && d.UpdatedAt >= start && d.UpdatedAt <= end);

        if (employeeId.HasValue)
        {
            contractsQuery = contractsQuery.Where(c => c.CreatedBy == employeeId.Value);
            dealsQuery = dealsQuery.Where(d => d.AssignedEmployeeId == employeeId.Value);
        }

        var cVal = await contractsQuery.SumAsync(c => (decimal?)c.TotalValue, ct) ?? 0m;
        var iVal = await invoicesQuery.SumAsync(i => (decimal?)i.TotalWithTax, ct) ?? 0m;
        var cash = await paymentsQuery.SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
        var deals = await dealsQuery.CountAsync(ct);

        return (cVal, iVal, cash, deals);
    }
}
