using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.AutomationAndAi;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[Authorize]
[Route("api/v1/dashboards")]
public class DashboardsController : BaseApiController
{
    private readonly IRoleDashboardService _roleDashboardService;

    public DashboardsController(IRoleDashboardService roleDashboardService)
    {
        _roleDashboardService = roleDashboardService;
    }

    [HttpGet("sales")]
    public async Task<ActionResult<ApiResponse<SalesDashboardDto>>> GetSalesDashboard(
        [FromQuery] Guid? employeeId = null,
        CancellationToken ct = default)
    {
        var result = await _roleDashboardService.GetSalesDashboardAsync(employeeId, ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod},{SystemRoles.Manager}")]
    [HttpGet("manager")]
    public async Task<ActionResult<ApiResponse<ManagerDashboardDto>>> GetManagerDashboard(
        [FromQuery] Guid? managerId = null,
        CancellationToken ct = default)
    {
        var result = await _roleDashboardService.GetManagerDashboardAsync(managerId, ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.CeoBod}")]
    [HttpGet("executive")]
    public async Task<ActionResult<ApiResponse<ExecutiveDashboardDto>>> GetExecutiveDashboard(CancellationToken ct = default)
    {
        var result = await _roleDashboardService.GetExecutiveDashboardAsync(ct);
        return OkResponse(result);
    }
}
