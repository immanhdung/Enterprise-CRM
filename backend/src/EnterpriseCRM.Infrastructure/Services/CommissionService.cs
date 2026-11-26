using System.Text.Json;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.PerformanceAndCompensation;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class CommissionService : ICommissionService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICompensationAuditService _auditService;
    private readonly ILogger<CommissionService> _logger;

    public CommissionService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICompensationAuditService auditService,
        ILogger<CommissionService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _logger = logger;
    }

    // ==================== POLICIES ====================

    public async Task<List<CommissionPolicyDto>> GetPoliciesAsync(bool? activeOnly = null, CancellationToken ct = default)
    {
        var query = _context.CommissionPolicies
            .Include(p => p.Service)
            .Include(p => p.Department)
            .Include(p => p.Team)
            .AsNoTracking()
            .AsQueryable();

        if (activeOnly == true)
        {
            query = query.Where(p => p.IsActive);
        }

        var list = await query.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
        return list.Select(MapPolicyToDto).ToList();
    }

    public async Task<CommissionPolicyDto> GetPolicyByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _context.CommissionPolicies
            .Include(x => x.Service)
            .Include(x => x.Department)
            .Include(x => x.Team)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Commission policy '{id}' not found.");

        return MapPolicyToDto(p);
    }

    public async Task<CommissionPolicyDto> CreatePolicyAsync(CreateCommissionPolicyRequest req, CancellationToken ct = default)
    {
        var policy = new CommissionPolicy
        {
            Id = Guid.NewGuid(),
            Name = req.Name.Trim(),
            PolicyType = req.PolicyType,
            CalculationBasis = req.CalculationBasis,
            CustomerType = req.CustomerType,
            ServiceId = req.ServiceId,
            DepartmentId = req.DepartmentId,
            TeamId = req.TeamId,
            MinThreshold = req.MinThreshold,
            MaxThreshold = req.MaxThreshold,
            MaxCap = req.MaxCap,
            RatePercent = req.RatePercent,
            Version = 1,
            Description = req.Description,
            TierRulesJson = req.TierRulesJson,
            EffectiveFrom = req.EffectiveFrom,
            EffectiveTo = req.EffectiveTo,
            IsActive = true
        };

        _context.CommissionPolicies.Add(policy);
        await _context.SaveChangesAsync(ct);

        await _auditService.LogActionAsync("CommissionPolicy", policy.Id, "Create", null, policy.Name, "Created commission policy", ct);

        return await GetPolicyByIdAsync(policy.Id, ct);
    }

    public async Task<CommissionPolicyDto> UpdatePolicyAsync(Guid id, UpdateCommissionPolicyRequest req, CancellationToken ct = default)
    {
        var policy = await _context.CommissionPolicies.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Commission policy '{id}' not found.");

        // Increment version to guarantee audit integrity
        policy.Name = req.Name.Trim();
        policy.PolicyType = req.PolicyType;
        policy.CalculationBasis = req.CalculationBasis;
        policy.CustomerType = req.CustomerType;
        policy.ServiceId = req.ServiceId;
        policy.DepartmentId = req.DepartmentId;
        policy.TeamId = req.TeamId;
        policy.MinThreshold = req.MinThreshold;
        policy.MaxThreshold = req.MaxThreshold;
        policy.MaxCap = req.MaxCap;
        policy.RatePercent = req.RatePercent;
        policy.Version += 1;
        policy.Description = req.Description;
        policy.TierRulesJson = req.TierRulesJson;
        policy.EffectiveFrom = req.EffectiveFrom;
        policy.EffectiveTo = req.EffectiveTo;
        policy.IsActive = req.IsActive;

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("CommissionPolicy", policy.Id, "Update", null, $"v{policy.Version}", "Updated commission policy version", ct);

        return await GetPolicyByIdAsync(policy.Id, ct);
    }

    public async Task<bool> DeletePolicyAsync(Guid id, CancellationToken ct = default)
    {
        var policy = await _context.CommissionPolicies.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Commission policy '{id}' not found.");

        policy.IsDeleted = true;
        policy.DeletedAt = DateTime.UtcNow;
        policy.DeletedBy = _currentUserService.UserId;

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("CommissionPolicy", policy.Id, "Delete", null, null, "Deleted commission policy", ct);
        return true;
    }

    // ==================== CALCULATION ENGINE ====================

    public async Task<List<CommissionCalculationItemDto>> CalculateCommissionsAsync(RunCommissionCalculationRequest req, CancellationToken ct = default)
    {
        var start = new DateTime(req.PeriodYear, req.PeriodMonth, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1).AddTicks(-1);

        // 1. Load active policies
        var policiesQuery = _context.CommissionPolicies.Where(p => p.IsActive);
        if (req.PolicyId.HasValue)
        {
            policiesQuery = policiesQuery.Where(p => p.Id == req.PolicyId.Value);
        }
        var policies = await policiesQuery.ToListAsync(ct);
        if (!policies.Any())
        {
            _logger.LogWarning("No active commission policies found for calculation.");
            return new List<CommissionCalculationItemDto>();
        }

        // 2. Load employees
        var employeesQuery = _context.Employees
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Team)
            .Where(e => e.IsActive);

        if (req.EmployeeId.HasValue)
        {
            employeesQuery = employeesQuery.Where(e => e.Id == req.EmployeeId.Value);
        }
        if (req.DepartmentId.HasValue)
        {
            employeesQuery = employeesQuery.Where(e => e.DepartmentId == req.DepartmentId.Value);
        }
        var employees = await employeesQuery.ToListAsync(ct);

        var generated = new List<Commission>();

        foreach (var emp in employees)
        {
            // Lookup employee KPI score for tiered target policies
            var kpi = await _context.Kpis
                .FirstOrDefaultAsync(k => k.EmployeeId == emp.Id && k.PeriodMonth == req.PeriodMonth && k.PeriodYear == req.PeriodYear, ct);
            var achievementScore = kpi?.AchievementScore ?? 0m;

            foreach (var policy in policies)
            {
                // Department/Team scoping on policy
                if (policy.DepartmentId.HasValue && policy.DepartmentId != emp.DepartmentId) continue;
                if (policy.TeamId.HasValue && policy.TeamId != emp.TeamId) continue;

                if (policy.CalculationBasis == "CollectedCash")
                {
                    // Strict Rule: Only confirmed payments count toward collected cash commission!
                    var payments = await _context.Payments
                        .Include(p => p.Contract)
                        .Where(p => p.Contract.CreatedBy == emp.UserId &&
                                    p.Status == "Confirmed" &&
                                    p.PaymentDate >= start && p.PaymentDate <= end)
                        .ToListAsync(ct);

                    foreach (var payment in payments)
                    {
                        // Duplicate prevention check
                        var exists = await _context.Commissions.AnyAsync(c =>
                            c.EmployeeId == emp.Id &&
                            c.PaymentId == payment.Id &&
                            c.PolicyId == policy.Id &&
                            c.PeriodMonth == req.PeriodMonth &&
                            c.PeriodYear == req.PeriodYear &&
                            c.Status != "Cancelled", ct);

                        if (exists) continue;

                        if (payment.Amount < policy.MinThreshold) continue;

                        var rate = policy.RatePercent;
                        var amount = Math.Round(payment.Amount * (rate / 100m), 2);
                        if (policy.MaxCap.HasValue && amount > policy.MaxCap.Value)
                        {
                            amount = policy.MaxCap.Value;
                        }

                        var comm = new Commission
                        {
                            Id = Guid.NewGuid(),
                            EmployeeId = emp.Id,
                            ContractId = payment.ContractId,
                            PaymentId = payment.Id,
                            PolicyId = policy.Id,
                            PeriodMonth = req.PeriodMonth,
                            PeriodYear = req.PeriodYear,
                            CalculationBasis = policy.CalculationBasis,
                            BaseAmount = payment.Amount,
                            RateApplied = rate,
                            CalculatedAmount = amount,
                            AdjustmentAmount = 0m,
                            NetCommission = amount,
                            Status = "Calculated",
                            Notes = $"Tính 4% trên thanh toán thực thu {payment.TransactionReference} (Hợp đồng: {payment.Contract?.ContractNumber})",
                            PolicyVersion = policy.Version,
                            SourceType = "Payment"
                        };
                        _context.Commissions.Add(comm);
                        generated.Add(comm);
                    }
                }
                else if (policy.CalculationBasis == "ContractValue" || policy.CalculationBasis == "TargetTierBased" || policy.CalculationBasis == "CustomerTypeBased")
                {
                    // Contracts signed in period
                    var contracts = await _context.Contracts
                        .Include(c => c.Account)
                        .Where(c => c.CreatedBy == emp.UserId &&
                                    c.CreatedAt >= start && c.CreatedAt <= end &&
                                    (c.Status == ContractStatus.Signed || c.Status == ContractStatus.Active || c.Status == ContractStatus.Approved))
                        .ToListAsync(ct);

                    foreach (var contract in contracts)
                    {
                        // Customer type filter
                        if (policy.CalculationBasis == "CustomerTypeBased" && policy.CustomerType == "NewCustomer")
                        {
                            // If not new customer, skip
                            if (contract.Account == null || contract.Account.CreatedAt < start.AddMonths(-3))
                            {
                                continue;
                            }
                        }

                        // Duplicate prevention
                        var exists = await _context.Commissions.AnyAsync(c =>
                            c.EmployeeId == emp.Id &&
                            c.ContractId == contract.Id &&
                            c.PolicyId == policy.Id &&
                            c.PeriodMonth == req.PeriodMonth &&
                            c.PeriodYear == req.PeriodYear &&
                            c.Status != "Cancelled", ct);

                        if (exists) continue;

                        if (contract.TotalValue < policy.MinThreshold) continue;

                        var rate = policy.RatePercent;

                        // Tiered Target Achievement Rate calculation
                        if (policy.CalculationBasis == "TargetTierBased" && !string.IsNullOrWhiteSpace(policy.TierRulesJson))
                        {
                            rate = ResolveTierRate(policy.TierRulesJson, achievementScore, policy.RatePercent);
                        }

                        if (rate <= 0m) continue;

                        var amount = Math.Round(contract.TotalValue * (rate / 100m), 2);
                        if (policy.MaxCap.HasValue && amount > policy.MaxCap.Value)
                        {
                            amount = policy.MaxCap.Value;
                        }

                        var comm = new Commission
                        {
                            Id = Guid.NewGuid(),
                            EmployeeId = emp.Id,
                            ContractId = contract.Id,
                            PolicyId = policy.Id,
                            PeriodMonth = req.PeriodMonth,
                            PeriodYear = req.PeriodYear,
                            CalculationBasis = policy.CalculationBasis,
                            BaseAmount = contract.TotalValue,
                            RateApplied = rate,
                            CalculatedAmount = amount,
                            AdjustmentAmount = 0m,
                            NetCommission = amount,
                            Status = "Calculated",
                            Notes = $"Tính hoa hồng theo hợp đồng {contract.ContractNumber} (Tỷ lệ: {rate}%, Điểm KPI: {achievementScore}%)",
                            PolicyVersion = policy.Version,
                            SourceType = "Contract"
                        };
                        _context.Commissions.Add(comm);
                        generated.Add(comm);
                    }
                }
            }
        }

        if (generated.Any())
        {
            await _context.SaveChangesAsync(ct);
            await _auditService.LogActionAsync("Commission", Guid.Empty, "CalculateBatch", null, $"Generated {generated.Count} records", $"Executed commission run for {req.PeriodMonth}/{req.PeriodYear}", ct);
        }

        return await GetCommissionsAsync(req.PeriodMonth, req.PeriodYear, req.EmployeeId, null, ct);
    }

    public async Task<List<CommissionCalculationItemDto>> GetCommissionsAsync(
        int? month,
        int? year,
        Guid? employeeId,
        string? status,
        CancellationToken ct = default)
    {
        var query = _context.Commissions
            .Include(c => c.Employee).ThenInclude(e => e.User)
            .Include(c => c.Policy)
            .Include(c => c.Contract)
            .Include(c => c.Payment)
            .Include(c => c.Deal)
            .AsNoTracking()
            .AsQueryable();

        if (month.HasValue) query = query.Where(c => c.PeriodMonth == month.Value);
        if (year.HasValue) query = query.Where(c => c.PeriodYear == year.Value);
        if (employeeId.HasValue) query = query.Where(c => c.EmployeeId == employeeId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(c => c.Status == status);

        var list = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        return list.Select(MapCommissionToDto).ToList();
    }

    public async Task<CommissionCalculationItemDto> GetCommissionByIdAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _context.Commissions
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.Policy)
            .Include(x => x.Contract)
            .Include(x => x.Payment)
            .Include(x => x.Deal)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Commission record '{id}' not found.");

        return MapCommissionToDto(c);
    }

    public async Task<CommissionCalculationItemDto> ApproveCommissionAsync(Guid id, ApproveCommissionRequest req, CancellationToken ct = default)
    {
        var comm = await _context.Commissions
            .Include(c => c.Employee)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Commission record '{id}' not found.");

        var currentUserId = _currentUserService.UserId;
        if (currentUserId.HasValue && comm.Employee.UserId == currentUserId.Value)
        {
            throw new InvalidOperationException("Anti-Self-Approval violation: You cannot approve your own commission calculation.");
        }

        comm.Status = req.Approved ? "Approved" : "Rejected";
        comm.ApprovedBy = currentUserId;
        comm.ApprovedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(req.Notes))
        {
            comm.Notes = (comm.Notes ?? "") + $"\n[Approval Note]: {req.Notes}";
        }

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("Commission", comm.Id, req.Approved ? "Approve" : "Reject", null, comm.Status, req.Notes ?? "Commission approval decision", ct);

        return await GetCommissionByIdAsync(comm.Id, ct);
    }

    public async Task<CommissionCalculationItemDto> AdjustCommissionAsync(CommissionAdjustmentRequest req, CancellationToken ct = default)
    {
        var comm = await _context.Commissions.FirstOrDefaultAsync(x => x.Id == req.CommissionId, ct)
            ?? throw new KeyNotFoundException($"Commission record '{req.CommissionId}' not found.");

        if (comm.Status == "Paid")
        {
            throw new InvalidOperationException("Cannot adjust commission that has already been disbursed in finalized payroll.");
        }

        var oldNet = comm.NetCommission;
        comm.AdjustmentAmount = req.AdjustmentAmount;
        comm.NetCommission = comm.CalculatedAmount + req.AdjustmentAmount;
        comm.Notes = (comm.Notes ?? "") + $"\n[Manual Adjustment: {req.AdjustmentAmount:N0} VND]: {req.Reason}";

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("Commission", comm.Id, "Adjust", oldNet.ToString(), comm.NetCommission.ToString(), req.Reason, ct);

        return await GetCommissionByIdAsync(comm.Id, ct);
    }

    public async Task<CommissionCalculationItemDto> RecalculateCommissionAsync(Guid id, CancellationToken ct = default)
    {
        var comm = await _context.Commissions
            .Include(c => c.Policy)
            .Include(c => c.Contract)
            .Include(c => c.Payment)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Commission record '{id}' not found.");

        if (comm.Status == "Approved" || comm.Status == "Paid")
        {
            throw new InvalidOperationException("Cannot recalculate an already approved or paid commission.");
        }

        if (comm.Policy != null)
        {
            var rate = comm.Policy.RatePercent;
            comm.CalculatedAmount = Math.Round(comm.BaseAmount * (rate / 100m), 2);
            if (comm.Policy.MaxCap.HasValue && comm.CalculatedAmount > comm.Policy.MaxCap.Value)
            {
                comm.CalculatedAmount = comm.Policy.MaxCap.Value;
            }
            comm.NetCommission = comm.CalculatedAmount + comm.AdjustmentAmount;
            comm.RateApplied = rate;
            comm.PolicyVersion = comm.Policy.Version;
        }

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("Commission", comm.Id, "Recalculate", null, comm.NetCommission.ToString(), "Recalculated commission from active policy", ct);

        return await GetCommissionByIdAsync(comm.Id, ct);
    }

    private static decimal ResolveTierRate(string json, decimal achievementScore, decimal fallbackRate)
    {
        try
        {
            var tiers = JsonSerializer.Deserialize<List<CommissionTierRule>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (tiers != null)
            {
                var match = tiers.FirstOrDefault(t => achievementScore >= t.MinAchievement && (!t.MaxAchievement.HasValue || achievementScore <= t.MaxAchievement.Value));
                if (match != null)
                {
                    return match.Rate;
                }
            }
        }
        catch
        {
            // fallback
        }
        return fallbackRate;
    }

    private static CommissionPolicyDto MapPolicyToDto(CommissionPolicy p)
    {
        return new CommissionPolicyDto(
            p.Id,
            p.Name,
            p.PolicyType,
            p.CalculationBasis,
            p.CustomerType,
            p.ServiceId,
            p.Service?.Name,
            p.DepartmentId,
            p.Department?.Name,
            p.TeamId,
            p.Team?.Name,
            p.MinThreshold,
            p.MaxThreshold,
            p.MaxCap,
            p.RatePercent,
            p.Version,
            p.Description,
            p.TierRulesJson,
            p.EffectiveFrom,
            p.EffectiveTo,
            p.IsActive
        );
    }

    private static CommissionCalculationItemDto MapCommissionToDto(Commission c)
    {
        var empName = c.Employee != null ? $"{c.Employee.User?.FirstName} {c.Employee.User?.LastName}".Trim() : "Unknown";
        return new CommissionCalculationItemDto(
            c.Id,
            c.EmployeeId,
            empName,
            c.Employee?.EmployeeCode ?? "",
            c.PolicyId,
            c.Policy?.Name,
            c.CalculationBasis,
            c.PeriodMonth,
            c.PeriodYear,
            c.DealId,
            c.Deal?.Title,
            c.ContractId,
            c.Contract?.ContractNumber,
            c.PaymentId,
            c.Payment?.TransactionReference,
            c.BaseAmount,
            c.RateApplied,
            c.CalculatedAmount,
            c.AdjustmentAmount,
            c.NetCommission,
            c.Status,
            c.Notes,
            c.PolicyVersion,
            c.SourceType,
            c.ApprovedBy,
            c.ApprovedAt
        );
    }
}
