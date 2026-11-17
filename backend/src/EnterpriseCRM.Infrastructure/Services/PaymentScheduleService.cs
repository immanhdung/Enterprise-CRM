using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.Finance;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class PaymentScheduleService : IPaymentScheduleService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<PaymentScheduleService> _logger;

    public PaymentScheduleService(IApplicationDbContext context, ILogger<PaymentScheduleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PaymentScheduleDto>> GetContractSchedulesAsync(Guid contractId, CancellationToken cancellationToken = default)
    {
        var schedules = await _context.PaymentSchedules
            .AsNoTracking()
            .Include(s => s.Contract)
                .ThenInclude(c => c.Account)
            .Where(s => s.ContractId == contractId)
            .OrderBy(s => s.InstallmentNumber)
            .ToListAsync(cancellationToken);

        return schedules.Select(MapToDto).ToList();
    }

    public async Task<PaymentScheduleDto> CreateScheduleAsync(Guid contractId, CreatePaymentScheduleRequest request, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (contract == null)
            throw new InvalidOperationException($"Contract with ID {contractId} was not found.");

        var schedule = new PaymentSchedule
        {
            ContractId = contractId,
            InstallmentNumber = request.InstallmentNumber,
            MilestoneTitle = request.MilestoneTitle,
            Percentage = request.Percentage,
            DueDate = request.DueDate,
            AmountDue = request.Amount,
            AmountPaid = 0,
            Status = "Pending",
            Notes = request.Notes
        };

        _context.PaymentSchedules.Add(schedule);
        await _context.SaveChangesAsync(cancellationToken);

        schedule.Contract = contract;
        return MapToDto(schedule);
    }

    public async Task<IReadOnlyList<PaymentScheduleDto>> GenerateContractSchedulesAsync(Guid contractId, GenerateInstallmentSchedulesRequest request, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (contract == null)
            throw new InvalidOperationException($"Contract with ID {contractId} was not found.");

        if (request.Installments == null || request.Installments.Count == 0)
            throw new InvalidOperationException("Installments list cannot be empty.");

        var totalPercentage = request.Installments.Sum(i => i.Percentage);
        if (Math.Abs(totalPercentage - 100m) > 0.01m)
            throw new InvalidOperationException($"Total percentage must equal 100%. Current sum: {totalPercentage}%");

        // Check if existing schedules have payments
        var existingSchedules = await _context.PaymentSchedules
            .Where(s => s.ContractId == contractId)
            .ToListAsync(cancellationToken);

        var existingPaid = existingSchedules.Any(s => s.AmountPaid > 0);
        if (existingPaid)
        {
            throw new InvalidOperationException("Cannot regenerate payment schedule because payments have already been recorded on existing installments.");
        }

        // Remove old schedules
        if (existingSchedules.Count > 0)
        {
            _context.PaymentSchedules.RemoveRange(existingSchedules);
        }

        var newSchedules = new List<PaymentSchedule>();
        decimal accumulatedAmount = 0m;

        for (int i = 0; i < request.Installments.Count; i++)
        {
            var item = request.Installments[i];
            decimal amount;

            if (i == request.Installments.Count - 1)
            {
                // Last installment takes remaining balance to prevent rounding discrepancies
                amount = contract.TotalValue - accumulatedAmount;
            }
            else
            {
                amount = Math.Round(contract.TotalValue * (item.Percentage / 100m), 2);
                accumulatedAmount += amount;
            }

            var schedule = new PaymentSchedule
            {
                ContractId = contractId,
                InstallmentNumber = i + 1,
                MilestoneTitle = item.MilestoneTitle,
                Percentage = item.Percentage,
                DueDate = item.DueDate,
                AmountDue = amount,
                AmountPaid = 0,
                Status = "Pending",
                Notes = item.Notes
            };

            newSchedules.Add(schedule);
        }

        _context.PaymentSchedules.AddRange(newSchedules);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var s in newSchedules)
        {
            s.Contract = contract;
        }

        return newSchedules.Select(MapToDto).ToList();
    }

    public async Task<bool> DeleteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        var schedule = await _context.PaymentSchedules
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == scheduleId, cancellationToken);

        if (schedule == null)
            return false;

        if (schedule.Payments.Any(p => p.Status == "Confirmed"))
            throw new InvalidOperationException("Cannot delete a payment schedule installment that has confirmed payments.");

        _context.PaymentSchedules.Remove(schedule);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static PaymentScheduleDto MapToDto(PaymentSchedule s)
    {
        DateTime? paidAt = null;
        if (s.Payments != null)
        {
            var lastConfirmed = s.Payments.Where(p => p.Status == "Confirmed").OrderByDescending(p => p.PaymentDate).FirstOrDefault();
            if (lastConfirmed != null)
                paidAt = lastConfirmed.PaymentDate;
        }

        return new PaymentScheduleDto(
            Id: s.Id,
            ContractId: s.ContractId,
            ContractNumber: s.Contract?.ContractNumber,
            AccountId: s.Contract?.AccountId,
            AccountName: s.Contract?.Account?.Name,
            InstallmentNumber: s.InstallmentNumber,
            MilestoneTitle: s.MilestoneTitle,
            Percentage: s.Percentage,
            DueDate: s.DueDate,
            Amount: s.AmountDue,
            PaidAmount: s.AmountPaid,
            Status: s.Status,
            Notes: s.Notes,
            PaidAt: paidAt,
            CreatedAt: s.CreatedAt
        );
    }
}
