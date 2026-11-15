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
[Route("api/v1/employees")]
[Authorize]
public class EmployeesController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public EmployeesController(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    [HasPermission(Permissions.EmployeesView)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EmployeeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? teamId,
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var query = _context.Employees
            .Include(e => e.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .Include(e => e.Department)
            .Include(e => e.Team)
            .AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(e => e.DepartmentId == departmentId.Value);
        }

        if (teamId.HasValue)
        {
            query = query.Where(e => e.TeamId == teamId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(e => e.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(e =>
                e.User.FirstName.ToLower().Contains(s) ||
                e.User.LastName.ToLower().Contains(s) ||
                e.User.Email.ToLower().Contains(s) ||
                e.EmployeeCode.ToLower().Contains(s) ||
                e.Position.ToLower().Contains(s));
        }

        var employees = await query
            .OrderBy(e => e.User.LastName)
            .ThenBy(e => e.User.FirstName)
            .Select(e => new EmployeeDto(
                e.Id,
                e.UserId,
                e.User.FullName,
                e.User.Email,
                e.EmployeeCode,
                e.Position,
                e.Phone,
                e.HireDate,
                e.BaseSalary,
                e.IsActive,
                e.DepartmentId,
                e.Department.Name,
                e.TeamId,
                e.Team != null ? e.Team.Name : null,
                e.User.UserRoles.Select(ur => ur.Role.Name).ToList(),
                e.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<EmployeeDto>>.SuccessResponse(employees));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.EmployeesView)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var e = await _context.Employees
            .Include(x => x.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .Include(x => x.Department)
            .Include(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (e == null)
        {
            return NotFound(ApiResponse<EmployeeDto>.FailureResponse("Employee not found.", 404));
        }

        var dto = new EmployeeDto(
            e.Id,
            e.UserId,
            e.User.FullName,
            e.User.Email,
            e.EmployeeCode,
            e.Position,
            e.Phone,
            e.HireDate,
            e.BaseSalary,
            e.IsActive,
            e.DepartmentId,
            e.Department.Name,
            e.TeamId,
            e.Team?.Name,
            e.User.UserRoles.Select(ur => ur.Role.Name).ToList(),
            e.CreatedAt
        );

        return Ok(ApiResponse<EmployeeDto>.SuccessResponse(dto));
    }

    [HttpPost]
    [HasPermission(Permissions.EmployeesCreate)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == email, cancellationToken);
        if (emailExists)
        {
            return BadRequest(ApiResponse<EmployeeDto>.FailureResponse("Email address is already in use.", 400));
        }

        var codeExists = await _context.Employees.AnyAsync(e => e.EmployeeCode.ToUpper() == request.EmployeeCode.ToUpper(), cancellationToken);
        if (codeExists)
        {
            return BadRequest(ApiResponse<EmployeeDto>.FailureResponse("Employee code is already in use.", 400));
        }

        var (hash, salt) = _passwordHasher.HashPassword(request.Password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hash,
            Salt = salt,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IsActive = true
        };
        _context.Users.Add(user);

        // Assign Roles
        if (request.Roles != null && request.Roles.Any())
        {
            var validRoles = await _context.Roles.Where(r => request.Roles.Contains(r.Name)).ToListAsync(cancellationToken);
            foreach (var r in validRoles)
            {
                _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = r.Id });
            }
        }

        var emp = new Employee
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DepartmentId = request.DepartmentId,
            TeamId = request.TeamId,
            EmployeeCode = request.EmployeeCode.Trim().ToUpperInvariant(),
            Position = request.Position.Trim(),
            Phone = request.Phone.Trim(),
            HireDate = request.HireDate,
            BaseSalary = request.BaseSalary,
            IsActive = true
        };
        _context.Employees.Add(emp);

        await _context.SaveChangesAsync(cancellationToken);

        var dept = await _context.Departments.FindAsync(new object[] { request.DepartmentId }, cancellationToken);
        var team = request.TeamId.HasValue ? await _context.Teams.FindAsync(new object[] { request.TeamId.Value }, cancellationToken) : null;

        var dto = new EmployeeDto(
            emp.Id,
            user.Id,
            user.FullName,
            user.Email,
            emp.EmployeeCode,
            emp.Position,
            emp.Phone,
            emp.HireDate,
            emp.BaseSalary,
            emp.IsActive,
            emp.DepartmentId,
            dept?.Name ?? "",
            emp.TeamId,
            team?.Name,
            request.Roles ?? new List<string>(),
            emp.CreatedAt
        );

        return CreatedAtAction(nameof(GetById), new { id = emp.Id }, ApiResponse<EmployeeDto>.SuccessResponse(dto, "Employee created successfully.", 201));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.EmployeesEdit)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var emp = await _context.Employees
            .Include(e => e.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .Include(e => e.Department)
            .Include(e => e.Team)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (emp == null)
        {
            return NotFound(ApiResponse<EmployeeDto>.FailureResponse("Employee not found.", 404));
        }

        emp.User.FirstName = request.FirstName.Trim();
        emp.User.LastName = request.LastName.Trim();
        emp.Position = request.Position.Trim();
        emp.Phone = request.Phone.Trim();
        emp.HireDate = request.HireDate;
        emp.BaseSalary = request.BaseSalary;
        emp.DepartmentId = request.DepartmentId;
        emp.TeamId = request.TeamId;
        emp.IsActive = request.IsActive;
        emp.User.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        var dept = await _context.Departments.FindAsync(new object[] { request.DepartmentId }, cancellationToken);
        var team = request.TeamId.HasValue ? await _context.Teams.FindAsync(new object[] { request.TeamId.Value }, cancellationToken) : null;

        var dto = new EmployeeDto(
            emp.Id,
            emp.UserId,
            emp.User.FullName,
            emp.User.Email,
            emp.EmployeeCode,
            emp.Position,
            emp.Phone,
            emp.HireDate,
            emp.BaseSalary,
            emp.IsActive,
            emp.DepartmentId,
            dept?.Name ?? "",
            emp.TeamId,
            team?.Name,
            emp.User.UserRoles.Select(ur => ur.Role.Name).ToList(),
            emp.CreatedAt
        );

        return Ok(ApiResponse<EmployeeDto>.SuccessResponse(dto, "Employee updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.EmployeesDelete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var emp = await _context.Employees.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (emp == null)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("Employee not found.", 404));
        }

        emp.IsActive = false;
        emp.IsDeleted = true;
        emp.DeletedAt = DateTime.UtcNow;

        emp.User.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Employee deactivated successfully."));
    }
}
