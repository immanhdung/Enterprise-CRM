using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/admin/rbac")]
[Authorize]
public class RbacAdministrationController : BaseApiController
{
    private readonly IRbacAdministrationService _rbacService;

    public RbacAdministrationController(IRbacAdministrationService rbacService)
    {
        _rbacService = rbacService;
    }

    [HttpGet("roles")]
    [HasPermission(Permissions.RolesView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoleDto>>>> GetRoles(CancellationToken ct)
    {
        var roles = await _rbacService.GetRolesAsync(ct);
        return OkResponse(roles);
    }

    [HttpGet("roles/{id:guid}")]
    [HasPermission(Permissions.RolesView)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetRoleById(Guid id, CancellationToken ct)
    {
        var role = await _rbacService.GetRoleByIdAsync(id, ct);
        if (role == null) return FailResponse<RoleDto>("Role not found.", StatusCodes.Status404NotFound);
        return OkResponse(role);
    }

    [HttpPost("roles")]
    [HasPermission(Permissions.RolesManage)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> CreateRole([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        try
        {
            var role = await _rbacService.CreateRoleAsync(request, ct);
            return OkResponse(role, "Role created successfully.");
        }
        catch (Exception ex)
        {
            return FailResponse<RoleDto>(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("roles/{id:guid}")]
    [HasPermission(Permissions.RolesManage)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
    {
        try
        {
            var role = await _rbacService.UpdateRoleAsync(id, request, ct);
            return OkResponse(role, "Role updated successfully.");
        }
        catch (KeyNotFoundException ex)
        {
            return FailResponse<RoleDto>(ex.Message, StatusCodes.Status404NotFound);
        }
    }

    [HttpDelete("roles/{id:guid}")]
    [HasPermission(Permissions.RolesManage)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRole(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _rbacService.DeleteRoleAsync(id, ct);
            if (!result) return FailResponse<bool>("Role not found.", StatusCodes.Status404NotFound);
            return OkResponse(true, "Role deleted successfully.");
        }
        catch (Exception ex)
        {
            return FailResponse<bool>(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpGet("permissions")]
    [HasPermission(Permissions.RolesView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PermissionDto>>>> GetAllPermissions(CancellationToken ct)
    {
        var permissions = await _rbacService.GetAllPermissionsAsync(ct);
        return OkResponse(permissions);
    }

    [HttpGet("matrix")]
    [HasPermission(Permissions.RolesView)]
    public async Task<ActionResult<ApiResponse<RolePermissionMatrixDto>>> GetMatrix(CancellationToken ct)
    {
        var matrix = await _rbacService.GetPermissionMatrixAsync(ct);
        return OkResponse(matrix);
    }

    [HttpPost("matrix")]
    [HasPermission(Permissions.RolesManage)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateMatrixEntry([FromBody] UpdateRolePermissionMatrixRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _rbacService.UpdatePermissionMatrixEntryAsync(request, ct);
            return OkResponse(result, "Permission matrix updated.");
        }
        catch (KeyNotFoundException ex)
        {
            return FailResponse<bool>(ex.Message, StatusCodes.Status404NotFound);
        }
    }
}
