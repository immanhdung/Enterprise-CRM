using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.Finance;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class FinancialReconciliationService : IFinancialReconciliationService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<FinancialReconciliationService> _logger;

    public FinancialReconciliationService(
        IApplicationDbContext context,
        ILogger<FinancialReconciliationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FinancialReconciliationRecordDto>> GetReconciliationHistoryAsync(CancellationToken cancellationToken = default)
    {
        var records = await _context.FinancialReconciliationRecords
            .AsNoTracking()
            .Include(r => r.Items)
            .OrderByDescending(r => r.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        return records.Select(MapToDto).ToList();
    }

    public async Task<FinancialReconciliationRecordDto?> GetReconciliationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _context.FinancialReconciliationRecords
            .AsNoTracking()
            .Include(r => r.Items)
                .ThenInclude(i => i.Contract)
            .Include(r => r.Items)
                .ThenInclude(i => i.Payment)
            .Include(r => r.Items)
                .ThenInclude(i => i.Invoice)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        return record == null ? null : MapToDto(record);
    }

    public async Task<FinancialReconciliationRecordDto> RunReconciliationAsync(
        RunReconciliationRequest request,
        Guid currentUserId,
        string currentUserName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Running financial reconciliation for period {Start} to {End} by {UserName}",
            request.PeriodStart, request.PeriodEnd, currentUserName);

        var startDateTime = request.PeriodStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endDateTime = request.PeriodEnd.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var contracts = await _context.Contracts
            .AsNoTracking()
            .Include(c => c.PaymentSchedules)
            .Where(c => c.StartDate >= request.PeriodStart && c.StartDate <= request.PeriodEnd)
            .ToListAsync(cancellationToken);

        var payments = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Contract)
            .Where(p => p.PaymentDate >= startDateTime && p.PaymentDate <= endDateTime)
            .ToListAsync(cancellationToken);

        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Contract)
            .Where(i => i.IssueDate >= request.PeriodStart && i.IssueDate <= request.PeriodEnd)
            .ToListAsync(cancellationToken);

        var totalContractValue = contracts.Sum(c => c.TotalValue);
        var totalInvoiced = invoices.Where(i => i.Status != InvoiceStatus.Cancelled).Sum(i => i.TotalWithTax);
        var confirmedPayments = payments.Where(p => p.Status == "Confirmed").ToList();
        var totalPayments = confirmedPayments.Sum(p => p.Amount);

        var discrepancyItems = new List<ReconciliationItem>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // 1. Detect Invoices overdue with Missing Payment
        foreach (var inv in invoices.Where(i => i.Status != InvoiceStatus.Cancelled && i.IssueDate.AddDays(30) < today))
        {
            var matchingPayments = confirmedPayments
                .Where(p => p.ContractId == inv.ContractId && Math.Abs(p.Amount - inv.TotalWithTax) < 1m)
                .ToList();

            if (matchingPayments.Count == 0)
            {
                discrepancyItems.Add(new ReconciliationItem
                {
                    ContractId = inv.ContractId,
                    InvoiceId = inv.Id,
                    DiscrepancyType = "MissingPayment",
                    ExpectedAmount = inv.TotalWithTax,
                    ActualAmount = 0m,
                    Difference = inv.TotalWithTax,
                    ResolutionStatus = "Pending"
                });
            }
        }

        // 2. Detect Unmatched Invoices (Confirmed payments with no corresponding invoice)
        foreach (var pay in confirmedPayments)
        {
            var hasInvoice = invoices.Any(i => i.ContractId == pay.ContractId && Math.Abs(i.TotalWithTax - pay.Amount) < 1m);
            if (!hasInvoice)
            {
                discrepancyItems.Add(new ReconciliationItem
                {
                    ContractId = pay.ContractId,
                    PaymentId = pay.Id,
                    DiscrepancyType = "UnmatchedInvoice",
                    ExpectedAmount = pay.Amount,
                    ActualAmount = 0m,
                    Difference = pay.Amount,
                    ResolutionStatus = "Pending"
                });
            }
        }

        // 3. Detect Duplicate Transactions
        var duplicateGroups = payments
            .Where(p => !string.IsNullOrWhiteSpace(p.TransactionReference))
            .GroupBy(p => p.TransactionReference)
            .Where(g => g.Count() > 1);

        foreach (var grp in duplicateGroups)
        {
            foreach (var duplicatePay in grp.Skip(1))
            {
                discrepancyItems.Add(new ReconciliationItem
                {
                    ContractId = duplicatePay.ContractId,
                    PaymentId = duplicatePay.Id,
                    DiscrepancyType = "DuplicateTransaction",
                    ExpectedAmount = duplicatePay.Amount,
                    ActualAmount = duplicatePay.Amount * 2,
                    Difference = duplicatePay.Amount,
                    ResolutionStatus = "Pending"
                });
            }
        }

        decimal totalDiscrepancy = discrepancyItems.Sum(d => d.Difference);
        string status = discrepancyItems.Count == 0 ? "Balanced" : "DiscrepancyDetected";

        var record = new FinancialReconciliationRecord
        {
            Period = $"{request.PeriodStart:yyyy-MM}",
            ReconciliationDate = today,
            TotalContractValue = totalContractValue,
            TotalExpectedRevenue = totalInvoiced,
            TotalCollectedRevenue = totalPayments,
            TotalInvoicedRevenue = totalInvoiced,
            DiscrepancyAmount = totalDiscrepancy,
            Status = status,
            ReconciledById = currentUserId,
            ReconciledByName = currentUserName,
            Notes = request.Notes,
            Items = discrepancyItems
        };

        _context.FinancialReconciliationRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reconciliation completed. Found {Count} discrepancies totaling {Amount}",
            discrepancyItems.Count, totalDiscrepancy);

        return MapToDto(record);
    }

    public async Task<ReconciliationItemDto> ResolveDiscrepancyAsync(
        Guid itemId,
        ResolveDiscrepancyRequest request,
        Guid currentUserId,
        string currentUserName,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.ReconciliationItems
            .Include(i => i.Contract)
            .Include(i => i.Payment)
            .Include(i => i.Invoice)
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);

        if (item == null)
            throw new InvalidOperationException($"Reconciliation item with ID {itemId} was not found.");

        item.ResolutionStatus = request.Action == "ConfirmAdjustment" ? "Confirmed" : "Dismissed";
        item.ResolutionNotes = request.ResolutionNotes;

        var parentRecord = await _context.FinancialReconciliationRecords
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == item.ReconciliationRecordId, cancellationToken);

        if (parentRecord != null && parentRecord.Items.All(i => i.ResolutionStatus != "Pending"))
        {
            parentRecord.Status = "Reconciled";
            parentRecord.ApprovedById = currentUserId;
            parentRecord.ApprovedByName = currentUserName;
            parentRecord.ApprovedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reconciliation item {Id} resolved as {Status} by {UserName}",
            item.Id, item.ResolutionStatus, currentUserName);

        return new ReconciliationItemDto(
            Id: item.Id,
            ReconciliationRecordId: item.ReconciliationRecordId,
            ContractId: item.ContractId,
            ContractNumber: item.Contract?.ContractNumber,
            PaymentId: item.PaymentId,
            PaymentNumber: item.Payment?.PaymentNumber,
            InvoiceId: item.InvoiceId,
            InvoiceNumber: item.Invoice?.InvoiceNumber,
            DiscrepancyType: item.DiscrepancyType,
            ExpectedAmount: item.ExpectedAmount,
            ActualAmount: item.ActualAmount,
            Difference: item.Difference,
            ResolutionStatus: item.ResolutionStatus,
            ResolutionNotes: item.ResolutionNotes
        );
    }

    private static FinancialReconciliationRecordDto MapToDto(FinancialReconciliationRecord r)
    {
        var items = r.Items?.Select(item => new ReconciliationItemDto(
            Id: item.Id,
            ReconciliationRecordId: item.ReconciliationRecordId,
            ContractId: item.ContractId,
            ContractNumber: item.Contract?.ContractNumber,
            PaymentId: item.PaymentId,
            PaymentNumber: item.Payment?.PaymentNumber,
            InvoiceId: item.InvoiceId,
            InvoiceNumber: item.Invoice?.InvoiceNumber,
            DiscrepancyType: item.DiscrepancyType,
            ExpectedAmount: item.ExpectedAmount,
            ActualAmount: item.ActualAmount,
            Difference: item.Difference,
            ResolutionStatus: item.ResolutionStatus,
            ResolutionNotes: item.ResolutionNotes
        )).ToList();

        return new FinancialReconciliationRecordDto(
            Id: r.Id,
            Period: r.Period,
            ReconciliationDate: r.ReconciliationDate,
            TotalContractValue: r.TotalContractValue,
            TotalExpectedRevenue: r.TotalExpectedRevenue,
            TotalCollectedRevenue: r.TotalCollectedRevenue,
            TotalInvoicedRevenue: r.TotalInvoicedRevenue,
            DiscrepancyAmount: r.DiscrepancyAmount,
            Status: r.Status,
            Notes: r.Notes,
            ReconciledById: r.ReconciledById,
            ReconciledByName: r.ReconciledByName,
            ApprovedById: r.ApprovedById,
            ApprovedByName: r.ApprovedByName,
            ApprovedAt: r.ApprovedAt,
            CreatedAt: r.CreatedAt,
            Items: items
        );
    }
}
