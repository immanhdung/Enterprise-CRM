using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Organization;
using EnterpriseCRM.Domain.Constants;
using EnterpriseCRM.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/teams")]
[Authorize]
public class TeamsController : BaseApiController
{
    private readonly IApplicationDbContext _context;

    public TeamsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [HasPermission(Permissions.TeamsView)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TeamDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? departmentId, CancellationToken cancellationToken)
    {
        var query = _context.Teams
            .Include(t => t.Department)
            .Include(t => t.Members)
            .AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(t => t.DepartmentId == departmentId.Value);
        }

        var teams = await query
            .OrderBy(t => t.Name)
            .Select(t => new TeamDto(
                t.Id,
                t.DepartmentId,
                t.Department.Name,
                t.Name,
                t.TeamLeaderId,
                t.Members.Where(m => m.Id == t.TeamLeaderId).Select(m => m.User.FullName).FirstOrDefault(),
                t.Members.Count(m => !m.IsDeleted),
                t.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<TeamDto>>.SuccessResponse(teams));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.TeamsView)]
    [ProducesResponseType(typeof(ApiResponse<TeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TeamDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var team = await _context.Teams
            .Include(t => t.Department)
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team == null)
        {
            return NotFound(ApiResponse<TeamDto>.FailureResponse("Team not found.", 404));
        }

        var leaderName = team.TeamLeaderId.HasValue
            ? await _context.Employees.Where(e => e.Id == team.TeamLeaderId).Select(e => e.User.FullName).FirstOrDefaultAsync(cancellationToken)
            : null;

        var dto = new TeamDto(
            team.Id,
            team.DepartmentId,
            team.Department.Name,
            team.Name,
            team.TeamLeaderId,
            leaderName,
            team.Members.Count(m => !m.IsDeleted),
            team.CreatedAt
        );

        return Ok(ApiResponse<TeamDto>.SuccessResponse(dto));
    }

    [HttpPost]
    [HasPermission(Permissions.TeamsManage)]
    [ProducesResponseType(typeof(ApiResponse<TeamDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TeamDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTeamRequest request, CancellationToken cancellationToken)
    {
        var deptExists = await _context.Departments.AnyAsync(d => d.Id == request.DepartmentId, cancellationToken);
        if (!deptExists)
        {
            return BadRequest(ApiResponse<TeamDto>.FailureResponse("Department does not exist.", 400));
        }

        var team = new Team
        {
            Id = Guid.NewGuid(),
            DepartmentId = request.DepartmentId,
            Name = request.Name.Trim(),
            TeamLeaderId = request.TeamLeaderId
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync(cancellationToken);

        var dept = await _context.Departments.FindAsync(new object[] { request.DepartmentId }, cancellationToken);
        var dto = new TeamDto(team.Id, team.DepartmentId, dept?.Name ?? "", team.Name, team.TeamLeaderId, null, 0, team.CreatedAt);

        return CreatedAtAction(nameof(GetById), new { id = team.Id }, ApiResponse<TeamDto>.SuccessResponse(dto, "Team created successfully.", 201));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.TeamsManage)]
    [ProducesResponseType(typeof(ApiResponse<TeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TeamDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeamRequest request, CancellationToken cancellationToken)
    {
        var team = await _context.Teams
            .Include(t => t.Department)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team == null)
        {
            return NotFound(ApiResponse<TeamDto>.FailureResponse("Team not found.", 404));
        }

        team.Name = request.Name.Trim();
        team.TeamLeaderId = request.TeamLeaderId;

        await _context.SaveChangesAsync(cancellationToken);

        var memberCount = await _context.Employees.CountAsync(e => e.TeamId == id, cancellationToken);
        var leaderName = team.TeamLeaderId.HasValue
            ? await _context.Employees.Where(e => e.Id == team.TeamLeaderId).Select(e => e.User.FullName).FirstOrDefaultAsync(cancellationToken)
            : null;

        var dto = new TeamDto(team.Id, team.DepartmentId, team.Department.Name, team.Name, team.TeamLeaderId, leaderName, memberCount, team.CreatedAt);
        return Ok(ApiResponse<TeamDto>.SuccessResponse(dto, "Team updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.TeamsManage)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (team == null)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Team not found.", 404));
        }

        team.IsDeleted = true;
        team.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Team deleted successfully."));
    }
}
