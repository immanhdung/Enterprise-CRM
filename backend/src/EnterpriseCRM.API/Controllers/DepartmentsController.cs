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
[Route("api/v1/departments")]
[Authorize]
public class DepartmentsController : BaseApiController
{
    private readonly IApplicationDbContext _context;

    public DepartmentsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [HasPermission(Permissions.DepartmentsView)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DepartmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var departments = await _context.Departments
            .Include(d => d.Employees)
            .Include(d => d.Teams)
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto(
                d.Id,
                d.Code,
                d.Name,
                d.Description,
                d.ManagerId,
                d.Employees.Where(e => e.Id == d.ManagerId).Select(e => e.User.FullName).FirstOrDefault(),
                d.Employees.Count(e => !e.IsDeleted),
                d.Teams.Count(t => !t.IsDeleted),
                d.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<DepartmentDto>>.SuccessResponse(departments));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.DepartmentsView)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dept = await _context.Departments
            .Include(d => d.Employees)
            .Include(d => d.Teams)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (dept == null)
        {
            return NotFound(ApiResponse<DepartmentDto>.FailureResponse("Department not found.", 404));
        }

        var managerName = dept.ManagerId.HasValue
            ? await _context.Employees.Where(e => e.Id == dept.ManagerId).Select(e => e.User.FullName).FirstOrDefaultAsync(cancellationToken)
            : null;

        var dto = new DepartmentDto(
            dept.Id,
            dept.Code,
            dept.Name,
            dept.Description,
            dept.ManagerId,
            managerName,
            dept.Employees.Count(e => !e.IsDeleted),
            dept.Teams.Count(t => !t.IsDeleted),
            dept.CreatedAt
        );

        return Ok(ApiResponse<DepartmentDto>.SuccessResponse(dto));
    }

    [HttpPost]
    [HasPermission(Permissions.DepartmentsManage)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var exists = await _context.Departments.AnyAsync(d => d.Code.ToUpper() == request.Code.ToUpper(), cancellationToken);
        if (exists)
        {
            return BadRequest(ApiResponse<DepartmentDto>.FailureResponse("Department code already exists.", 400));
        }

        var dept = new Department
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            ManagerId = request.ManagerId
        };

        _context.Departments.Add(dept);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new DepartmentDto(dept.Id, dept.Code, dept.Name, dept.Description, dept.ManagerId, null, 0, 0, dept.CreatedAt);
        return CreatedAtAction(nameof(GetById), new { id = dept.Id }, ApiResponse<DepartmentDto>.SuccessResponse(dto, "Department created successfully.", 201));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.DepartmentsManage)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var dept = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (dept == null)
        {
            return NotFound(ApiResponse<DepartmentDto>.FailureResponse("Department not found.", 404));
        }

        dept.Name = request.Name.Trim();
        dept.Description = request.Description?.Trim();
        dept.ManagerId = request.ManagerId;

        await _context.SaveChangesAsync(cancellationToken);

        var count = await _context.Employees.CountAsync(e => e.DepartmentId == id, cancellationToken);
        var teamCount = await _context.Teams.CountAsync(t => t.DepartmentId == id, cancellationToken);
        var managerName = dept.ManagerId.HasValue
            ? await _context.Employees.Where(e => e.Id == dept.ManagerId).Select(e => e.User.FullName).FirstOrDefaultAsync(cancellationToken)
            : null;

        var dto = new DepartmentDto(dept.Id, dept.Code, dept.Name, dept.Description, dept.ManagerId, managerName, count, teamCount, dept.CreatedAt);
        return Ok(ApiResponse<DepartmentDto>.SuccessResponse(dto, "Department updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.DepartmentsManage)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var dept = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (dept == null)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Department not found.", 404));
        }

        var hasEmployees = await _context.Employees.AnyAsync(e => e.DepartmentId == id, cancellationToken);
        if (hasEmployees)
        {
            return BadRequest(ApiResponse<bool>.FailureResponse("Cannot delete department with active employees. Reassign employees first.", 400));
        }

        dept.IsDeleted = true;
        dept.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Department deleted successfully."));
    }
}
