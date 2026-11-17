using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.Finance;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class RevenueService : IRevenueService
{
    private readonly IApplicationDbContext _context;

    public RevenueService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueAnalyticsResponse> GetRevenueAnalyticsAsync(RevenueFilterParams filter, CancellationToken cancellationToken = default)
    {
        var fromDate = filter.FromDate ?? DateTime.UtcNow.AddMonths(-12);
        var toDate = filter.ToDate ?? DateTime.UtcNow;

        // Base queries
        var contractsQuery = _context.Contracts
            .AsNoTracking()
            .Include(c => c.Account)
            .Include(c => c.Quotation)
            .Where(c => c.Status != ContractStatus.Draft && c.Status != ContractStatus.Terminated);

        var paymentsQuery = _context.Payments
            .AsNoTracking()
            .Include(p => p.Contract)
            .Where(p => p.Status == "Confirmed");

        var invoicesQuery = _context.Invoices
            .AsNoTracking()
            .Include(i => i.Contract)
            .Include(i => i.Items)
            .Where(i => i.Status != InvoiceStatus.Cancelled);

        // Filters
        if (filter.CustomerId.HasValue)
        {
            contractsQuery = contractsQuery.Where(c => c.AccountId == filter.CustomerId.Value);
            paymentsQuery = paymentsQuery.Where(p => p.AccountId == filter.CustomerId.Value);
            invoicesQuery = invoicesQuery.Where(i => i.AccountId == filter.CustomerId.Value);
        }

        if (filter.ContractId.HasValue)
        {
            contractsQuery = contractsQuery.Where(c => c.Id == filter.ContractId.Value);
            paymentsQuery = paymentsQuery.Where(p => p.ContractId == filter.ContractId.Value);
            invoicesQuery = invoicesQuery.Where(i => i.ContractId == filter.ContractId.Value);
        }

        if (filter.SalesEmployeeId.HasValue)
        {
            contractsQuery = contractsQuery.Where(c => c.CreatedBy == filter.SalesEmployeeId.Value);
            paymentsQuery = paymentsQuery.Where(p => p.Contract.CreatedBy == filter.SalesEmployeeId.Value);
            invoicesQuery = invoicesQuery.Where(i => i.Contract.CreatedBy == filter.SalesEmployeeId.Value);
        }

        var contracts = await contractsQuery.ToListAsync(cancellationToken);
        var payments = await paymentsQuery.ToListAsync(cancellationToken);
        var invoices = await invoicesQuery.ToListAsync(cancellationToken);

        // Overall summary
        var totalContractValue = contracts.Sum(c => c.TotalValue);
        var totalInvoicedAmount = invoices.Sum(i => i.TotalWithTax);
        var totalCollectedCash = payments.Sum(p => p.Amount);
        var totalOutstanding = Math.Max(0m, totalInvoicedAmount - totalCollectedCash);
        if (totalOutstanding == 0 && totalContractValue > totalCollectedCash)
        {
            totalOutstanding = totalContractValue - totalCollectedCash;
        }

        var invoicedPct = totalContractValue > 0 ? Math.Round((totalInvoicedAmount / totalContractValue) * 100m, 2) : 0m;
        var collectionRate = totalInvoicedAmount > 0 ? Math.Round((totalCollectedCash / totalInvoicedAmount) * 100m, 2) : 0m;

        var summary = new RevenueSummaryDto(
            TotalContractValue: totalContractValue,
            TotalInvoicedAmount: totalInvoicedAmount,
            TotalCollectedCash: totalCollectedCash,
            TotalOutstandingReceivables: totalOutstanding,
            InvoicedPercentage: invoicedPct,
            CashCollectionRate: collectionRate
        );

        // Trend by Period
        var trend = GenerateTrend(fromDate, toDate, filter.PeriodType, contracts, invoices, payments);

        // By Customer
        var byCustomer = contracts
            .GroupBy(c => new { c.AccountId, c.Account.Name })
            .Select(g =>
            {
                var custInvoices = invoices.Where(i => i.AccountId == g.Key.AccountId).Sum(i => i.TotalWithTax);
                var custPayments = payments.Where(p => p.AccountId == g.Key.AccountId).Sum(p => p.Amount);
                var custContractVal = g.Sum(c => c.TotalValue);
                return new RevenueByCustomerDto(
                    AccountId: g.Key.AccountId,
                    CustomerName: g.Key.Name,
                    ContractCount: g.Count(),
                    TotalContractValue: custContractVal,
                    InvoicedAmount: custInvoices,
                    CollectedCash: custPayments,
                    OutstandingAmount: Math.Max(0m, custContractVal - custPayments)
                );
            })
            .OrderByDescending(c => c.TotalContractValue)
            .Take(20)
            .ToList();

        // By Contract
        var byContract = contracts
            .Select(c =>
            {
                var cInvoices = invoices.Where(i => i.ContractId == c.Id).Sum(i => i.TotalWithTax);
                var cPayments = payments.Where(p => p.ContractId == c.Id).Sum(p => p.Amount);
                return new RevenueByContractDto(
                    ContractId: c.Id,
                    ContractNumber: c.ContractNumber,
                    CustomerName: c.Account.Name,
                    TotalValue: c.TotalValue,
                    InvoicedAmount: cInvoices,
                    CollectedCash: cPayments,
                    OutstandingAmount: Math.Max(0m, c.TotalValue - cPayments),
                    Status: c.Status.ToString()
                );
            })
            .OrderByDescending(c => c.TotalValue)
            .Take(20)
            .ToList();

        // By Sales
        var employees = await _context.Employees
            .AsNoTracking()
            .Include(e => e.User)
            .ToDictionaryAsync(e => e.UserId, e => e.User.FullName, cancellationToken);

        var bySales = contracts
            .GroupBy(c => c.CreatedBy)
            .Select(g =>
            {
                var empName = "Standard Sales";
                if (g.Key.HasValue && employees.TryGetValue(g.Key.Value, out var name))
                {
                    empName = name;
                }
                var cIds = g.Select(c => c.Id).ToHashSet();
                var salesInvoiced = invoices.Where(i => cIds.Contains(i.ContractId)).Sum(i => i.TotalWithTax);
                var salesPayments = payments.Where(p => cIds.Contains(p.ContractId)).Sum(p => p.Amount);

                return new RevenueBySalesDto(
                    EmployeeId: g.Key ?? Guid.Empty,
                    EmployeeName: empName,
                    ContractCount: g.Count(),
                    TotalContractValue: g.Sum(c => c.TotalValue),
                    InvoicedAmount: salesInvoiced,
                    CollectedCash: salesPayments
                );
            })
            .OrderByDescending(s => s.TotalContractValue)
            .ToList();

        // By Service
        var invoiceItems = invoices.SelectMany(i => i.Items).ToList();
        var byService = invoiceItems
            .GroupBy(item => new { item.ServiceId, item.Description })
            .Select(g => new RevenueByServiceDto(
                ServiceId: g.Key.ServiceId,
                ServiceName: g.Key.Description,
                QuantitySold: (int)g.Sum(x => x.Quantity),
                InvoicedAmount: g.Sum(x => x.LineTotal)
            ))
            .OrderByDescending(s => s.InvoicedAmount)
            .Take(15)
            .ToList();

        return new RevenueAnalyticsResponse(
            Summary: summary,
            TrendByPeriod: trend,
            ByCustomer: byCustomer,
            ByContract: byContract,
            BySales: bySales,
            ByService: byService
        );
    }

    private static List<RevenuePeriodItemDto> GenerateTrend(
        DateTime fromDate,
        DateTime toDate,
        string periodType,
        List<Contract> contracts,
        List<Invoice> invoices,
        List<Payment> payments)
    {
        var periods = new List<RevenuePeriodItemDto>();

        // Default monthly buckets
        var current = new DateTime(fromDate.Year, fromDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endMonth = new DateTime(toDate.Year, toDate.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);

        while (current < endMonth && periods.Count < 24)
        {
            var next = current.AddMonths(1);
            var label = current.ToString("MMM yyyy");

            var currentDateOnly = DateOnly.FromDateTime(current);
            var nextDateOnly = DateOnly.FromDateTime(next);

            var cVal = contracts
                .Where(c => c.CreatedAt >= current && c.CreatedAt < next)
                .Sum(c => c.TotalValue);

            var invVal = invoices
                .Where(i => i.IssueDate >= currentDateOnly && i.IssueDate < nextDateOnly)
                .Sum(i => i.TotalWithTax);

            var payVal = payments
                .Where(p => p.PaymentDate >= current && p.PaymentDate < next)
                .Sum(p => p.Amount);

            periods.Add(new RevenuePeriodItemDto(
                PeriodLabel: label,
                StartDate: current,
                EndDate: next.AddSeconds(-1),
                ContractValue: cVal,
                InvoicedAmount: invVal,
                CollectedCash: payVal,
                OutstandingAmount: Math.Max(0m, invVal - payVal)
            ));

            current = next;
        }

        return periods;
    }
}
