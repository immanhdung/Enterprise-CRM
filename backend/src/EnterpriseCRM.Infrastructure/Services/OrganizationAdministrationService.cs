using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class OrganizationAdministrationService : IOrganizationAdministrationService
{
    private readonly IApplicationDbContext _context;

    public OrganizationAdministrationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DepartmentHierarchyDto>> GetHierarchyAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _context.Departments
            .Include(d => d.Teams)
                .ThenInclude(t => t.Members)
                    .ThenInclude(m => m.User)
            .Include(d => d.Employees)
                .ThenInclude(e => e.User)
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        var allUsers = await _context.Users.AsNoTracking().ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        var result = new List<DepartmentHierarchyDto>();

        foreach (var d in departments)
        {
            string? mgrName = null;
            if (d.ManagerId.HasValue && allUsers.TryGetValue(d.ManagerId.Value, out var mName))
            {
                mgrName = mName;
            }

            var teamDtos = new List<TeamHierarchyDto>();
            foreach (var t in d.Teams)
            {
                string? leaderName = null;
                if (t.TeamLeaderId.HasValue && allUsers.TryGetValue(t.TeamLeaderId.Value, out var lName))
                {
                    leaderName = lName;
                }

                var memberDtos = t.Members
                    .Where(m => m.IsActive)
                    .Select(m => new EmployeeHierarchyDto(
                        m.Id,
                        m.UserId,
                        m.User.FullName,
                        m.User.Email,
                        m.EmployeeCode,
                        m.Position,
                        m.Phone,
                        m.IsActive,
                        t.Id,
                        t.Name
                    ))
                    .ToList();

                teamDtos.Add(new TeamHierarchyDto(
                    t.Id,
                    t.Name,
                    t.TeamLeaderId,
                    leaderName,
                    memberDtos.Count,
                    memberDtos
                ));
            }

            // Direct employees (not assigned to any team in department)
            var directDtos = d.Employees
                .Where(e => e.TeamId == null && e.IsActive)
                .Select(e => new EmployeeHierarchyDto(
                    e.Id,
                    e.UserId,
                    e.User.FullName,
                    e.User.Email,
                    e.EmployeeCode,
                    e.Position,
                    e.Phone,
                    e.IsActive,
                    null,
                    null
                ))
                .ToList();

            int totalEmployees = d.Employees.Count(e => e.IsActive);

            result.Add(new DepartmentHierarchyDto(
                d.Id,
                d.Code,
                d.Name,
                d.Description,
                d.ManagerId,
                mgrName,
                totalEmployees,
                teamDtos,
                directDtos
            ));
        }

        return result;
    }

    public async Task<OrganizationChangeHistoryDto> TransferEmployeeAsync(
        TransferEmployeeRequest request,
        Guid approvedByUserId,
        CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Team)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID '{request.EmployeeId}' was not found.");
        }

        var newDept = await _context.Departments.FirstOrDefaultAsync(d => d.Id == request.NewDepartmentId, cancellationToken);
        if (newDept == null)
        {
            throw new KeyNotFoundException($"Target Department with ID '{request.NewDepartmentId}' was not found.");
        }

        Team? newTeam = null;
        if (request.NewTeamId.HasValue)
        {
            newTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Id == request.NewTeamId.Value && t.DepartmentId == request.NewDepartmentId, cancellationToken);
            if (newTeam == null)
            {
                throw new KeyNotFoundException($"Target Team with ID '{request.NewTeamId}' was not found in department '{newDept.Name}'.");
            }
        }

        // Determine Change Type
        string changeType = "TransferDepartment";
        if (employee.DepartmentId == request.NewDepartmentId && employee.TeamId != request.NewTeamId)
        {
            changeType = "TransferTeam";
        }
        else if (request.NewBaseSalary.HasValue && request.NewBaseSalary.Value > employee.BaseSalary)
        {
            changeType = "Promotion";
        }

        var history = new OrganizationChangeHistory
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            ChangeType = changeType,
            OldDepartmentId = employee.DepartmentId,
            OldDepartmentName = employee.Department.Name,
            NewDepartmentId = newDept.Id,
            NewDepartmentName = newDept.Name,
            OldTeamId = employee.TeamId,
            OldTeamName = employee.Team?.Name,
            NewTeamId = newTeam?.Id,
            NewTeamName = newTeam?.Name,
            OldPosition = employee.Position,
            NewPosition = request.NewPosition ?? employee.Position,
            OldBaseSalary = employee.BaseSalary,
            NewBaseSalary = request.NewBaseSalary ?? employee.BaseSalary,
            EffectiveDate = request.EffectiveDate,
            Reason = request.Reason,
            ApprovedByUserId = approvedByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.OrganizationChangeHistories.Add(history);

        // Update employee current state
        employee.DepartmentId = newDept.Id;
        employee.TeamId = newTeam?.Id;
        if (!string.IsNullOrWhiteSpace(request.NewPosition))
        {
            employee.Position = request.NewPosition.Trim();
        }
        if (request.NewBaseSalary.HasValue)
        {
            employee.BaseSalary = request.NewBaseSalary.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new OrganizationChangeHistoryDto(
            history.Id,
            employee.Id,
            employee.User.FullName,
            history.ChangeType,
            history.OldDepartmentName,
            history.NewDepartmentName,
            history.OldTeamName,
            history.NewTeamName,
            history.OldPosition,
            history.NewPosition,
            history.OldBaseSalary,
            history.NewBaseSalary,
            history.EffectiveDate,
            history.Reason,
            history.CreatedAtUtc
        );
    }

    public async Task<IReadOnlyList<OrganizationChangeHistoryDto>> GetChangeHistoryAsync(Guid? employeeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.OrganizationChangeHistories
            .Include(h => h.Employee)
                .ThenInclude(e => e.User)
            .AsNoTracking()
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(h => h.EmployeeId == employeeId.Value);
        }

        return await query
            .OrderByDescending(h => h.CreatedAtUtc)
            .Take(100)
            .Select(h => new OrganizationChangeHistoryDto(
                h.Id,
                h.EmployeeId,
                h.Employee.User.FullName,
                h.ChangeType,
                h.OldDepartmentName,
                h.NewDepartmentName,
                h.OldTeamName,
                h.NewTeamName,
                h.OldPosition,
                h.NewPosition,
                h.OldBaseSalary,
                h.NewBaseSalary,
                h.EffectiveDate,
                h.Reason,
                h.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);
    }
}
