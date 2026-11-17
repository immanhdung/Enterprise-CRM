using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.Finance;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IApplicationDbContext context, ILogger<PaymentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedPaymentResult> GetPaymentsAsync(PaymentFilterParams filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Include(p => p.Contract)
            .Include(p => p.Account)
            .Include(p => p.PaymentSchedule)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.PaymentNumber.ToLower().Contains(term) ||
                                     p.TransactionReference.ToLower().Contains(term) ||
                                     p.SenderName!.ToLower().Contains(term) ||
                                     p.Contract.ContractNumber.ToLower().Contains(term) ||
                                     p.Account.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(p => p.Status == filter.Status);
        }

        if (filter.Method.HasValue)
        {
            query = query.Where(p => p.PaymentMethod == filter.Method.Value);
        }

        if (filter.ContractId.HasValue)
        {
            query = query.Where(p => p.ContractId == filter.ContractId.Value);
        }

        if (filter.AccountId.HasValue)
        {
            query = query.Where(p => p.AccountId == filter.AccountId.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= filter.ToDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortBy?.ToLower() switch
        {
            "amount" => filter.SortDescending ? query.OrderByDescending(p => p.Amount) : query.OrderBy(p => p.Amount),
            "paymentdate" => filter.SortDescending ? query.OrderByDescending(p => p.PaymentDate) : query.OrderBy(p => p.PaymentDate),
            "paymentnumber" => filter.SortDescending ? query.OrderByDescending(p => p.PaymentNumber) : query.OrderBy(p => p.PaymentNumber),
            _ => filter.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapToDto).ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedPaymentResult(dtos, totalCount, pageNumber, pageSize, totalPages);
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Contract)
            .Include(p => p.Account)
            .Include(p => p.PaymentSchedule)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return payment == null ? null : MapToDto(payment);
    }

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

        if (contract == null)
            throw new InvalidOperationException($"Contract with ID {request.ContractId} was not found.");

        var accountId = request.AccountId ?? contract.AccountId;

        // Generate payment number
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var countToday = await _context.Payments
            .CountAsync(p => p.PaymentNumber.StartsWith($"PAY-{today}"), cancellationToken);
        var paymentNumber = $"PAY-{today}-{(countToday + 1):D4}";

        var payment = new Payment
        {
            ContractId = request.ContractId,
            AccountId = accountId,
            PaymentScheduleId = request.PaymentScheduleId,
            PaymentNumber = paymentNumber,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate == default ? DateTime.UtcNow : request.PaymentDate,
            PaymentMethod = request.Method,
            TransactionReference = request.TransactionReference ?? string.Empty,
            BankName = request.BankName,
            BankAccountNumber = request.BankAccountNumber,
            SenderName = request.SenderName,
            Notes = request.Notes,
            AttachmentUrl = request.AttachmentUrl,
            Status = "Pending",
            IsVerified = false
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        payment.Contract = contract;
        payment.Account = contract.Account;
        if (request.PaymentScheduleId.HasValue)
        {
            payment.PaymentSchedule = await _context.PaymentSchedules.FindAsync([request.PaymentScheduleId.Value], cancellationToken);
        }

        _logger.LogInformation("Payment {PaymentNumber} created by user {UserName} for Contract {ContractNumber}, Amount: {Amount}",
            paymentNumber, currentUserName, contract.ContractNumber, request.Amount);

        return MapToDto(payment);
    }

    public async Task<PaymentDto> ConfirmPaymentAsync(Guid id, ConfirmPaymentRequest request, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .Include(p => p.Contract)
                .ThenInclude(c => c.Account)
            .Include(p => p.PaymentSchedule)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (payment == null)
            throw new InvalidOperationException($"Payment with ID {id} was not found.");

        if (payment.Status == "Confirmed")
            throw new InvalidOperationException("Payment is already confirmed.");

        if (payment.Status == "Reversed" || payment.Status == "Cancelled")
            throw new InvalidOperationException($"Cannot confirm a payment with status '{payment.Status}'.");

        payment.Status = "Confirmed";
        payment.IsVerified = true;
        payment.VerifiedBy = currentUserId;
        payment.ConfirmedAt = DateTime.UtcNow;
        payment.ConfirmedById = currentUserId;
        payment.ConfirmedByName = currentUserName;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            payment.Notes = string.IsNullOrWhiteSpace(payment.Notes)
                ? $"Confirmation notes: {request.Notes}"
                : $"{payment.Notes}\nConfirmation notes: {request.Notes}";
        }

        // Update payment schedule if linked
        if (payment.PaymentSchedule != null)
        {
            payment.PaymentSchedule.AmountPaid += payment.Amount;
            if (payment.PaymentSchedule.AmountPaid >= payment.PaymentSchedule.AmountDue)
            {
                payment.PaymentSchedule.Status = "Paid";
            }
            else if (payment.PaymentSchedule.AmountPaid > 0)
            {
                payment.PaymentSchedule.Status = "PartiallyPaid";
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment {PaymentNumber} confirmed by {UserName}", payment.PaymentNumber, currentUserName);

        return MapToDto(payment);
    }

    public async Task<PaymentDto> ReversePaymentAsync(Guid id, ReversePaymentRequest request, Guid currentUserId, string currentUserName, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .Include(p => p.Contract)
                .ThenInclude(c => c.Account)
            .Include(p => p.PaymentSchedule)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (payment == null)
            throw new InvalidOperationException($"Payment with ID {id} was not found.");

        if (payment.Status != "Confirmed")
            throw new InvalidOperationException("Only confirmed payments can be reversed.");

        payment.Status = "Reversed";
        payment.CancelledAt = DateTime.UtcNow;
        payment.CancelledById = currentUserId;
        payment.ReversalReason = request.ReversalReason;

        // Deduct from linked schedule
        if (payment.PaymentSchedule != null)
        {
            payment.PaymentSchedule.AmountPaid = Math.Max(0m, payment.PaymentSchedule.AmountPaid - payment.Amount);
            if (payment.PaymentSchedule.AmountPaid == 0)
            {
                payment.PaymentSchedule.Status = payment.PaymentSchedule.DueDate < DateOnly.FromDateTime(DateTime.UtcNow) ? "Overdue" : "Pending";
            }
            else if (payment.PaymentSchedule.AmountPaid < payment.PaymentSchedule.AmountDue)
            {
                payment.PaymentSchedule.Status = "PartiallyPaid";
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Payment {PaymentNumber} was REVERSED by {UserName}. Reason: {Reason}",
            payment.PaymentNumber, currentUserName, request.ReversalReason);

        return MapToDto(payment);
    }

    public async Task<ContractFinancialSummaryDto> GetContractFinancialSummaryAsync(Guid contractId, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .AsNoTracking()
            .Include(c => c.Account)
            .Include(c => c.PaymentSchedules)
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (contract == null)
            throw new InvalidOperationException($"Contract with ID {contractId} was not found.");

        var confirmedPayments = contract.Payments.Where(p => p.Status == "Confirmed").ToList();
        var totalPaid = confirmedPayments.Sum(p => p.Amount);
        var outstanding = Math.Max(0m, contract.TotalValue - totalPaid);

        var schedules = contract.PaymentSchedules.OrderBy(s => s.InstallmentNumber).ToList();
        var paidSchedulesCount = schedules.Count(s => s.Status == "Paid");
        var overdueSchedulesCount = schedules.Count(s => s.Status != "Paid" && s.DueDate < DateOnly.FromDateTime(DateTime.UtcNow));

        var scheduleDtos = schedules.Select(s => new PaymentScheduleDto(
            s.Id,
            s.ContractId,
            contract.ContractNumber,
            contract.AccountId,
            contract.Account.Name,
            s.InstallmentNumber,
            s.MilestoneTitle,
            s.Percentage,
            s.DueDate,
            s.AmountDue,
            s.AmountPaid,
            s.Status,
            s.Notes,
            null,
            s.CreatedAt
        )).ToList();

        var paymentDtos = contract.Payments.OrderByDescending(p => p.PaymentDate).Select(MapToDto).ToList();

        return new ContractFinancialSummaryDto(
            ContractId: contract.Id,
            ContractNumber: contract.ContractNumber,
            ContractTitle: contract.Title,
            AccountId: contract.AccountId,
            AccountName: contract.Account.Name,
            TotalValue: contract.TotalValue,
            PaidAmount: totalPaid,
            OutstandingAmount: outstanding,
            RemainingContractValue: outstanding,
            TotalSchedulesCount: schedules.Count,
            PaidSchedulesCount: paidSchedulesCount,
            OverdueSchedulesCount: overdueSchedulesCount,
            Schedules: scheduleDtos,
            Payments: paymentDtos
        );
    }

    private static PaymentDto MapToDto(Payment p)
    {
        return new PaymentDto(
            Id: p.Id,
            PaymentNumber: p.PaymentNumber,
            ContractId: p.ContractId,
            ContractNumber: p.Contract?.ContractNumber,
            AccountId: p.AccountId,
            AccountName: p.Account?.Name,
            PaymentScheduleId: p.PaymentScheduleId,
            MilestoneTitle: p.PaymentSchedule?.MilestoneTitle,
            Amount: p.Amount,
            PaymentDate: p.PaymentDate,
            Method: p.PaymentMethod,
            TransactionReference: p.TransactionReference,
            BankName: p.BankName,
            BankAccountNumber: p.BankAccountNumber,
            SenderName: p.SenderName,
            Notes: p.Notes,
            AttachmentUrl: p.AttachmentUrl,
            Status: p.Status,
            ConfirmedAt: p.ConfirmedAt,
            ConfirmedById: p.ConfirmedById,
            ConfirmedByName: p.ConfirmedByName,
            CancelledAt: p.CancelledAt,
            CancelledById: p.CancelledById,
            ReversalReason: p.ReversalReason,
            CreatedAt: p.CreatedAt
        );
    }
}
