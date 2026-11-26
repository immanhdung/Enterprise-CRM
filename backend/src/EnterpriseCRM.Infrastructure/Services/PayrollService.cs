using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.PerformanceAndCompensation;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class PayrollService : IPayrollService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICompensationAuditService _auditService;
    private readonly ILogger<PayrollService> _logger;

    public PayrollService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICompensationAuditService auditService,
        ILogger<PayrollService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _logger = logger;
    }

    // ==================== PERIODS ====================

    public async Task<List<PayrollPeriodDto>> GetPeriodsAsync(int? year = null, CancellationToken ct = default)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var periods = await _context.PayrollPeriods
            .Include(p => p.Records)
            .AsNoTracking()
            .Where(p => p.Year == targetYear)
            .OrderByDescending(p => p.Month)
            .ToListAsync(ct);

        return periods.Select(MapPeriodToDto).ToList();
    }

    public async Task<PayrollPeriodDto> GetPeriodByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _context.PayrollPeriods
            .Include(x => x.Records)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Payroll period '{id}' not found.");

        return MapPeriodToDto(p);
    }

    public async Task<PayrollPeriodDto> CreatePeriodAsync(CreatePayrollPeriodRequest req, CancellationToken ct = default)
    {
        var exists = await _context.PayrollPeriods.AnyAsync(p => p.Month == req.Month && p.Year == req.Year, ct);
        if (exists)
        {
            throw new InvalidOperationException($"Payroll period for {req.Month}/{req.Year} already exists.");
        }

        var period = new PayrollPeriod
        {
            Id = Guid.NewGuid(),
            Name = req.Name.Trim(),
            Month = req.Month,
            Year = req.Year,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            Status = "Draft"
        };

        _context.PayrollPeriods.Add(period);
        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("PayrollPeriod", period.Id, "Create", null, period.Name, "Created payroll period", ct);

        return await GetPeriodByIdAsync(period.Id, ct);
    }

    public async Task<PayrollPeriodDto> ProcessPeriodPayrollAsync(Guid periodId, CancellationToken ct = default)
    {
        var period = await _context.PayrollPeriods
            .Include(p => p.Records).ThenInclude(r => r.Adjustments)
            .FirstOrDefaultAsync(p => p.Id == periodId, ct)
            ?? throw new KeyNotFoundException($"Payroll period '{periodId}' not found.");

        if (period.Status == "Finalized" || period.Status == "Paid")
        {
            throw new InvalidOperationException("Cannot recalculate payroll for a Finalized or Paid period.");
        }

        var employees = await _context.Employees
            .Include(e => e.User)
            .Where(e => e.IsActive)
            .ToListAsync(ct);

        decimal totalBase = 0m;
        decimal totalComm = 0m;
        decimal totalBonus = 0m;
        decimal totalAllow = 0m;
        decimal totalDed = 0m;
        decimal totalGross = 0m;
        decimal totalNet = 0m;

        foreach (var emp in employees)
        {
            // 1. Commissions in this period
            var approvedComms = await _context.Commissions
                .Where(c => c.EmployeeId == emp.Id &&
                            c.PeriodMonth == period.Month &&
                            c.PeriodYear == period.Year &&
                            c.Status == "Approved")
                .ToListAsync(ct);

            var earnedComms = await _context.Commissions
                .Where(c => c.EmployeeId == emp.Id &&
                            c.PeriodMonth == period.Month &&
                            c.PeriodYear == period.Year &&
                            c.Status != "Cancelled")
                .SumAsync(c => (decimal?)c.NetCommission, ct) ?? 0m;

            var approvedCommTotal = approvedComms.Sum(c => c.NetCommission);

            // 2. Bonuses in this period
            var approvedBonuses = await _context.BonusRecords
                .Where(b => b.EmployeeId == emp.Id &&
                            b.PeriodMonth == period.Month &&
                            b.PeriodYear == period.Year &&
                            b.Status == "Approved")
                .ToListAsync(ct);

            var approvedBonusTotal = approvedBonuses.Sum(b => b.Amount);

            // 3. Find or create record
            var record = period.Records.FirstOrDefault(r => r.EmployeeId == emp.Id);
            if (record == null)
            {
                record = new PayrollRecord
                {
                    Id = Guid.NewGuid(),
                    PayrollPeriodId = period.Id,
                    EmployeeId = emp.Id,
                    BaseSalary = emp.BaseSalary,
                    Status = "PendingReview"
                };
                period.Records.Add(record);
            }

            record.BaseSalary = emp.BaseSalary;
            record.CommissionEarned = earnedComms;
            record.CommissionApproved = approvedCommTotal;
            record.CommissionPayable = approvedCommTotal;
            record.TotalCommission = approvedCommTotal;
            record.Bonuses = approvedBonusTotal;

            // Adjustments
            var adjAllowances = record.Adjustments.Where(a => a.Type == "Allowance").Sum(a => a.Amount);
            var adjDeductions = record.Adjustments.Where(a => a.Type == "Penalty" || a.Type == "Advance" || a.Type == "Deduction" || a.Type == "TaxAdjustment").Sum(a => a.Amount);

            record.Allowances = adjAllowances;
            record.Deductions = adjDeductions;
            record.GrossSalary = record.BaseSalary + record.TotalCommission + record.Bonuses + record.Allowances;
            record.NetSalary = Math.Max(0m, record.GrossSalary - record.Deductions);
            record.Status = "PendingReview";

            // Link commission and bonus records
            foreach (var c in approvedComms)
            {
                c.PayrollRecordId = record.Id;
            }
            foreach (var b in approvedBonuses)
            {
                b.PayrollRecordId = record.Id;
            }

            totalBase += record.BaseSalary;
            totalComm += record.TotalCommission;
            totalBonus += record.Bonuses;
            totalAllow += record.Allowances;
            totalDed += record.Deductions;
            totalGross += record.GrossSalary;
            totalNet += record.NetSalary;
        }

        period.TotalBaseSalary = totalBase;
        period.TotalCommission = totalComm;
        period.TotalBonuses = totalBonus;
        period.TotalAllowances = totalAllow;
        period.TotalDeductions = totalDed;
        period.TotalGross = totalGross;
        period.TotalNet = totalNet;
        period.Status = "Processing";

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("PayrollPeriod", period.Id, "Process", null, $"Gross: {totalGross:N0}, Net: {totalNet:N0}", $"Processed payroll calculations for {period.Name}", ct);

        return await GetPeriodByIdAsync(period.Id, ct);
    }

    public async Task<PayrollPeriodDto> ApprovePeriodAsync(Guid periodId, ApprovePayrollRequest req, CancellationToken ct = default)
    {
        var period = await _context.PayrollPeriods
            .Include(p => p.Records)
            .FirstOrDefaultAsync(p => p.Id == periodId, ct)
            ?? throw new KeyNotFoundException($"Payroll period '{periodId}' not found.");

        if (period.Status == "Finalized")
        {
            throw new InvalidOperationException("Payroll period is already finalized.");
        }

        var currentUserId = _currentUserService.UserId;

        if (req.RoleLevel.Contains("Director", StringComparison.OrdinalIgnoreCase) || req.RoleLevel.Contains("CEO", StringComparison.OrdinalIgnoreCase))
        {
            period.Status = req.Approved ? "ApprovedByDirector" : "Draft";
            foreach (var r in period.Records)
            {
                r.Status = req.Approved ? "ApprovedByDirector" : "PendingReview";
                r.ApprovedBy = currentUserId;
                r.ApprovedAt = DateTime.UtcNow;
            }
        }
        else
        {
            // Manager level
            period.Status = req.Approved ? "ApprovedByManager" : "Draft";
            foreach (var r in period.Records)
            {
                r.Status = req.Approved ? "ApprovedByManager" : "PendingReview";
                r.ApprovedBy = currentUserId;
                r.ApprovedAt = DateTime.UtcNow;
            }
        }

        period.ApprovedBy = currentUserId;
        period.ApprovedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(req.Comments))
        {
            period.ApprovalNotes = (period.ApprovalNotes ?? "") + $"\n[{req.RoleLevel} Approval]: {req.Comments}";
        }

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("PayrollPeriod", period.Id, req.Approved ? "Approve" : "Reject", null, period.Status, req.Comments ?? $"Payroll {req.RoleLevel} approval", ct);

        return await GetPeriodByIdAsync(period.Id, ct);
    }

    public async Task<PayrollPeriodDto> FinalizePeriodAsync(Guid periodId, CancellationToken ct = default)
    {
        var period = await _context.PayrollPeriods
            .Include(p => p.Records)
            .FirstOrDefaultAsync(p => p.Id == periodId, ct)
            ?? throw new KeyNotFoundException($"Payroll period '{periodId}' not found.");

        if (period.Status != "ApprovedByDirector" && period.Status != "ApprovedByManager")
        {
            throw new InvalidOperationException("Payroll must be approved before finalization.");
        }

        period.Status = "Finalized";
        period.FinalizedAt = DateTime.UtcNow;

        // Mark all records finalized and set commissions/bonuses to Paid
        foreach (var r in period.Records)
        {
            r.Status = "Finalized";
            r.CommissionPaid = r.CommissionPayable;
            r.PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        // Mark associated commissions and bonus records as Paid
        var recordIds = period.Records.Select(r => r.Id).ToList();

        var comms = await _context.Commissions.Where(c => c.PayrollRecordId.HasValue && recordIds.Contains(c.PayrollRecordId.Value)).ToListAsync(ct);
        foreach (var c in comms)
        {
            c.Status = "Paid";
        }

        var bonuses = await _context.BonusRecords.Where(b => b.PayrollRecordId.HasValue && recordIds.Contains(b.PayrollRecordId.Value)).ToListAsync(ct);
        foreach (var b in bonuses)
        {
            b.Status = "Paid";
        }

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("PayrollPeriod", period.Id, "Finalize", null, "Finalized", $"Finalized and locked payroll for {period.Name}", ct);

        return await GetPeriodByIdAsync(period.Id, ct);
    }

    // ==================== RECORDS ====================

    public async Task<List<PayrollRecordDto>> GetPayrollRecordsAsync(Guid periodId, Guid? employeeId = null, CancellationToken ct = default)
    {
        var query = _context.PayrollRecords
            .Include(r => r.PayrollPeriod)
            .Include(r => r.Employee).ThenInclude(e => e.User)
            .Include(r => r.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Adjustments)
            .AsNoTracking()
            .Where(r => r.PayrollPeriodId == periodId);

        if (employeeId.HasValue)
        {
            query = query.Where(r => r.EmployeeId == employeeId.Value);
        }

        var list = await query.OrderBy(r => r.Employee.EmployeeCode).ToListAsync(ct);
        return list.Select(MapRecordToDto).ToList();
    }

    public async Task<PayrollRecordDto> GetPayrollRecordByIdAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _context.PayrollRecords
            .Include(x => x.PayrollPeriod)
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.Employee).ThenInclude(e => e.Department)
            .Include(x => x.Adjustments)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Payroll record '{id}' not found.");

        return MapRecordToDto(r);
    }

    public async Task<PayrollRecordDto> AddAdjustmentAsync(AddPayrollAdjustmentRequest req, CancellationToken ct = default)
    {
        var record = await _context.PayrollRecords
            .Include(r => r.Adjustments)
            .Include(r => r.PayrollPeriod)
            .FirstOrDefaultAsync(r => r.Id == req.PayrollRecordId, ct)
            ?? throw new KeyNotFoundException($"Payroll record '{req.PayrollRecordId}' not found.");

        if (record.PayrollPeriod.Status == "Finalized")
        {
            throw new InvalidOperationException("Cannot add adjustment to a Finalized payroll period. Create a post-period adjustment in the next active payroll run.");
        }

        var adj = new PayrollAdjustment
        {
            Id = Guid.NewGuid(),
            PayrollRecordId = record.Id,
            Type = req.Type,
            Amount = req.Amount,
            Reason = req.Reason.Trim(),
            ApprovedBy = _currentUserService.UserId,
            ApprovedAt = DateTime.UtcNow
        };

        record.Adjustments.Add(adj);

        // Recalculate
        var adjAllowances = record.Adjustments.Where(a => a.Type == "Allowance").Sum(a => a.Amount);
        var adjDeductions = record.Adjustments.Where(a => a.Type == "Penalty" || a.Type == "Advance" || a.Type == "Deduction" || a.Type == "TaxAdjustment").Sum(a => a.Amount);

        record.Allowances = adjAllowances;
        record.Deductions = adjDeductions;
        record.GrossSalary = record.BaseSalary + record.TotalCommission + record.Bonuses + record.Allowances;
        record.NetSalary = Math.Max(0m, record.GrossSalary - record.Deductions);

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("PayrollAdjustment", adj.Id, "AddAdjustment", null, $"{req.Type}: {req.Amount:N0}", req.Reason, ct);

        return await GetPayrollRecordByIdAsync(record.Id, ct);
    }

    public async Task<PayrollRecordDto> RecalculateRecordAsync(Guid recordId, CancellationToken ct = default)
    {
        var record = await _context.PayrollRecords
            .Include(r => r.PayrollPeriod)
            .Include(r => r.Adjustments)
            .FirstOrDefaultAsync(r => r.Id == recordId, ct)
            ?? throw new KeyNotFoundException($"Payroll record '{recordId}' not found.");

        if (record.PayrollPeriod.Status == "Finalized")
        {
            throw new InvalidOperationException("Cannot recalculate finalized payroll record.");
        }

        var adjAllowances = record.Adjustments.Where(a => a.Type == "Allowance").Sum(a => a.Amount);
        var adjDeductions = record.Adjustments.Where(a => a.Type == "Penalty" || a.Type == "Advance" || a.Type == "Deduction" || a.Type == "TaxAdjustment").Sum(a => a.Amount);

        record.Allowances = adjAllowances;
        record.Deductions = adjDeductions;
        record.GrossSalary = record.BaseSalary + record.TotalCommission + record.Bonuses + record.Allowances;
        record.NetSalary = Math.Max(0m, record.GrossSalary - record.Deductions);

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("PayrollRecord", record.Id, "Recalculate", null, record.NetSalary.ToString(), "Recalculated employee payroll record", ct);

        return await GetPayrollRecordByIdAsync(record.Id, ct);
    }

    private static PayrollPeriodDto MapPeriodToDto(PayrollPeriod p)
    {
        return new PayrollPeriodDto(
            p.Id,
            p.Name,
            p.Month,
            p.Year,
            p.StartDate,
            p.EndDate,
            p.Status,
            p.TotalBaseSalary,
            p.TotalCommission,
            p.TotalBonuses,
            p.TotalAllowances,
            p.TotalDeductions,
            p.TotalGross,
            p.TotalNet,
            p.ApprovedBy,
            p.ApprovedAt,
            p.FinalizedAt,
            p.ApprovalNotes,
            p.Records.Count
        );
    }

    private static PayrollRecordDto MapRecordToDto(PayrollRecord r)
    {
        var empName = r.Employee != null ? $"{r.Employee.User?.FirstName} {r.Employee.User?.LastName}".Trim() : "Unknown";
        return new PayrollRecordDto(
            r.Id,
            r.PayrollPeriodId,
            r.PayrollPeriod?.Name ?? "",
            r.EmployeeId,
            empName,
            r.Employee?.EmployeeCode ?? "",
            r.Employee?.Department?.Name,
            r.Employee?.Position,
            r.BaseSalary,
            r.TotalCommission,
            r.Bonuses,
            r.Allowances,
            r.Deductions,
            r.GrossSalary,
            r.NetSalary,
            r.CommissionEarned,
            r.CommissionApproved,
            r.CommissionPayable,
            r.CommissionPaid,
            r.Status,
            r.PaymentDate,
            r.ApprovedBy,
            r.ApprovedAt,
            r.Notes,
            r.Adjustments.Select(a => new PayrollAdjustmentDto(
                a.Id,
                a.PayrollRecordId,
                a.Type,
                a.Amount,
                a.Reason,
                a.ApprovedBy,
                a.ApprovedAt
            )).ToList()
        );
    }
}
