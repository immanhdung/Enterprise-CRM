using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.PerformanceAndCompensation;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class BonusService : IBonusService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICompensationAuditService _auditService;

    public BonusService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICompensationAuditService auditService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    // ==================== POLICIES ====================

    public async Task<List<BonusPolicyDto>> GetPoliciesAsync(bool? activeOnly = null, CancellationToken ct = default)
    {
        var query = _context.BonusPolicies.AsNoTracking().AsQueryable();
        if (activeOnly == true)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new BonusPolicyDto(
                p.Id,
                p.Name,
                p.Type,
                p.Period,
                p.TargetThreshold,
                p.BonusAmount,
                p.Description,
                p.IsActive
            ))
            .ToListAsync(ct);
    }

    public async Task<BonusPolicyDto> GetPolicyByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _context.BonusPolicies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Bonus policy '{id}' not found.");

        return new BonusPolicyDto(
            p.Id,
            p.Name,
            p.Type,
            p.Period,
            p.TargetThreshold,
            p.BonusAmount,
            p.Description,
            p.IsActive
        );
    }

    public async Task<BonusPolicyDto> CreatePolicyAsync(CreateBonusPolicyRequest req, CancellationToken ct = default)
    {
        var policy = new BonusPolicy
        {
            Id = Guid.NewGuid(),
            Name = req.Name.Trim(),
            Type = req.Type,
            Period = req.Period,
            TargetThreshold = req.TargetThreshold,
            BonusAmount = req.BonusAmount,
            Description = req.Description,
            IsActive = true
        };

        _context.BonusPolicies.Add(policy);
        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("BonusPolicy", policy.Id, "Create", null, policy.Name, "Created bonus policy", ct);

        return await GetPolicyByIdAsync(policy.Id, ct);
    }

    public async Task<BonusPolicyDto> UpdatePolicyAsync(Guid id, UpdateBonusPolicyRequest req, CancellationToken ct = default)
    {
        var p = await _context.BonusPolicies.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Bonus policy '{id}' not found.");

        p.Name = req.Name.Trim();
        p.Type = req.Type;
        p.Period = req.Period;
        p.TargetThreshold = req.TargetThreshold;
        p.BonusAmount = req.BonusAmount;
        p.Description = req.Description;
        p.IsActive = req.IsActive;

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("BonusPolicy", p.Id, "Update", null, p.Name, "Updated bonus policy", ct);

        return await GetPolicyByIdAsync(p.Id, ct);
    }

    // ==================== RECORDS ====================

    public async Task<List<BonusRecordDto>> GetBonusRecordsAsync(
        int? month,
        int? year,
        Guid? employeeId,
        string? status,
        CancellationToken ct = default)
    {
        var query = _context.BonusRecords
            .Include(b => b.BonusPolicy)
            .Include(b => b.Employee).ThenInclude(e => e.User)
            .AsNoTracking()
            .AsQueryable();

        if (month.HasValue) query = query.Where(b => b.PeriodMonth == month.Value);
        if (year.HasValue) query = query.Where(b => b.PeriodYear == year.Value);
        if (employeeId.HasValue) query = query.Where(b => b.EmployeeId == employeeId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(b => b.Status == status);

        var list = await query
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

        return list.Select(MapRecordToDto).ToList();
    }

    public async Task<BonusRecordDto> GetBonusRecordByIdAsync(Guid id, CancellationToken ct = default)
    {
        var b = await _context.BonusRecords
            .Include(x => x.BonusPolicy)
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Bonus record '{id}' not found.");

        return MapRecordToDto(b);
    }

    public async Task<BonusRecordDto> CreateBonusRecordAsync(CreateBonusRecordRequest req, CancellationToken ct = default)
    {
        var emp = await _context.Employees.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == req.EmployeeId, ct)
            ?? throw new KeyNotFoundException($"Employee '{req.EmployeeId}' not found.");

        var record = new BonusRecord
        {
            Id = Guid.NewGuid(),
            BonusPolicyId = req.BonusPolicyId,
            EmployeeId = req.EmployeeId,
            PeriodMonth = req.PeriodMonth,
            PeriodYear = req.PeriodYear,
            Amount = req.Amount,
            Reason = req.Reason.Trim(),
            Status = "PendingApproval",
            Notes = req.Notes
        };

        _context.BonusRecords.Add(record);
        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("BonusRecord", record.Id, "Create", null, record.Amount.ToString(), $"Granted bonus: {record.Reason}", ct);

        return await GetBonusRecordByIdAsync(record.Id, ct);
    }

    public async Task<BonusRecordDto> ApproveBonusRecordAsync(Guid id, ApproveBonusRecordRequest req, CancellationToken ct = default)
    {
        var b = await _context.BonusRecords
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Bonus record '{id}' not found.");

        var currentUserId = _currentUserService.UserId;
        if (currentUserId.HasValue && b.Employee.UserId == currentUserId.Value)
        {
            throw new InvalidOperationException("Anti-Self-Approval violation: You cannot approve your own bonus reward.");
        }

        b.Status = req.Approved ? "Approved" : "Rejected";
        b.ApprovedBy = currentUserId;
        b.ApprovedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(req.Notes))
        {
            b.Notes = (b.Notes ?? "") + $"\n[Approval Note]: {req.Notes}";
        }

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("BonusRecord", b.Id, req.Approved ? "Approve" : "Reject", null, b.Status, req.Notes ?? "Bonus approval decision", ct);

        return await GetBonusRecordByIdAsync(b.Id, ct);
    }

    private static BonusRecordDto MapRecordToDto(BonusRecord b)
    {
        var empName = b.Employee != null ? $"{b.Employee.User?.FirstName} {b.Employee.User?.LastName}".Trim() : "Unknown";
        return new BonusRecordDto(
            b.Id,
            b.BonusPolicyId,
            b.BonusPolicy?.Name,
            b.EmployeeId,
            empName,
            b.Employee?.EmployeeCode ?? "",
            b.PeriodMonth,
            b.PeriodYear,
            b.Amount,
            b.Reason,
            b.Status,
            b.ApprovedBy,
            b.ApprovedAt,
            b.PayrollRecordId,
            b.Notes
        );
    }
}
