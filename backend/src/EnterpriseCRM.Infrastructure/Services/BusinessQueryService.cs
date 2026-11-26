using System.Text.Json;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class BusinessQueryService : IBusinessQueryService
{
    private readonly IApplicationDbContext _context;

    public BusinessQueryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BusinessQueryResultDto> ExecuteNaturalLanguageQueryAsync(BusinessQueryRequest request, CancellationToken cancellationToken = default)
    {
        var query = request.Query.Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

        // Pattern 1: Revenue / Cash Collection ("doanh thu", "thực thu", "revenue", "tiền thu")
        if (query.Contains("doanh thu") || query.Contains("revenue") || query.Contains("thực thu") || query.Contains("tiền thu"))
        {
            var payments = await _context.Payments
                .Where(p => (p.Status == "Completed" || p.Status == "Confirmed") && p.PaymentDate >= startOfMonth && p.PaymentDate <= endOfMonth)
                .ToListAsync(cancellationToken);

            var totalCollected = payments.Sum(p => p.Amount);
            var paymentCount = payments.Count;

            var contracts = await _context.Contracts
                .Where(c => c.CreatedAt >= startOfMonth && c.CreatedAt <= endOfMonth)
                .ToListAsync(cancellationToken);

            var contractRevenue = contracts.Sum(c => c.TotalValue);

            var columns = new List<string> { "Chỉ số", "Giá trị (VND)", "Số lượng giao dịch" };
            var rows = new List<Dictionary<string, object>>
            {
                new() { { "Chỉ số", "Thực thu dòng tiền (Collected Cash)" }, { "Giá trị (VND)", totalCollected }, { "Số lượng giao dịch", paymentCount } },
                new() { { "Chỉ số", "Doanh thu hợp đồng mới ký" }, { "Giá trị (VND)", contractRevenue }, { "Số lượng giao dịch", contracts.Count } }
            };

            var chartData = new
            {
                labels = new[] { "Thực thu dòng tiền", "Hợp đồng mới ký" },
                datasets = new[]
                {
                    new { label = "Giá trị (VND)", data = new[] { totalCollected, contractRevenue } }
                }
            };

            return new BusinessQueryResultDto(
                request.Query,
                "RevenueAndCashCollection",
                $"Trong tháng {now.Month}/{now.Year}, tổng dòng tiền thực thu đạt {totalCollected:N0} VND qua {paymentCount} giao dịch thanh toán thành công. Tổng giá trị hợp đồng mới ký kết đạt {contractRevenue:N0} VND.",
                $"Tháng {now.Month}/{now.Year}",
                columns,
                rows,
                "bar",
                JsonSerializer.Serialize(chartData),
                "Số liệu được tính toán thời gian thực từ bảng payments và contracts.",
                true
            );
        }

        // Pattern 2: Top Sales Reps ("top sales", "nhân viên xuất sắc", "người bán hàng", "top performer")
        if (query.Contains("top sales") || query.Contains("nhân viên") || query.Contains("người bán") || query.Contains("performer"))
        {
            var startDateOnly = new DateOnly(now.Year, now.Month, 1);
            var wonDeals = await _context.Deals
                .Include(d => d.AssignedEmployee)
                    .ThenInclude(e => e!.User)
                .Where(d => d.Status == DealStatus.Won && d.ActualClosingDate >= startDateOnly)
                .ToListAsync(cancellationToken);

            var repStats = wonDeals
                .GroupBy(d => d.AssignedEmployeeId)
                .Select(g =>
                {
                    var emp = g.First().AssignedEmployee;
                    var name = emp?.User != null ? $"{emp.User.FirstName} {emp.User.LastName}" : emp?.EmployeeCode ?? "Chưa phân công";
                    return new
                    {
                        Name = name,
                        Revenue = g.Sum(d => d.Amount),
                        DealsWon = g.Count()
                    };
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToList();

            var columns = new List<string> { "Xếp hạng", "Nhân viên Sales", "Doanh thu chốt (VND)", "Số Deal Won" };
            var rows = repStats.Select((s, idx) => new Dictionary<string, object>
            {
                { "Xếp hạng", $"#{idx + 1}" },
                { "Nhân viên Sales", s.Name },
                { "Doanh thu chốt (VND)", s.Revenue },
                { "Số Deal Won", s.DealsWon }
            }).ToList();

            var chartData = new
            {
                labels = repStats.Select(s => s.Name).ToArray(),
                datasets = new[]
                {
                    new { label = "Doanh thu (VND)", data = repStats.Select(s => s.Revenue).ToArray() }
                }
            };

            return new BusinessQueryResultDto(
                request.Query,
                "TopSalesPerformers",
                $"Top {repStats.Count} nhân viên bán hàng xuất sắc nhất tháng {now.Month}/{now.Year} dẫn đầu bởi {(repStats.FirstOrDefault()?.Name ?? "N/A")} với doanh số {repStats.FirstOrDefault()?.Revenue:N0} VND.",
                $"Tháng {now.Month}/{now.Year}",
                columns,
                rows,
                "bar",
                JsonSerializer.Serialize(chartData),
                "Số liệu tổng hợp từ các cơ hội chốt thành công (Status = Won) trong tháng.",
                true
            );
        }

        // Pattern 3: Receivables & Overdue Debt ("công nợ", "debt", "quá hạn", "receivables", "chưa thanh toán")
        if (query.Contains("công nợ") || query.Contains("debt") || query.Contains("quá hạn") || query.Contains("receivable"))
        {
            var unpaidSchedules = await _context.PaymentSchedules
                .Include(s => s.Contract)
                .Where(s => s.Status != "Paid")
                .ToListAsync(cancellationToken);

            var todayDate = DateOnly.FromDateTime(now);
            var overdueSchedules = unpaidSchedules.Where(s => s.DueDate < todayDate).ToList();
            var upcomingSchedules = unpaidSchedules.Where(s => s.DueDate >= todayDate).ToList();

            var totalOverdue = overdueSchedules.Sum(s => s.AmountDue - s.AmountPaid);
            var totalUpcoming = upcomingSchedules.Sum(s => s.AmountDue - s.AmountPaid);

            var columns = new List<string> { "Phân loại công nợ", "Số đợt", "Số tiền (VND)" };
            var rows = new List<Dictionary<string, object>>
            {
                new() { { "Phân loại công nợ", "Công nợ quá hạn (Overdue)" }, { "Số đợt", overdueSchedules.Count }, { "Số tiền (VND)", totalOverdue } },
                new() { { "Phân loại công nợ", "Công nợ sắp đến hạn (Upcoming)" }, { "Số đợt", upcomingSchedules.Count }, { "Số tiền (VND)", totalUpcoming } }
            };

            var chartData = new
            {
                labels = new[] { "Công nợ quá hạn", "Sắp đến hạn" },
                datasets = new[]
                {
                    new { label = "Số tiền (VND)", data = new[] { totalOverdue, totalUpcoming } }
                }
            };

            return new BusinessQueryResultDto(
                request.Query,
                "ReceivablesOverview",
                $"Tổng công nợ quá hạn hiện tại là {totalOverdue:N0} VND ({overdueSchedules.Count} đợt thanh toán quá hạn). Công nợ sắp đến hạn trong kỳ là {totalUpcoming:N0} VND.",
                "Thời gian thực",
                columns,
                rows,
                "donut",
                JsonSerializer.Serialize(chartData),
                "Truy vấn trực tiếp từ bảng payment_schedules và invoices.",
                true
            );
        }

        // Pattern 4: Deals / Pipeline ("deal", "cơ hội", "pipeline", "giai đoạn")
        if (query.Contains("deal") || query.Contains("cơ hội") || query.Contains("pipeline"))
        {
            var deals = await _context.Deals
                .Include(d => d.Stage)
                .Where(d => d.Status == DealStatus.Open)
                .ToListAsync(cancellationToken);

            var stageGroups = deals
                .GroupBy(d => d.Stage?.Name ?? "Chưa có stage")
                .Select(g => new
                {
                    StageName = g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(d => d.Amount)
                })
                .OrderByDescending(x => x.TotalValue)
                .ToList();

            var columns = new List<string> { "Giai đoạn Pipeline", "Số lượng Deal", "Tổng giá trị (VND)" };
            var rows = stageGroups.Select(s => new Dictionary<string, object>
            {
                { "Giai đoạn Pipeline", s.StageName },
                { "Số lượng Deal", s.Count },
                { "Tổng giá trị (VND)", s.TotalValue }
            }).ToList();

            var chartData = new
            {
                labels = stageGroups.Select(s => s.StageName).ToArray(),
                datasets = new[]
                {
                    new { label = "Giá trị Pipeline (VND)", data = stageGroups.Select(s => s.TotalValue).ToArray() }
                }
            };

            return new BusinessQueryResultDto(
                request.Query,
                "PipelineOverview",
                $"Pipeline hiện có {deals.Count} cơ hội đang mở với tổng giá trị {deals.Sum(d => d.Amount):N0} VND phân bổ qua {stageGroups.Count} giai đoạn.",
                "Hiện tại",
                columns,
                rows,
                "bar",
                JsonSerializer.Serialize(chartData),
                "Dữ liệu thời gian thực từ bảng deals và pipeline_stages.",
                true
            );
        }

        // Pattern 5: Leads / Funnel ("lead", "khách hàng tiềm năng", "nguồn lead")
        if (query.Contains("lead") || query.Contains("tiềm năng") || query.Contains("nguồn"))
        {
            var leads = await _context.Leads
                .Include(l => l.Source)
                .Where(l => l.CreatedAt >= startOfMonth)
                .ToListAsync(cancellationToken);

            var sourceGroups = leads
                .GroupBy(l => l.Source?.Name ?? "Trực tiếp / Khác")
                .Select(g => new
                {
                    Source = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            var columns = new List<string> { "Nguồn Lead", "Số lượng Lead mới" };
            var rows = sourceGroups.Select(s => new Dictionary<string, object>
            {
                { "Nguồn Lead", s.Source },
                { "Số lượng Lead mới", s.Count }
            }).ToList();

            var chartData = new
            {
                labels = sourceGroups.Select(s => s.Source).ToArray(),
                datasets = new[]
                {
                    new { label = "Số lượng", data = sourceGroups.Select(s => (decimal)s.Count).ToArray() }
                }
            };

            return new BusinessQueryResultDto(
                request.Query,
                "LeadsBySource",
                $"Trong tháng {now.Month}/{now.Year}, hệ thống ghi nhận {leads.Count} khách hàng tiềm năng mới. Nguồn mang lại nhiều lead nhất là {(sourceGroups.FirstOrDefault()?.Source ?? "N/A")}.",
                $"Tháng {now.Month}/{now.Year}",
                columns,
                rows,
                "pie",
                JsonSerializer.Serialize(chartData),
                "Dữ liệu truy vấn trực tiếp từ bảng leads.",
                true
            );
        }

        // Default General CRM Metric Overview
        var openDealsCount = await _context.Deals.CountAsync(d => d.Status == DealStatus.Open, cancellationToken);
        var totalPipeline = await _context.Deals.Where(d => d.Status == DealStatus.Open).SumAsync(d => d.Amount, cancellationToken);
        var newLeadsCount = await _context.Leads.CountAsync(l => l.CreatedAt >= startOfMonth, cancellationToken);
        var activeContractsCount = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active, cancellationToken);

        var generalColumns = new List<string> { "Chỉ số tổng quan", "Giá trị" };
        var generalRows = new List<Dictionary<string, object>>
        {
            new() { { "Chỉ số tổng quan", "Cơ hội đang mở (Open Deals)" }, { "Giá trị", openDealsCount } },
            new() { { "Chỉ số tổng quan", "Tổng giá trị Pipeline" }, { "Giá trị", $"{totalPipeline:N0} VND" } },
            new() { { "Chỉ số tổng quan", "Lead mới trong tháng" }, { "Giá trị", newLeadsCount } },
            new() { { "Chỉ số tổng quan", "Hợp đồng đang thực hiện" }, { "Giá trị", activeContractsCount } }
        };

        return new BusinessQueryResultDto(
            request.Query,
            "GeneralOverview",
            $"Tổng quan hệ thống: Có {openDealsCount} cơ hội bán hàng đang mở với giá trị {totalPipeline:N0} VND, {newLeadsCount} lead mới trong tháng, và {activeContractsCount} hợp đồng đang hiệu lực.",
            "Hiện tại",
            generalColumns,
            generalRows,
            null,
            null,
            "Bạn có thể đặt các câu hỏi cụ thể hơn như: 'Doanh thu tháng này', 'Top sales xuất sắc nhất', 'Công nợ quá hạn', 'Cơ hội theo giai đoạn'.",
            true
        );
    }
}
