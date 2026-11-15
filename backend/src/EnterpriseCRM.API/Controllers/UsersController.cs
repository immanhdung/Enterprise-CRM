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
[Route("api/v1/users")]
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IApplicationDbContext _context;

    public UsersController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [HasPermission(Permissions.UsersView)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserManagementDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.EmployeeProfile!)
                .ThenInclude(e => e.Department)
            .AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s) ||
                u.Email.ToLower().Contains(s));
        }

        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new UserManagementDto(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.FullName,
                u.IsActive,
                u.LastLoginAt,
                u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                u.EmployeeProfile != null ? u.EmployeeProfile.Department.Name : null,
                u.EmployeeProfile != null ? u.EmployeeProfile.Position : null,
                u.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<UserManagementDto>>.SuccessResponse(users));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.UsersView)]
    [ProducesResponseType(typeof(ApiResponse<UserManagementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserManagementDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var u = await _context.Users
            .Include(x => x.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(x => x.EmployeeProfile!)
                .ThenInclude(e => e.Department)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (u == null)
        {
            return NotFound(ApiResponse<UserManagementDto>.FailureResponse("User not found.", 404));
        }

        var dto = new UserManagementDto(
            u.Id,
            u.Email,
            u.FirstName,
            u.LastName,
            u.FullName,
            u.IsActive,
            u.LastLoginAt,
            u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            u.EmployeeProfile?.Department?.Name,
            u.EmployeeProfile?.Position,
            u.CreatedAt
        );

        return Ok(ApiResponse<UserManagementDto>.SuccessResponse(dto));
    }

    [HttpPost("{id:guid}/roles")]
    [HasPermission(Permissions.UsersManageRoles)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignRolesRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("User not found.", 404));
        }

        // Remove current roles
        _context.UserRoles.RemoveRange(user.UserRoles);

        // Add requested roles
        if (request.Roles != null && request.Roles.Any())
        {
            var rolesToAdd = await _context.Roles
                .Where(r => request.Roles.Contains(r.Name))
                .ToListAsync(cancellationToken);

            foreach (var r in rolesToAdd)
            {
                _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = r.Id });
            }
        }

        // Invalidate active refresh token to force token refresh with updated roles
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Roles assigned successfully."));
    }

    [HttpPatch("{id:guid}/status")]
    [HasPermission(Permissions.UsersEdit)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromQuery] bool isActive, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null)
        {
            return NotFound(ApiResponse<bool>.FailureResponse("User not found.", 404));
        }

        user.IsActive = isActive;
        if (!isActive)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(true, $"User {(isActive ? "activated" : "deactivated")} successfully."));
    }

    [HttpGet("roles")]
    [HasPermission(Permissions.RolesView)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<object>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                r.IsSystem,
                Permissions = r.RolePermissions.Select(rp => rp.Permission.Code).ToList()
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<object>>.SuccessResponse(roles));
    }
}
