using System.Text.Json;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Entities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class LeadAssignmentEngine : ILeadAssignmentEngine
{
    private readonly IApplicationDbContext _context;

    public LeadAssignmentEngine(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeadAssignmentRuleDto>> GetRulesAsync(CancellationToken cancellationToken = default)
    {
        var rules = await _context.LeadAssignmentRules
            .AsNoTracking()
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);

        return rules.Select(r => new LeadAssignmentRuleDto(
            r.Id,
            r.Name,
            r.Description,
            r.Priority,
            r.Mode,
            r.ConditionsJson,
            r.TargetEmployeeIdsJson,
            r.TargetTeamIdsJson,
            r.MaxActiveLeadsPerRep,
            r.IsActive
        )).ToList();
    }

    public async Task<LeadAssignmentRuleDto> CreateRuleAsync(CreateLeadAssignmentRuleRequest request, CancellationToken cancellationToken = default)
    {
        var rule = new LeadAssignmentRule
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Priority = request.Priority,
            Mode = request.Mode,
            ConditionsJson = request.ConditionsJson,
            TargetEmployeeIdsJson = request.TargetEmployeeIdsJson,
            TargetTeamIdsJson = request.TargetTeamIdsJson,
            MaxActiveLeadsPerRep = request.MaxActiveLeadsPerRep,
            IsActive = request.IsActive
        };

        _context.LeadAssignmentRules.Add(rule);
        await _context.SaveChangesAsync(cancellationToken);

        return new LeadAssignmentRuleDto(
            rule.Id,
            rule.Name,
            rule.Description,
            rule.Priority,
            rule.Mode,
            rule.ConditionsJson,
            rule.TargetEmployeeIdsJson,
            rule.TargetTeamIdsJson,
            rule.MaxActiveLeadsPerRep,
            rule.IsActive
        );
    }

    public async Task<LeadAssignmentRuleDto> UpdateRuleAsync(Guid id, CreateLeadAssignmentRuleRequest request, CancellationToken cancellationToken = default)
    {
        var rule = await _context.LeadAssignmentRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (rule == null)
            throw new KeyNotFoundException($"Assignment rule {id} not found.");

        rule.Name = request.Name.Trim();
        rule.Description = request.Description?.Trim() ?? string.Empty;
        rule.Priority = request.Priority;
        rule.Mode = request.Mode;
        rule.ConditionsJson = request.ConditionsJson;
        rule.TargetEmployeeIdsJson = request.TargetEmployeeIdsJson;
        rule.TargetTeamIdsJson = request.TargetTeamIdsJson;
        rule.MaxActiveLeadsPerRep = request.MaxActiveLeadsPerRep;
        rule.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return new LeadAssignmentRuleDto(
            rule.Id,
            rule.Name,
            rule.Description,
            rule.Priority,
            rule.Mode,
            rule.ConditionsJson,
            rule.TargetEmployeeIdsJson,
            rule.TargetTeamIdsJson,
            rule.MaxActiveLeadsPerRep,
            rule.IsActive
        );
    }

    public async Task<bool> DeleteRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _context.LeadAssignmentRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (rule == null) return false;

        rule.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<LeadAssignmentLogDto> AssignLeadAsync(AssignLeadRequest request, CancellationToken cancellationToken = default)
    {
        var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == request.LeadId, cancellationToken);
        if (lead == null)
            throw new KeyNotFoundException($"Lead {request.LeadId} not found.");

        var previousEmpId = lead.AssignedEmployeeId;
        Guid? assignedEmpId = request.SpecificEmployeeId;
        Guid? matchedRuleId = null;
        string? matchedRuleName = null;
        string assignmentMode = request.Mode ?? "Manual";
        string reason = request.Reason ?? "Manual assignment";

        // If manual specific assignment
        if (assignedEmpId.HasValue)
        {
            assignmentMode = "Manual";
            reason = string.IsNullOrWhiteSpace(request.Reason) ? "Manually assigned by manager" : request.Reason;
        }
        else
        {
            // Evaluate active rules
            var activeRules = await _context.LeadAssignmentRules
                .Where(r => r.IsActive)
                .OrderBy(r => r.Priority)
                .ToListAsync(cancellationToken);

            var activeEmployees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.User)
                .Where(e => e.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var rule in activeRules)
            {
                var candidateEmployees = FilterCandidateEmployees(activeEmployees, rule);
                if (!candidateEmployees.Any()) continue;

                if (rule.Mode == "Workload")
                {
                    // Pick employee with minimum active leads count
                    var employeeLeadCounts = await _context.Leads
                        .Where(l => l.AssignedEmployeeId != null && l.Status != LeadStatus.Converted && l.Status != LeadStatus.Unqualified)
                        .GroupBy(l => l.AssignedEmployeeId!.Value)
                        .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.EmployeeId, x => x.Count, cancellationToken);

                    var selected = candidateEmployees
                        .Select(e => new
                        {
                            Employee = e,
                            Count = employeeLeadCounts.TryGetValue(e.Id, out var c) ? c : 0
                        })
                        .Where(x => !rule.MaxActiveLeadsPerRep.HasValue || x.Count < rule.MaxActiveLeadsPerRep.Value)
                        .OrderBy(x => x.Count)
                        .FirstOrDefault();

                    if (selected != null)
                    {
                        assignedEmpId = selected.Employee.Id;
                        matchedRuleId = rule.Id;
                        matchedRuleName = rule.Name;
                        assignmentMode = "Workload";
                        reason = $"Assigned via Workload Rule '{rule.Name}' (Active leads: {selected.Count})";
                        break;
                    }
                }
                else // RoundRobin or default
                {
                    // Find last assigned employee under this rule or mode
                    var lastLog = await _context.LeadAssignmentLogs
                        .Where(l => l.RuleId == rule.Id)
                        .OrderByDescending(l => l.Timestamp)
                        .FirstOrDefaultAsync(cancellationToken);

                    int nextIdx = 0;
                    if (lastLog?.AssignedEmployeeId != null)
                    {
                        var lastIdx = candidateEmployees.FindIndex(e => e.Id == lastLog.AssignedEmployeeId.Value);
                        if (lastIdx >= 0)
                        {
                            nextIdx = (lastIdx + 1) % candidateEmployees.Count;
                        }
                    }

                    var selected = candidateEmployees[nextIdx];
                    assignedEmpId = selected.Id;
                    matchedRuleId = rule.Id;
                    matchedRuleName = rule.Name;
                    assignmentMode = rule.Mode;
                    reason = $"Assigned via {rule.Mode} Rule '{rule.Name}'";
                    break;
                }
            }

            // Fallback if no rule matched
            if (!assignedEmpId.HasValue && activeEmployees.Any())
            {
                var fallback = activeEmployees.First();
                assignedEmpId = fallback.Id;
                assignmentMode = "Fallback";
                reason = "Default fallback to first active sales representative";
            }
        }

        lead.AssignedEmployeeId = assignedEmpId;
        lead.UpdatedAt = DateTime.UtcNow;

        var assignedEmployee = assignedEmpId.HasValue
            ? await _context.Employees.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == assignedEmpId.Value, cancellationToken)
            : null;

        var previousEmployee = previousEmpId.HasValue
            ? await _context.Employees.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == previousEmpId.Value, cancellationToken)
            : null;

        var log = new LeadAssignmentLog
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            RuleId = matchedRuleId,
            PreviousEmployeeId = previousEmpId,
            AssignedEmployeeId = assignedEmpId,
            Mode = assignmentMode,
            Reason = reason,
            Timestamp = DateTime.UtcNow
        };

        _context.LeadAssignmentLogs.Add(log);

        // Notify assigned employee if user exists
        if (assignedEmployee?.UserId != null)
        {
            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = assignedEmployee.UserId,
                Title = "New Lead Assigned",
                Message = $"Lead '{lead.FullName}' ({lead.CompanyName ?? "Individual"}) has been assigned to you. Mode: {assignmentMode}.",
                Type = "LeadAssignment",
                Category = "Lead",
                Priority = "High",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new LeadAssignmentLogDto(
            log.Id,
            lead.Id,
            lead.FullName,
            matchedRuleId,
            matchedRuleName,
            previousEmpId,
            previousEmployee?.User?.FullName ?? previousEmployee?.EmployeeCode,
            assignedEmpId,
            assignedEmployee?.User?.FullName ?? assignedEmployee?.EmployeeCode,
            assignmentMode,
            reason,
            null,
            log.Timestamp
        );
    }

    public async Task<List<LeadAssignmentLogDto>> GetAssignmentLogsAsync(Guid? leadId = null, int limit = 50, CancellationToken cancellationToken = default)
    {
        var query = _context.LeadAssignmentLogs
            .Include(l => l.Lead)
            .Include(l => l.Rule)
            .Include(l => l.AssignedEmployee)
                .ThenInclude(e => e!.User)
            .AsNoTracking()
            .AsQueryable();

        if (leadId.HasValue)
        {
            query = query.Where(l => l.LeadId == leadId.Value);
        }

        var logs = await query.OrderByDescending(l => l.Timestamp).Take(limit).ToListAsync(cancellationToken);

        return logs.Select(l => new LeadAssignmentLogDto(
            l.Id,
            l.LeadId,
            l.Lead?.FullName ?? "Unknown Lead",
            l.RuleId,
            l.Rule?.Name,
            l.PreviousEmployeeId,
            null,
            l.AssignedEmployeeId,
            l.AssignedEmployee?.User?.FullName ?? l.AssignedEmployee?.EmployeeCode,
            l.Mode,
            l.Reason,
            l.AssignedByUserId,
            l.Timestamp
        )).ToList();
    }

    public async Task<LeadDistributionMetricsDto> GetDistributionMetricsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var totalAssignedToday = await _context.LeadAssignmentLogs
            .CountAsync(l => l.Timestamp >= todayStart, cancellationToken);

        var totalAssignedThisMonth = await _context.LeadAssignmentLogs
            .CountAsync(l => l.Timestamp >= monthStart, cancellationToken);

        var totalUnassignedLeads = await _context.Leads
            .CountAsync(l => l.AssignedEmployeeId == null && l.Status != LeadStatus.Converted && l.Status != LeadStatus.Unqualified, cancellationToken);

        var modeCounts = await _context.LeadAssignmentLogs
            .Where(l => l.Timestamp >= monthStart)
            .GroupBy(l => l.Mode)
            .Select(g => new { Mode = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Mode, x => x.Count, cancellationToken);

        var employees = await _context.Employees
            .Include(e => e.Team)
            .Include(e => e.User)
            .Where(e => e.IsActive)
            .ToListAsync(cancellationToken);

        var activeLeadsPerRep = await _context.Leads
            .Where(l => l.AssignedEmployeeId != null && l.Status != LeadStatus.Converted && l.Status != LeadStatus.Unqualified)
            .GroupBy(l => l.AssignedEmployeeId!.Value)
            .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EmployeeId, x => x.Count, cancellationToken);

        var repWorkloads = employees.Select(e =>
        {
            var activeCount = activeLeadsPerRep.TryGetValue(e.Id, out var c) ? c : 0;
            var maxCap = 50; // standard default capacity
            var utilPercent = maxCap > 0 ? Math.Round((decimal)activeCount / maxCap * 100m, 1) : 0m;

            return new RepWorkloadDto(
                e.Id,
                e.User?.FullName ?? e.EmployeeCode,
                e.EmployeeCode,
                e.Team?.Name,
                activeCount,
                maxCap,
                utilPercent,
                activeCount < maxCap
            );
        }).OrderByDescending(r => r.ActiveLeadsCount).ToList();

        return new LeadDistributionMetricsDto(
            totalAssignedToday,
            totalAssignedThisMonth,
            totalUnassignedLeads,
            modeCounts,
            repWorkloads
        );
    }

    private List<Employee> FilterCandidateEmployees(List<Employee> allEmployees, LeadAssignmentRule rule)
    {
        if (string.IsNullOrWhiteSpace(rule.TargetEmployeeIdsJson) || rule.TargetEmployeeIdsJson == "[]")
            return allEmployees;

        try
        {
            var ids = JsonSerializer.Deserialize<List<Guid>>(rule.TargetEmployeeIdsJson);
            if (ids != null && ids.Any())
            {
                return allEmployees.Where(e => ids.Contains(e.Id)).ToList();
            }
        }
        catch { }

        return allEmployees;
    }
}
