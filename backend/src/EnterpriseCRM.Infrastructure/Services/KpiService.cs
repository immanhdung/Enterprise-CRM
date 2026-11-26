using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.PerformanceAndCompensation;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseCRM.Infrastructure.Services;

public class KpiService : IKpiService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICompensationAuditService _auditService;
    private readonly ILogger<KpiService> _logger;

    public KpiService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICompensationAuditService auditService,
        ILogger<KpiService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _logger = logger;
    }

    // ==================== DEFINITIONS ====================

    public async Task<List<KpiDefinitionDto>> GetDefinitionsAsync(CancellationToken ct = default)
    {
        return await _context.KpiDefinitions
            .AsNoTracking()
            .OrderBy(d => d.Category)
            .ThenBy(d => d.Name)
            .Select(d => new KpiDefinitionDto(
                d.Id,
                d.Code,
                d.Name,
                d.Category,
                d.MetricType,
                d.Unit,
                d.DefaultTarget,
                d.Description,
                d.IsActive
            ))
            .ToListAsync(ct);
    }

    public async Task<KpiDefinitionDto> GetDefinitionByIdAsync(Guid id, CancellationToken ct = default)
    {
        var d = await _context.KpiDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"KPI Definition '{id}' not found.");

        return new KpiDefinitionDto(
            d.Id,
            d.Code,
            d.Name,
            d.Category,
            d.MetricType,
            d.Unit,
            d.DefaultTarget,
            d.Description,
            d.IsActive
        );
    }

    public async Task<KpiDefinitionDto> CreateDefinitionAsync(CreateKpiDefinitionRequest req, CancellationToken ct = default)
    {
        var existing = await _context.KpiDefinitions.AnyAsync(x => x.Code == req.Code, ct);
        if (existing)
        {
            throw new InvalidOperationException($"KPI Definition with code '{req.Code}' already exists.");
        }

        var def = new KpiDefinition
        {
            Id = Guid.NewGuid(),
            Code = req.Code.Trim().ToUpperInvariant(),
            Name = req.Name.Trim(),
            Category = req.Category,
            MetricType = req.MetricType,
            Unit = req.Unit,
            DefaultTarget = req.DefaultTarget,
            Description = req.Description,
            IsActive = true
        };

        _context.KpiDefinitions.Add(def);
        await _context.SaveChangesAsync(ct);

        await _auditService.LogActionAsync("KpiDefinition", def.Id, "Create", null, def.Code, "Created KPI definition", ct);

        return new KpiDefinitionDto(
            def.Id,
            def.Code,
            def.Name,
            def.Category,
            def.MetricType,
            def.Unit,
            def.DefaultTarget,
            def.Description,
            def.IsActive
        );
    }

    public async Task<KpiDefinitionDto> UpdateDefinitionAsync(Guid id, UpdateKpiDefinitionRequest req, CancellationToken ct = default)
    {
        var def = await _context.KpiDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"KPI Definition '{id}' not found.");

        def.Name = req.Name.Trim();
        def.Category = req.Category;
        def.MetricType = req.MetricType;
        def.Unit = req.Unit;
        def.DefaultTarget = req.DefaultTarget;
        def.Description = req.Description;
        def.IsActive = req.IsActive;

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("KpiDefinition", def.Id, "Update", null, def.Name, "Updated KPI definition", ct);

        return new KpiDefinitionDto(
            def.Id,
            def.Code,
            def.Name,
            def.Category,
            def.MetricType,
            def.Unit,
            def.DefaultTarget,
            def.Description,
            def.IsActive
        );
    }

    public async Task<bool> DeleteDefinitionAsync(Guid id, CancellationToken ct = default)
    {
        var def = await _context.KpiDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"KPI Definition '{id}' not found.");

        def.IsDeleted = true;
        def.DeletedAt = DateTime.UtcNow;
        def.DeletedBy = _currentUserService.UserId;

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("KpiDefinition", def.Id, "Delete", null, null, "Deleted KPI definition", ct);
        return true;
    }

    // ==================== ASSIGNMENTS & EVALUATIONS ====================

    public async Task<List<KpiAssignmentDto>> GetAssignmentsAsync(
        int? month,
        int? year,
        Guid? employeeId,
        Guid? departmentId,
        string? status,
        CancellationToken ct = default)
    {
        var query = _context.Kpis
            .Include(k => k.Employee).ThenInclude(e => e.User)
            .Include(k => k.Definition)
            .Include(k => k.Team)
            .Include(k => k.Department)
            .AsNoTracking()
            .AsQueryable();

        if (month.HasValue) query = query.Where(k => k.PeriodMonth == month.Value);
        if (year.HasValue) query = query.Where(k => k.PeriodYear == year.Value);
        if (employeeId.HasValue) query = query.Where(k => k.EmployeeId == employeeId.Value);
        if (departmentId.HasValue) query = query.Where(k => k.DepartmentId == departmentId.Value || k.Employee.DepartmentId == departmentId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(k => k.Status == status);

        var list = await query
            .OrderByDescending(k => k.PeriodYear)
            .ThenByDescending(k => k.PeriodMonth)
            .ToListAsync(ct);

        return list.Select(MapToDto).ToList();
    }

    public async Task<KpiAssignmentDto> GetAssignmentByIdAsync(Guid id, CancellationToken ct = default)
    {
        var k = await _context.Kpis
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.Definition)
            .Include(x => x.Team)
            .Include(x => x.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"KPI assignment '{id}' not found.");

        return MapToDto(k);
    }

    public async Task<KpiAssignmentDto> CreateAssignmentAsync(CreateKpiAssignmentRequest req, CancellationToken ct = default)
    {
        var emp = await _context.Employees.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == req.EmployeeId, ct)
            ?? throw new KeyNotFoundException($"Employee '{req.EmployeeId}' not found.");

        var kpi = new Kpi
        {
            Id = Guid.NewGuid(),
            EmployeeId = req.EmployeeId,
            DefinitionId = req.DefinitionId,
            TeamId = req.TeamId ?? emp.TeamId,
            DepartmentId = req.DepartmentId ?? emp.DepartmentId,
            Scope = req.Scope,
            PeriodMonth = req.PeriodMonth,
            PeriodYear = req.PeriodYear,
            PeriodQuarter = req.PeriodQuarter,
            TargetRevenue = req.TargetRevenue,
            TargetDealsWon = req.TargetDealsWon,
            TargetNewLeads = req.TargetNewLeads,
            TargetCalls = req.TargetCalls,
            TargetMeetings = req.TargetMeetings,
            TargetTasksCompleted = req.TargetTasksCompleted,
            TargetConversionRate = req.TargetConversionRate,
            WeightPercent = req.WeightPercent,
            Status = "PendingApproval",
            ApprovalNotes = req.Notes
        };

        _context.Kpis.Add(kpi);
        await _context.SaveChangesAsync(ct);

        await _auditService.LogActionAsync("KpiAssignment", kpi.Id, "Create", null, $"Target: {kpi.TargetRevenue}", "Assigned KPI target", ct);

        return await GetAssignmentByIdAsync(kpi.Id, ct);
    }

    public async Task<KpiAssignmentDto> UpdateAssignmentAsync(Guid id, UpdateKpiAssignmentRequest req, CancellationToken ct = default)
    {
        var kpi = await _context.Kpis.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"KPI assignment '{id}' not found.");

        if (kpi.Status == "Approved" || kpi.Status == "Evaluated")
        {
            throw new InvalidOperationException("Cannot modify a KPI assignment that has already been approved or evaluated. Request revision first.");
        }

        kpi.TargetRevenue = req.TargetRevenue;
        kpi.TargetDealsWon = req.TargetDealsWon;
        kpi.TargetNewLeads = req.TargetNewLeads;
        kpi.TargetCalls = req.TargetCalls;
        kpi.TargetMeetings = req.TargetMeetings;
        kpi.TargetTasksCompleted = req.TargetTasksCompleted;
        kpi.TargetConversionRate = req.TargetConversionRate;
        kpi.WeightPercent = req.WeightPercent;
        kpi.ApprovalNotes = req.Notes;

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("KpiAssignment", kpi.Id, "Update", null, $"Target: {kpi.TargetRevenue}", "Updated KPI target", ct);

        return await GetAssignmentByIdAsync(kpi.Id, ct);
    }

    public async Task<KpiAssignmentDto> EvaluateAssignmentAsync(Guid id, EvaluateKpiRequest req, CancellationToken ct = default)
    {
        var kpi = await _context.Kpis.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"KPI assignment '{id}' not found.");

        if (req.AchievedRevenue.HasValue) kpi.AchievedRevenue = req.AchievedRevenue.Value;
        if (req.AchievedDealsWon.HasValue) kpi.AchievedDealsWon = req.AchievedDealsWon.Value;
        if (req.AchievedNewLeads.HasValue) kpi.AchievedNewLeads = req.AchievedNewLeads.Value;
        if (req.AchievedCalls.HasValue) kpi.AchievedCalls = req.AchievedCalls.Value;
        if (req.AchievedMeetings.HasValue) kpi.AchievedMeetings = req.AchievedMeetings.Value;
        if (req.AchievedTasksCompleted.HasValue) kpi.AchievedTasksCompleted = req.AchievedTasksCompleted.Value;
        if (req.AchievedConversionRate.HasValue) kpi.AchievedConversionRate = req.AchievedConversionRate.Value;

        // Calculate score
        kpi.AchievementScore = CalculateOverallScore(kpi);
        kpi.Status = "Evaluated";
        kpi.EvaluatedBy = _currentUserService.UserId;
        kpi.EvaluatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(req.EvaluationNotes))
        {
            kpi.ApprovalNotes = req.EvaluationNotes;
        }

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("KpiAssignment", kpi.Id, "Evaluate", null, $"Score: {kpi.AchievementScore}%", "Evaluated KPI performance", ct);

        return await GetAssignmentByIdAsync(kpi.Id, ct);
    }

    public async Task<KpiAssignmentDto> ApproveAssignmentAsync(Guid id, ApproveKpiRequest req, CancellationToken ct = default)
    {
        var kpi = await _context.Kpis
            .Include(k => k.Employee)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"KPI assignment '{id}' not found.");

        // Anti-self-approval rule: employee cannot approve their own KPI
        var currentUserId = _currentUserService.UserId;
        if (currentUserId.HasValue && kpi.Employee.UserId == currentUserId.Value)
        {
            throw new InvalidOperationException("Anti-Self-Approval violation: You cannot approve your own KPI assignment.");
        }

        kpi.Status = req.Approved ? "Approved" : "Rejected";
        kpi.ApprovedBy = currentUserId;
        kpi.ApprovedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(req.Comments))
        {
            kpi.ApprovalNotes = (kpi.ApprovalNotes ?? "") + $"\n[Approval Note]: {req.Comments}";
        }

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("KpiAssignment", kpi.Id, req.Approved ? "Approve" : "Reject", null, kpi.Status, req.Comments ?? "KPI approval decision", ct);

        return await GetAssignmentByIdAsync(kpi.Id, ct);
    }

    public async Task<KpiAssignmentDto> AutoCalculateActualsAsync(Guid id, CancellationToken ct = default)
    {
        var kpi = await _context.Kpis
            .Include(k => k.Employee)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"KPI assignment '{id}' not found.");

        var startDate = new DateTime(kpi.PeriodYear, kpi.PeriodMonth, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        // 1. Achieved Revenue from signed contracts
        var contractRevenue = await _context.Contracts
            .Where(c => c.CreatedBy == kpi.Employee.UserId &&
                        c.CreatedAt >= startDate && c.CreatedAt <= endDate &&
                        (c.Status == ContractStatus.Signed || c.Status == ContractStatus.Active || c.Status == ContractStatus.Approved))
            .SumAsync(c => (decimal?)c.TotalValue, ct) ?? 0m;

        // 2. Won Deals
        var wonDeals = await _context.Deals
            .Where(d => d.AssignedEmployeeId == kpi.EmployeeId &&
                        d.Status == DealStatus.Won &&
                        d.UpdatedAt >= startDate && d.UpdatedAt <= endDate)
            .CountAsync(ct);

        // 3. New Leads assigned
        var newLeads = await _context.Leads
            .Where(l => l.AssignedEmployeeId == kpi.EmployeeId &&
                        l.CreatedAt >= startDate && l.CreatedAt <= endDate)
            .CountAsync(ct);

        // 4. Calls
        var calls = await _context.LeadActivities
            .Where(a => a.EmployeeId == kpi.EmployeeId &&
                        a.ActivityType == "Call" &&
                        a.CreatedAt >= startDate && a.CreatedAt <= endDate)
            .CountAsync(ct);

        // 5. Meetings
        var meetings = await _context.Appointments
            .Where(m => m.EmployeeId == kpi.EmployeeId &&
                        m.Status == "Completed" &&
                        m.StartTime >= startDate && m.StartTime <= endDate)
            .CountAsync(ct);

        // 6. Tasks Completed
        var tasks = await _context.Tasks
            .Where(t => t.AssignedEmployeeId == kpi.EmployeeId &&
                        t.Status == "Completed" &&
                        t.CompletedAt >= startDate && t.CompletedAt <= endDate)
            .CountAsync(ct);

        // 7. Conversion Rate
        var convRate = newLeads > 0 ? Math.Round(((decimal)wonDeals / newLeads) * 100m, 2) : 0m;

        kpi.AchievedRevenue = contractRevenue;
        kpi.AchievedDealsWon = wonDeals;
        kpi.AchievedNewLeads = newLeads;
        kpi.AchievedCalls = calls;
        kpi.AchievedMeetings = meetings;
        kpi.AchievedTasksCompleted = tasks;
        kpi.AchievedConversionRate = convRate;

        kpi.AchievementScore = CalculateOverallScore(kpi);
        kpi.Status = "Evaluated";
        kpi.EvaluatedBy = _currentUserService.UserId;
        kpi.EvaluatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        await _auditService.LogActionAsync("KpiAssignment", kpi.Id, "AutoCalculate", null, $"Score: {kpi.AchievementScore}%", "Auto-calculated actual metrics from database", ct);

        return await GetAssignmentByIdAsync(kpi.Id, ct);
    }

    private static decimal CalculateOverallScore(Kpi kpi)
    {
        // Compute achievement percentages
        var revPercent = kpi.TargetRevenue > 0 ? (kpi.AchievedRevenue / kpi.TargetRevenue) * 100m : 100m;
        var dealsPercent = kpi.TargetDealsWon > 0 ? ((decimal)kpi.AchievedDealsWon / kpi.TargetDealsWon) * 100m : 100m;
        var leadsPercent = kpi.TargetNewLeads > 0 ? ((decimal)kpi.AchievedNewLeads / kpi.TargetNewLeads) * 100m : 100m;

        // Weighted blended score: Revenue (50%), Deals Won (30%), Leads (20%)
        var blended = (revPercent * 0.50m) + (dealsPercent * 0.30m) + (leadsPercent * 0.20m);
        return Math.Round(blended, 2);
    }

    private static KpiAssignmentDto MapToDto(Kpi k)
    {
        var empName = k.Employee != null ? $"{k.Employee.User?.FirstName} {k.Employee.User?.LastName}".Trim() : "Unknown";
        return new KpiAssignmentDto(
            k.Id,
            k.EmployeeId,
            empName,
            k.Employee?.EmployeeCode ?? "",
            k.DefinitionId,
            k.Definition?.Code,
            k.Definition?.Name,
            k.TeamId,
            k.Team?.Name,
            k.DepartmentId,
            k.Department?.Name,
            k.Scope,
            k.PeriodMonth,
            k.PeriodYear,
            k.PeriodQuarter,
            k.TargetRevenue,
            k.AchievedRevenue,
            k.TargetDealsWon,
            k.AchievedDealsWon,
            k.TargetNewLeads,
            k.AchievedNewLeads,
            k.TargetCalls,
            k.AchievedCalls,
            k.TargetMeetings,
            k.AchievedMeetings,
            k.TargetTasksCompleted,
            k.AchievedTasksCompleted,
            k.TargetConversionRate,
            k.AchievedConversionRate,
            k.WeightPercent,
            k.AchievementScore,
            k.Status,
            k.ApprovalNotes,
            k.EvaluatedBy,
            k.EvaluatedAt,
            k.ApprovedBy,
            k.ApprovedAt
        );
    }
}
