using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize]
public class UserAdministrationController : BaseApiController
{
    private readonly IUserAdministrationService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UserAdministrationController(IUserAdministrationService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [HasPermission(Permissions.UsersView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserSummaryDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] Guid? departmentId,
        [FromQuery] string? role,
        CancellationToken ct)
    {
        var users = await _userService.GetAllUsersAsync(search, status, departmentId, role, ct);
        return OkResponse(users);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.UsersView)]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> GetById(Guid id, CancellationToken ct)
    {
        var user = await _userService.GetUserByIdAsync(id, ct);
        if (user == null) return FailResponse<UserDetailDto>("User not found.", StatusCodes.Status404NotFound);
        return OkResponse(user);
    }

    [HttpPost]
    [HasPermission(Permissions.UsersCreate)]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request, ct);
            return OkResponse(user, "User created successfully.");
        }
        catch (Exception ex)
        {
            return FailResponse<UserDetailDto>(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.UsersEdit)]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, request, ct);
            return OkResponse(user, "User updated successfully.");
        }
        catch (KeyNotFoundException ex)
        {
            return FailResponse<UserDetailDto>(ex.Message, StatusCodes.Status404NotFound);
        }
    }

    [HttpPatch("{id:guid}/status")]
    [HasPermission(Permissions.UsersEdit)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(Guid id, [FromBody] UpdateUserStatusRequest request, CancellationToken ct)
    {
        var result = await _userService.UpdateUserStatusAsync(id, request.Status, ct);
        if (!result) return FailResponse<bool>("User not found.", StatusCodes.Status404NotFound);
        return OkResponse(true, $"User status updated to {request.Status}.");
    }

    [HttpPost("{id:guid}/reset-password")]
    [HasPermission(Permissions.UsersEdit)]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword(Guid id, [FromBody] AdminResetPasswordRequest request, CancellationToken ct)
    {
        var result = await _userService.AdminResetPasswordAsync(id, request.NewPassword, ct);
        if (!result) return FailResponse<bool>("User not found.", StatusCodes.Status404NotFound);
        return OkResponse(true, "User password has been reset successfully.");
    }

    [HttpPost("{id:guid}/roles")]
    [HasPermission(Permissions.UsersManageRoles)]
    public async Task<ActionResult<ApiResponse<bool>>> AssignRoles(Guid id, [FromBody] AssignUserRolesRequest request, CancellationToken ct)
    {
        var result = await _userService.AssignRolesAsync(id, request.Roles, ct);
        if (!result) return FailResponse<bool>("User not found.", StatusCodes.Status404NotFound);
        return OkResponse(true, "Roles updated successfully.");
    }

    [HttpGet("sessions")]
    [HasPermission(Permissions.UsersView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserSessionDto>>>> GetSessions([FromQuery] Guid? userId, CancellationToken ct)
    {
        var sessions = await _userService.GetUserSessionsAsync(userId, ct);
        return OkResponse(sessions);
    }

    [HttpPost("sessions/{sessionId:guid}/revoke")]
    [HasPermission(Permissions.UsersEdit)]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeSession(Guid sessionId, CancellationToken ct)
    {
        var result = await _userService.RevokeSessionAsync(sessionId, ct);
        if (!result) return FailResponse<bool>("Session not found.", StatusCodes.Status404NotFound);
        return OkResponse(true, "Session revoked successfully.");
    }
}
