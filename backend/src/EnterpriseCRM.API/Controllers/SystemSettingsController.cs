using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/admin/settings")]
[Authorize]
public class SystemSettingsController : BaseApiController
{
    private readonly ISystemConfigurationService _configService;
    private readonly ICurrentUserService _currentUserService;

    public SystemSettingsController(ISystemConfigurationService configService, ICurrentUserService currentUserService)
    {
        _configService = configService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [HasPermission(Permissions.SettingsView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SystemConfigurationDto>>>> GetSettings([FromQuery] string? category, CancellationToken ct)
    {
        var settings = await _configService.GetAllSettingsAsync(category, ct);
        return OkResponse(settings);
    }

    [HttpPut]
    [HasPermission(Permissions.SettingsManage)]
    public async Task<ActionResult<ApiResponse<SystemConfigurationDto>>> UpdateSetting([FromBody] UpdateConfigurationRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var email = _currentUserService.Email ?? "admin";
        var result = await _configService.UpdateSettingAsync(request, userId, email, ct);
        return OkResponse(result, "Setting updated successfully.");
    }

    [HttpPost("bulk")]
    [HasPermission(Permissions.SettingsManage)]
    public async Task<ActionResult<ApiResponse<bool>>> BulkUpdate([FromBody] BulkUpdateConfigurationRequest request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var email = _currentUserService.Email ?? "admin";
        var result = await _configService.BulkUpdateSettingsAsync(request, userId, email, ct);
        return OkResponse(result, "Settings updated in bulk.");
    }

    [HttpGet("{id:guid}/history")]
    [HasPermission(Permissions.SettingsView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ConfigurationHistoryDto>>>> GetHistory(Guid id, CancellationToken ct)
    {
        var history = await _configService.GetSettingHistoryAsync(id, ct);
        return OkResponse(history);
    }
}
